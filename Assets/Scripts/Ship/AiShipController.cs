using System.Collections.Generic;
using System.Linq;
using PhysicsUtilities;
using UnityEngine;

namespace Ship {
    public class AiShipController : ShipController {
        private readonly ShipMain _controlledShip;
        
        private ShipMain _currentGunTarget;
        private Vector3 currentAimPoint;
        
        private float _nextSeekTime = float.MinValue;

        public AiShipController(ShipMain controlledShip) {
            _controlledShip = controlledShip;
        }

        public ShipGearInput GetVerticalInput() {
            return ShipGearInput.None;
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
    }
}