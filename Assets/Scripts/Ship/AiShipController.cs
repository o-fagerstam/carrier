using System.Collections.Generic;
using CommandMode;
using Pathfinding;
using PhysicsUtilities;
using UnityEngine;

namespace Ship {
    public class AiShipController : Unit.AiUnitController, IShipController {
        private const float NextWaypointDistanceSquared = 10f * 10f;
        private const float MaxReverseDistanceSquared = 100f * 100f;
        private const float SteeringSmoothFactor = 0.7f;
        
        private ShipMain _controlledShip;
        private Seeker _seeker;
        private Path _path;
        private int _currentWayPointIndex;
        private bool _reachedEndOfPath;
        
        private GameUnit _currentGunTarget;

        private float _lastPathUpdateTime = float.MinValue;
        private const float PathUpdateFrequency = 5f;
        
        private float _lastEnemySeekTime = float.MinValue;
        private const float EnemySeekFrequency = 5f;

        protected override void Awake() {
            base.Awake();
            _controlledShip = GetComponent<ShipMain>();
            _seeker = gameObject.AddComponent<Seeker>();
            
            RaycastModifier raycastModifier = gameObject.AddComponent<RaycastModifier>();
            raycastModifier.quality = RaycastModifier.Quality.Highest;
        }

        protected override void Update() {
            base.Update();
            if (_controlledShip.vehicleUserType != VehicleUserType.Ai || !_controlledShip.alive) {
                return;
            }
            UpdateMovementInput();
            UpdateAimInput();
        }

        public void UpdateAimInput() {
            if (_currentGunTarget == null) {
                return;
            }
            DesiredImpactPointPrediction prediction = MovementAdjustedFiringPrediction(_currentGunTarget);
            if (prediction.willImpact) {
                _controlledShip.ReceiveAimInput(prediction.desiredImpactPoint);
                _controlledShip.ReceiveFireInput();
            }
        }

        private void UpdateMovementInput() {
            if ( _path == null) {
                _controlledShip.ReceiveSteeringInput(ShipGearInput.Zero, 0f);
                return;
            }

            _reachedEndOfPath = false;

            Vector3 currentPosition = transform.position;
            float squareDistanceToWaypoint;
            while (true) {
                squareDistanceToWaypoint =
                    Vector3.SqrMagnitude(_path.vectorPath[_currentWayPointIndex] - currentPosition);
                if (squareDistanceToWaypoint < NextWaypointDistanceSquared) {
                    if (_currentWayPointIndex < _path.vectorPath.Count - 1) {
                        _currentWayPointIndex++;
                    }
                    else {
                        _reachedEndOfPath = true;
                        break;
                    }
                }
                else {
                    break;
                }
            }

            Vector3 targetWaypoint = _path.vectorPath[_currentWayPointIndex];
            Vector3 moveDirection = (targetWaypoint - currentPosition).normalized;
            float angleToDir = Vector3.SignedAngle(transform.forward, moveDirection, Vector3.up);
            float steeringAngle = Mathf.Clamp(
                angleToDir * SteeringSmoothFactor,
                -_controlledShip.maxSteerAngle,
                _controlledShip.maxSteerAngle
            );
            float rudderInput = Mathf.InverseLerp(
                -_controlledShip.maxSteerAngle,
                _controlledShip.maxSteerAngle,
                steeringAngle
            ) * 2f - 1f;

            ShipGearInput gearInput;
            float dotProduct = Vector3.Dot(transform.forward, moveDirection);
            if (dotProduct >= 0f) {
                // Forward movement
                gearInput = ShipGearInput.Raise;
            }
            else {
                // Backwards movement
                gearInput = ShipGearInput.Lower;
                if (squareDistanceToWaypoint > MaxReverseDistanceSquared) {
                    // Far away, make T turn
                    rudderInput = -rudderInput;
                }
            }
            _controlledShip.ReceiveSteeringInput(gearInput, rudderInput);
            
            
        }

        private void RecalculateNavPath(Vector3 targetPosition) {
            _lastPathUpdateTime = Time.time;
            _seeker.StartPath(transform.position, targetPosition, OnPathComplete);
        }

        public void OnPathComplete(Path p) {
            if (!p.error) {
                _path = p;
                _currentWayPointIndex = 0;
                _reachedEndOfPath = false;
            }
            else {
                Debug.Log("Pathing error: " + p.error);
            }
        }

        private ShipMain SeekTarget() {
            IReadOnlyCollection<ShipMain> allShips = VehiclesManager.AllShips;
            ShipMain closestShip = null;
            float closestShipDistance = float.MaxValue;
            foreach (ShipMain ship in allShips) {
                if (ship.team == _controlledShip.team) {
                    continue;
                }
                var distance = (ship.transform.position - _controlledShip.transform.position).magnitude;
                if (distance < closestShipDistance) {
                    closestShip = ship;
                    closestShipDistance = distance;
                }
            }

            return closestShip;
        }

        private DesiredImpactPointPrediction MovementAdjustedFiringPrediction(GameUnit target) {
            ShipGun tracingGun = _controlledShip.MainGuns[0];

            bool canPotentiallyHitTarget = tracingGun.CanPotentiallyHitPoint(target.transform.position, out float desiredAngle, out float timeOfFlight);
            if (!canPotentiallyHitTarget) {
                return new DesiredImpactPointPrediction(false, new Vector3());
            }
            
            Vector3 positionOffset = target.Rigidbody.velocity * timeOfFlight;

            return new DesiredImpactPointPrediction(true, target.transform.position + positionOffset);
        }

        protected override void OnDestroy() {
            base.OnDestroy();
            Destroy(_seeker);
        }
        
        /*
         * ORDERS
         */

        public override void OnNewCommand() {
            _lastPathUpdateTime = float.MinValue;
        }

        public override void IdleCommand() {
            if (Time.time - _lastEnemySeekTime > EnemySeekFrequency) {
                _lastEnemySeekTime = Time.time;
                GameUnit newTarget = SeekTarget();
                if (newTarget != null) {
                    AttackCommand attackCommand = new AttackCommand(this, newTarget);
                    SetCommand(attackCommand);
                }
            } else {
                _path = null;
                _currentGunTarget = null;
            }

        }

        public override void PointMoveCommand(Vector3 targetPoint) {
            if (Time.time - _lastPathUpdateTime > PathUpdateFrequency) {
                RecalculateNavPath(targetPoint);
            }
        }

        public override void FollowCommand(GameUnit unitToFollow) {
            if (Time.time - _lastPathUpdateTime > PathUpdateFrequency) {
                RecalculateNavPath(unitToFollow.transform.position);
            }
        }

        public override void AttackCommand(GameUnit target) {
            if (Time.time - _lastPathUpdateTime > PathUpdateFrequency) {
                RecalculateNavPath(target.transform.position);
            }
            _currentGunTarget = target;
        }
    }

    struct DesiredImpactPointPrediction {
        public bool willImpact;
        public Vector3 desiredImpactPoint;

        public DesiredImpactPointPrediction (bool willImpact, Vector3 desiredImpactPoint) {
            this.willImpact = willImpact;
            this.desiredImpactPoint = desiredImpactPoint;
        }
    }
}

