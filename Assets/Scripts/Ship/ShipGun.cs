using PhysicsUtilities;
using UnityEngine;

namespace Ship {
    public abstract class ShipGun : MonoBehaviour {
        [SerializeField] public Shell ammunitionPrefab;
        [SerializeField] protected ParticleSystem muzzleParticleSystemPrefab;

        public Ship parentBoat;
        [SerializeField] protected Transform horizontalRotationPart;
        public Transform verticalRotationPart;

        [SerializeField] protected float horizontalRotationSpeed = 2f;
        [SerializeField] protected float maxHorizontalRotation = 181f; // Put it above 180 for 180 degree rotation
        [SerializeField] protected float verticalElevationSpeed = 1f;
        public float maxElevation;
        public float minElevation;
        public float muzzleVelocity = 100f;
        [SerializeField] protected float reloadTime = 3f;
        public float spreadAngle;

        public bool IsLoaded => _lastFired <= Time.time - reloadTime;
        protected Vector3 MuzzlePosition => verticalRotationPart.position + verticalRotationPart.forward * 3;

        protected bool _hasAllowedFiringAngle;
        private bool _hasPredictedImpactThisTick;
        protected float _lastFired;
        private GunImpactPrediction _lastImpactPrediction;

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
            if (maxHorizontalRotation >= 180f) {
                desiredFiringAngle.y = RotateTurret(targetPoint);
            }
            else {
                desiredFiringAngle.y = RotateTurretClamped(targetPoint);
            }

            desiredFiringAngle.x = RotateGunElevation(targetPoint);
        }

        protected float RotateTurret(Vector3 targetPoint) {
            Vector3 position = horizontalRotationPart.position;
            Vector3 horizontalDirection = VectorTools.HorizontalComponent(targetPoint - position).normalized;
            Quaternion desiredLookRotation = Quaternion.LookRotation(horizontalDirection);

            Quaternion nextRotation = Quaternion.RotateTowards(
                horizontalRotationPart.rotation,
                desiredLookRotation,
                Time.deltaTime * horizontalRotationSpeed
            );
            horizontalRotationPart.rotation = nextRotation;

            return desiredLookRotation.eulerAngles.y;
        }

        protected float RotateTurretClamped(Vector3 targetPoint) {
            Vector3 position = horizontalRotationPart.position;
            Vector3 horizontalDirection = VectorTools.HorizontalComponent(targetPoint - position).normalized;
            Quaternion desiredLookRotation = Quaternion.LookRotation(horizontalDirection);
            float globalDesiredLookAngle = desiredLookRotation.eulerAngles.y;
            float globalRotPartAngle = horizontalRotationPart.rotation.eulerAngles.y;
            float localRotPartAngle = horizontalRotationPart.localRotation.eulerAngles.y;
            float globalRotPartZeroAngle = Mathf.Repeat(globalRotPartAngle - localRotPartAngle, 360f);
            float localDesiredLookAngle = Mathf.Repeat(globalDesiredLookAngle - globalRotPartZeroAngle, 360f);


            Vector3 nextRotation;

            if (localRotPartAngle < 180f && localDesiredLookAngle > 180f) {
                nextRotation = Quaternion.RotateTowards(
                        horizontalRotationPart.localRotation,
                        Quaternion.Euler(0f, 359f, 0f),
                        Time.deltaTime * horizontalRotationSpeed
                    )
                    .eulerAngles;
            }
            else if (localRotPartAngle > 180f && localDesiredLookAngle < 180f) {
                nextRotation = Quaternion.RotateTowards(
                        horizontalRotationPart.localRotation,
                        Quaternion.Euler(0f, 1f, 0f),
                        Time.deltaTime * horizontalRotationSpeed
                    )
                    .eulerAngles;
            }
            else {
                nextRotation = Quaternion.RotateTowards(
                        horizontalRotationPart.localRotation,
                        Quaternion.Euler(0f, localDesiredLookAngle, 0f),
                        Time.deltaTime * horizontalRotationSpeed
                    )
                    .eulerAngles;
            }

            if (nextRotation.y > 180f && nextRotation.y < 360 - maxHorizontalRotation) {
                nextRotation.y = 360f - maxHorizontalRotation;
            }
            else if (nextRotation.y < 180f && nextRotation.y > maxHorizontalRotation) {
                nextRotation.y = maxHorizontalRotation;
            }

            horizontalRotationPart.localRotation = Quaternion.Euler(nextRotation);

            return desiredLookRotation.eulerAngles.y;
        }

        protected float RotateGunElevation(Vector3 targetPoint) {
            Vector3 deltaPosition = targetPoint - MuzzlePosition;
            bool validAngle = ProjectileMotion.FiringAngle(
                deltaPosition,
                muzzleVelocity,
                out float angle,
                minElevation,
                maxElevation
            );
            if (!validAngle) {
                return -90;
            }

            angle = -angle;

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
                ShipCamera.GunTargetingMask
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