using PhysicsUtilities;
using UnityEngine;

namespace Ship {
    public class NavalGun : ShipGun {
        protected override void Fire () {
            timeOfLastFiring = Time.time;
            Quaternion spread = Quaternion.Euler(
                Random.Range(-spreadAngle, spreadAngle),
                Random.Range(-spreadAngle, spreadAngle),
                Random.Range(-spreadAngle, spreadAngle)
            );

            Quaternion muzzleRotation = verticalRotationPart.rotation * spread;

            Shell firedShell = Instantiate(ammunitionPrefab, MuzzlePosition, muzzleRotation);
            firedShell.shellOwner = transform.parent;
            firedShell.Rigidbody.velocity = firedShell.transform.forward * muzzleVelocity;

            Instantiate(muzzleParticleSystemPrefab, MuzzlePosition, muzzleRotation);
        }
        protected override GunImpactPrediction PredictGunImpact ()  {
            Vector3 origin = MuzzlePosition;
            Vector3 v0 = verticalRotationPart.forward*muzzleVelocity;
            bool success = Raycasting.TraceTrajectoryUntilImpact(
                origin,
                v0,
                out RaycastHit hit,
                (int)LayerMasks.ShipGunTarget
            );
            if (success) {
                return new GunImpactPrediction(true, hit.point);
            } else {
                return new GunImpactPrediction(false, new Vector3());
            }
        }
    }
}
