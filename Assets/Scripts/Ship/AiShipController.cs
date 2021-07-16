using System;
using System.Collections.Generic;
using Pathfinding;
using PhysicsUtilities;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Ship {
    public class AiShipController : MonoBehaviour, IShipController {
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

        private float _nextSeekTime = float.MinValue;

        private void Awake() {
            _controlledShip = GetComponent<ShipMain>();
            _seeker = gameObject.AddComponent<Seeker>();
            
            RaycastModifier raycastModifier = gameObject.AddComponent<RaycastModifier>();
            raycastModifier.quality = RaycastModifier.Quality.Highest;
        }

        private void Update() {
            UpdateMovementInput();
        }

        public ShipGearInput GetVerticalInput() {
            return _currentGearInput;
            /*if (_currentGunTarget != null) {
                Vector3 targetPos = _shipNavigatorAgent.transform.position;
                Transform myTransform = _controlledShip.transform;
                Vector3 targetDir = (targetPos - myTransform.position).normalized;

                float dot = Vector3.Dot(targetDir, myTransform.forward);
                if (dot >= 0) {
                    return ShipGearInput.Raise;
                }
                else {
                    return ShipGearInput.Lower;
                }
            }
            else {
                return ShipGearInput.None;
            }*/
        }

        public float GetHorizontalInput() {
            return _currentRudderInput;

        }

        public Vector3 GetAimPoint() {
            if (Time.time >= _nextSeekTime) {
                _currentGunTarget = SeekTarget();

                if (_currentGunTarget != null) {
                    RefreshNavigationPath();
                }
                
            }

            if (_currentGunTarget != null) {
                GunImpactPrediction prediction = PredictPosition(_currentGunTarget);
                if (prediction.willImpact) {
                    _currentAimPoint = prediction.impactPosition;
                }
            }

            return _currentAimPoint;
        }

        private void UpdateMovementInput() {
            if (_path == null) {
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

        private void RefreshNavigationPath() {
            _seeker.StartPath(transform.position, _currentGunTarget.transform.position, OnPathComplete);
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

        public bool GetFireInput() {
            return _currentGunTarget != null;
        }

        public bool GetTorpedoInput() {
            return false; // Not yet implemented
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

            _nextSeekTime = Time.time + Random.Range(2.5f, 3.5f);

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

        private void OnDestroy() {
            Destroy(_seeker);
        }
    }
}