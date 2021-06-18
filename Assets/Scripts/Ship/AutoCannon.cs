using PhysicsUtilities;
using UnityEngine;

namespace Ship {
    public class AutoCannon : ShipGun {
        public float maxEffectiveRange;

        protected override void Fire() {
            _lastFired = Time.time;
            Quaternion muzzleRotation = verticalRotationPart.rotation;
            DistanceLimitedShell firedShell = (DistanceLimitedShell) Instantiate(ammunitionPrefab, MuzzlePosition, muzzleRotation);
            firedShell.shellOwner = transform.parent;
            firedShell.maxDistance = maxEffectiveRange;
            firedShell.Rigidbody.velocity = firedShell.transform.forward * muzzleVelocity;

            Instantiate(muzzleParticleSystemPrefab, MuzzlePosition, muzzleRotation);
        }

        protected override GunImpactPrediction PredictGunImpact() {
            Vector3 origin = MuzzlePosition;
            Vector3 v0 = verticalRotationPart.forward * muzzleVelocity;
            var success = Raycasting.TraceTrajectoryUntilImpact(
                origin,
                v0,
                out RaycastHit hit,
                GameCamera.GunTargetingMask,
                maxEffectiveRange
            );
            if (success) {
                return new GunImpactPrediction(true, hit.point);
            }
            else {
                return new GunImpactPrediction(true, new Vector3());
            }
        }
    }
}