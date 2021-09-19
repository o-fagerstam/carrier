using PhysicsUtilities;
using UnityEngine;

namespace Ship {
    public abstract class ShipGun : MonoBehaviour {
        [SerializeField] public Shell ammunitionPrefab;
        [SerializeField] protected ParticleSystem muzzleParticleSystemPrefab;

        public ShipMain parentBoat;
        [SerializeField] protected Transform horizontalRotationPart;
        public Transform verticalRotationPart;

        [SerializeField] protected float horizontalRotationSpeed = 2f;
        [SerializeField] protected float maxHorizontalRotation = 181f; // Put it above 180 for 180 degree rotation
        [SerializeField] protected float verticalElevationSpeed = 1f;
        public float maxElevation;
        public float minElevation;
        
        [SerializeField] protected float reloadTime = 3f;
        public float muzzleVelocity = 100f;
        public float spreadAngle;

        public bool IsLoaded => timeOfLastFiring <= Time.time - reloadTime;
        protected Vector3 MuzzlePosition => verticalRotationPart.position + verticalRotationPart.forward * 3;

        private Vector3 _desiredFiringAngle;
        private bool _hasPredictedImpactThisTick;
        protected float timeOfLastFiring;
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

        protected virtual void Awake() {
            timeOfLastFiring = -reloadTime;
        }

        protected virtual void Update() {
            _hasPredictedImpactThisTick = false;
        }
        
        public void ReceiveAimInput (Vector3 desiredAimPoint) {
            ProcessAim(desiredAimPoint, out _desiredFiringAngle);
        }
        public void ReceiveFireInput () {
            bool hasAllowedFiringAngle = TestAllowedFiringAngle(_desiredFiringAngle);
            if (hasAllowedFiringAngle && IsLoaded) {
                Fire();
            }
        }

        protected void ProcessAim(Vector3 targetPoint, out Vector3 desiredFiringAngle) {
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

        private bool TestAllowedFiringAngle(Vector3 desiredFiringVector) {
            Quaternion e = Quaternion.Euler(desiredFiringVector);
            float angleToTarget = Quaternion.Angle(e, verticalRotationPart.rotation);
            return angleToTarget < 0.02f;
        }

        protected abstract void Fire ();

        /// <summary>
        /// Predicts whether a shell will impact, and if so, the point of impact.
        /// </summary>
        protected abstract GunImpactPrediction PredictGunImpact ();

        public virtual bool CanPotentiallyHitPoint (Vector3 targetPoint, out float firingAngle, out float timeToImpact) {
            Vector3 deltaPosition = targetPoint - transform.position;
            
            bool canPotentiallyHitTarget = ProjectileMotion.FiringAngle(
                deltaPosition,
                muzzleVelocity,
                out firingAngle,
                minElevation,
                maxElevation
            );
            
            timeToImpact = ProjectileMotion.ExpectedTimeOfFlight(firingAngle, muzzleVelocity);
            return canPotentiallyHitTarget;
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