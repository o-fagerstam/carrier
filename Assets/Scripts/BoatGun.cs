using PhysicsUtilities;
using UnityEngine;

public class BoatGun : MonoBehaviour {
    private static readonly float
        traceFrameInterval = 30; // How many physics frames between each step for trajectory tracing

    private bool _hasAllowedFiringAngle;
    private float _timeSinceLastFired;
    [SerializeField] public Shell ammunitionPrefab;
    [SerializeField] private Transform gunElevationTransform;
    [SerializeField] private float horizontalRotationSpeed = 2f;
    [SerializeField] private ParticleSystem muzzleParticleSystemPrefab;
    [SerializeField] private float muzzleVelocity = 100f;
    public BoatMovement parentBoat;
    [SerializeField] private float reloadTime = 3f;
    [SerializeField] private float verticalElevationSpeed = 1f;
    private Vector3 MuzzlePosition => gunElevationTransform.position + gunElevationTransform.forward * 3;
    public Vector3 CurrentImpactPoint => PredictGunImpact();

    private void Awake() {
        _timeSinceLastFired = reloadTime;
    }

    private void Update() {
        if (parentBoat.controller == Controller.Human) {
            HandlePlayerControl();
        }
    }

    private void HandlePlayerControl() {
        if (GameCamera.RayCastMadeGunTargetingHit) {
            Vector3 targetPoint = GameCamera.RayCastGunTargetingHit.point;
            HandleAim(targetPoint, out Vector3 desiredFiringAngle);
            _hasAllowedFiringAngle = CheckAllowedFiringAngle(desiredFiringAngle);
        }

        if (Input.GetMouseButton(0)) {
            HandleGunFire(_hasAllowedFiringAngle);
        }

        _timeSinceLastFired = Mathf.Min(_timeSinceLastFired, reloadTime);
    }

    private void HandleAim(Vector3 targetPoint, out Vector3 desiredFiringAngle) {
        desiredFiringAngle.z = 0f;
        desiredFiringAngle.y = RotateTurret(targetPoint);
        desiredFiringAngle.x = RotateGunElevation(targetPoint);
    }

    private float RotateTurret(Vector3 targetPoint) {
        Transform thisTransform = transform;
        Vector3 position = thisTransform.position;
        Vector3 dir = targetPoint - position;
        dir.y = 0f;
        dir = dir.normalized;
        Quaternion lookRotation = Quaternion.LookRotation(dir);
        Vector3 nextRotation = Quaternion.RotateTowards(
                transform.rotation,
                lookRotation,
                Time.deltaTime * horizontalRotationSpeed
            )
            .eulerAngles;
        transform.rotation = Quaternion.Euler(nextRotation);

        return lookRotation.eulerAngles.y;
    }

    private float RotateGunElevation(Vector3 targetPoint) {
        Vector3 deltaPosition = targetPoint - MuzzlePosition;
        var validAngle = ProjectileMotion.FiringAngle(deltaPosition, muzzleVelocity, out var angle);
        angle = -angle;
        if (!validAngle) {
            return -90;
        }

        Quaternion targetGunElevation = Quaternion.Euler(angle, 0f, 0f);
        Quaternion nextGunElevation = Quaternion.RotateTowards(
            gunElevationTransform.localRotation,
            targetGunElevation,
            Time.deltaTime * verticalElevationSpeed
        );
        gunElevationTransform.localRotation = nextGunElevation;
        return angle;
    }

    private bool CheckAllowedFiringAngle(Vector3 desiredFiringVector) {
        Quaternion e = Quaternion.Euler(desiredFiringVector);
        var angleToTarget = Quaternion.Angle(e, gunElevationTransform.rotation);
        return angleToTarget < 0.02f;
    }

    private void HandleGunFire(bool hasAllowedFiringAngle) {
        _timeSinceLastFired += Time.deltaTime;
        if (hasAllowedFiringAngle &&
            _timeSinceLastFired >= reloadTime
        ) {
            _timeSinceLastFired = 0f; // Should be made more accurate with a subtraction instead
            Quaternion muzzleRotation = gunElevationTransform.rotation;
            Shell firedShell = Instantiate(ammunitionPrefab, MuzzlePosition, muzzleRotation);
            firedShell.shellOwner = transform.parent;
            var firedShellRigidBody = firedShell.GetComponent<Rigidbody>();
            firedShellRigidBody.velocity = firedShell.transform.forward * muzzleVelocity;

            Instantiate(muzzleParticleSystemPrefab, MuzzlePosition, muzzleRotation);
        }
    }

    private Vector3 PredictGunImpact() {
        var step = 1;
        Vector3 origin = MuzzlePosition;
        Vector3 p0 = origin;
        Vector3 velocity = gunElevationTransform.forward * muzzleVelocity;
        Vector3 p1 = ProjectileMotion.PointAtTime(p0, velocity, traceFrameInterval * Time.fixedDeltaTime * step++);

        var madeHit = Raycasting.ClosestRaycastHit(p0, p1, 10, GameCamera.GunTargetingMask, out RaycastHit hit);
        while (!madeHit && p1.y > -100) {
            p0 = p1;
            p1 = ProjectileMotion.PointAtTime(origin, velocity, traceFrameInterval * Time.fixedDeltaTime * step++);
            Debug.DrawLine(p0, p1);
            madeHit = Raycasting.ClosestRaycastHit(p0, p1, 10, GameCamera.GunTargetingMask, out hit);
        }

        if (!madeHit) {
            throw new UnityException("Gun impact trace failed to find anything, even sea level");
        }

        return hit.point;
    }
}