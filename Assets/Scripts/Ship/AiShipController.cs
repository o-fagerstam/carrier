using System.Collections.Generic;
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
        
        private ShipMain _currentGunTarget;
        private Vector3 _currentAimPoint;

        private ShipGearInput _currentGearInput = ShipGearInput.None;
        private float _currentRudderInput;

        private float _lastPathUpdateTime = float.MinValue;
        private const float PathUpdateFrequency = 5f;

        protected override void Awake() {
            base.Awake();
            _controlledShip = GetComponent<ShipMain>();
            _seeker = gameObject.AddComponent<Seeker>();
            
            RaycastModifier raycastModifier = gameObject.AddComponent<RaycastModifier>();
            raycastModifier.quality = RaycastModifier.Quality.Highest;
        }

        protected override void Update() {
            base.Update();
            UpdateMovementInput();
        }

        public ShipGearInput GetVerticalInput() {
            return _currentGearInput;
        }

        public float GetHorizontalInput() {
            return _currentRudderInput;
        }

        public Vector3 GetAimPoint() {
            if (_currentGunTarget != null) {
                GunImpactPrediction prediction = PredictPosition(_currentGunTarget);
                if (prediction.willImpact) {
                    _currentAimPoint = prediction.impactPosition;
                }
            }

            return _currentAimPoint;
        }
        
        public bool GetFireInput() {
            return _currentGunTarget != null;
        }

        public bool GetTorpedoInput() {
            return false; // Not yet implemented
        }

        private void UpdateMovementInput() {
            if ( _path == null) {
                _currentGearInput = ShipGearInput.None;
                _currentRudderInput = 0f;
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
            float newRudderInput = Mathf.InverseLerp(
                -_controlledShip.maxSteerAngle,
                _controlledShip.maxSteerAngle,
                steeringAngle
            ) * 2f - 1f;

            
            float dotProduct = Vector3.Dot(transform.forward, moveDirection);
            if (dotProduct >= 0f) {
                // Forward movement
                _currentGearInput = ShipGearInput.Raise;
                _currentRudderInput = newRudderInput;
            }
            else {
                // Backwards movement
                _currentGearInput = ShipGearInput.Lower;
                if (squareDistanceToWaypoint > MaxReverseDistanceSquared) {
                    // Far away, make T turn
                    _currentRudderInput = -newRudderInput;
                }
                else {
                    // Close by, reverse to waypoint
                    _currentRudderInput = newRudderInput;
                }
            }
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
            HashSet<ShipMain> allShips = VehiclesManager.Instance.AllShips;
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

        private GunImpactPrediction PredictPosition(ShipMain target) {
            ShipGun tracingGun = _controlledShip.MainGuns[0];
            Vector3 deltaPosition = target.transform.position - _controlledShip.transform.position;
            
            bool hasValidAngle = ProjectileMotion.FiringAngle(
                deltaPosition,
                tracingGun.muzzleVelocity,
                out float angle,
                tracingGun.minElevation,
                tracingGun.maxElevation
            );
            if (!hasValidAngle) {
                return new GunImpactPrediction(false, new Vector3());
            }
            
            float timeOfFlight = ProjectileMotion.ExpectedTimeOfFlight(angle, tracingGun.muzzleVelocity);
            Vector3 positionOffset = target.Rigidbody.velocity * timeOfFlight;
            
            return new GunImpactPrediction(true, target.transform.position + positionOffset);
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

        public override void IdleCommand() { }

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
    }
}