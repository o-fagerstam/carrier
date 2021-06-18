using PhysicsUtilities;
using UnityEngine;

namespace Ship {
    public abstract class ShipGun : MonoBehaviour {
        protected bool _hasAllowedFiringAngle;
        private bool _hasPredictedImpactThisTick;
        protected float _lastFired;
        private GunImpactPrediction _lastImpactPrediction;
        [SerializeField] public Shell ammunitionPrefab;
        [SerializeField] protected Transform horizontalRotationPart;
        [SerializeField] protected float horizontalRotationSpeed = 2f;
        [SerializeField] protected ParticleSystem muzzleParticleSystemPrefab;
        public float muzzleVelocity = 100f;
        public Ship parentBoat;
        [SerializeField] protected float reloadTime = 3f;
        public float spreadAngle;
        [SerializeField] protected float verticalElevationSpeed = 1f;
        public Transform verticalRotationPart;
        protected Vector3 MuzzlePosition => verticalRotationPart.position + verticalRotationPart.forward * 3;
        public bool IsLoaded => _lastFired <= Time.time - reloadTime;

        public GunImpactPrediction GunImpactPrediction {
            get {
                if (!_hasPredictedImpactThisTick) {
                    _lastImpactPrediction = PredictGunImpact();
                    _hasPredictedImpactThisTick = true;
                }

                return _lastImpactPrediction;
            }
        }

        protected void Awake() {
            _lastFired = -reloadTime;
        }

        protected void Update() {
            _hasPredictedImpactThisTick = false;
            if (parentBoat.isActive) {
                HandleGunControl();
            }
        }

        protected void HandleGunControl() {
            Vector3 targetPoint = parentBoat.shipController.GetAimPoint();
            HandleAim(targetPoint, out Vector3 desiredFiringAngle);
            _hasAllowedFiringAngle = CheckAllowedFiringAngle(desiredFiringAngle);

            if (parentBoat.shipController.GetFireInput()) {
                HandleGunFire(_hasAllowedFiringAngle);
            }
        }

        protected void HandleAim(Vector3 targetPoint, out Vector3 desiredFiringAngle) {
            desiredFiringAngle.z = 0f;
            desiredFiringAngle.y = RotateTurret(targetPoint);
            desiredFiringAngle.x = RotateGunElevation(targetPoint);
        }

        protected float RotateTurret(Vector3 targetPoint) {
            Vector3 position = horizontalRotationPart.position;
            Vector3 dir = targetPoint - position;
            dir.y = 0f;
            dir = dir.normalized;
            Quaternion lookRotation = Quaternion.LookRotation(dir);
            Vector3 nextRotation = Quaternion.RotateTowards(
                    horizontalRotationPart.rotation,
                    lookRotation,
                    Time.deltaTime * horizontalRotationSpeed
                )
                .eulerAngles;
            horizontalRotationPart.rotation = Quaternion.Euler(nextRotation);

            return lookRotation.eulerAngles.y;
        }

        protected float RotateGunElevation(Vector3 targetPoint) {
            Vector3 deltaPosition = targetPoint - MuzzlePosition;
            bool validAngle = ProjectileMotion.FiringAngle(deltaPosition, muzzleVelocity, out float angle);
            angle = -angle;
            if (!validAngle) {
                return -90;
            }

            Quaternion targetGunElevation = Quaternion.Euler(angle, 0f, 0f);
            Quaternion nextGunElevation = Quaternion.RotateTowards(
                verticalRotationPart.localRotation,
                targetGunElevation,
                Time.deltaTime * verticalElevationSpeed
            );
            verticalRotationPart.localRotation = nextGunElevation;
            return angle;
        }

        protected bool CheckAllowedFiringAngle(Vector3 desiredFiringVector) {
            Quaternion e = Quaternion.Euler(desiredFiringVector);
            float angleToTarget = Quaternion.Angle(e, verticalRotationPart.rotation);
            return angleToTarget < 0.02f;
        }

        protected void HandleGunFire(bool hasAllowedFiringAngle) {
            if (CheckFiringIsLegal(hasAllowedFiringAngle)) {
                Fire();
            }
        }

        protected bool CheckFiringIsLegal(bool hasAllowedFiringAngle) {
            return hasAllowedFiringAngle && IsLoaded;
        }

        protected virtual void Fire() {
            _lastFired = Time.time;
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

        protected virtual GunImpactPrediction PredictGunImpact() {
            Vector3 origin = MuzzlePosition;
            Vector3 v0 = verticalRotationPart.forward * muzzleVelocity;
            bool success = Raycasting.TraceTrajectoryUntilImpact(
                origin,
                v0,
                out RaycastHit hit,
                GameCamera.GunTargetingMask
            );
            if (success) {
                return new GunImpactPrediction(true, hit.point);
            }
            else {
                return new GunImpactPrediction(false, new Vector3());
            }
        }
    }
}

public struct GunImpactPrediction {
    public bool willImpact;
    public Vector3 impactPosition;

    public GunImpactPrediction(bool willImpact, Vector3 impactPosition) {
        this.willImpact = willImpact;
        this.impactPosition = impactPosition;
    }
}