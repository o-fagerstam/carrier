using System.Collections.Generic;
using PhysicsUtilities;
using UnityEngine;

namespace Ship {
    public class AiShipController : ShipController {
        private readonly Ship _controlledShip;
        
        private Ship _currentGunTarget;
        private Vector3 currentAimPoint;
        
        private float _nextSeekTime = float.MinValue;

        public AiShipController(Ship controlledShip) {
            _controlledShip = controlledShip;
        }

        public float GetVerticalInput() {
            return 0f; // Not yet implemented
        }

        public float GetHorizontalInput() {
            return 0f; // Not yet implemented
        }

        public Vector3 GetAimPoint() {
            if (Time.time >= _nextSeekTime) {
                _currentGunTarget = SeekTarget();
            }

            if (_currentGunTarget != null) {
                GunImpactPrediction prediction = PredictPosition(_currentGunTarget);
                if (prediction.willImpact) {
                    currentAimPoint = prediction.impactPosition;
                }
            }

            return currentAimPoint;
        }

        public bool GetFireInput() {
            return true;
        }

        private Ship SeekTarget() {
            HashSet<Ship> allShips = VehiclesManager.Instance.AllShips;
            Ship closestShip = null;
            float closestShipDistance = float.MaxValue;
            foreach (Ship ship in allShips) {
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

        private GunImpactPrediction PredictPosition(Ship target) {
            ShipGun tracingGun = _controlledShip.MainGuns[0];
            Vector3 deltaPosition = target.transform.position - _controlledShip.transform.position;
            
            bool hasValidAngle = ProjectileMotion.FiringAngle(
                deltaPosition,
                tracingGun.muzzleVelocity,
                out float angle
            );
            if (!hasValidAngle) {
                return new GunImpactPrediction(false, new Vector3());
            }
            
            float timeOfFlight = ProjectileMotion.ExpectedTimeOfFlight(angle, tracingGun.muzzleVelocity);
            Vector3 positionOffset = target.Rigidbody.velocity * timeOfFlight;
            
            return new GunImpactPrediction(true, target.transform.position + positionOffset);
        }
    }
}