using System;
using PhysicsUtilities;
using UnityEngine;

public class ShipGun : MonoBehaviour {
    private bool _hasAllowedFiringAngle;
    private float _lastFired;
    [SerializeField] public Shell ammunitionPrefab;
    [SerializeField] private Transform gunElevationTransform;
    [SerializeField] private float horizontalRotationSpeed = 2f;
    [SerializeField] private ParticleSystem muzzleParticleSystemPrefab;
    [SerializeField] private float muzzleVelocity = 100f;
    public ShipMain parentBoat;
    [SerializeField] private float reloadTime = 3f;
    [SerializeField] private float verticalElevationSpeed = 1f;
    private Vector3 MuzzlePosition => gunElevationTransform.position + gunElevationTransform.forward * 3;
    public Vector3 CurrentImpactPoint => PredictGunImpact();
    public bool IsLoaded => _lastFired <= Time.time - reloadTime;

    private void Awake() {
        _lastFired = -reloadTime;
    }

    private void Update() {
        if (parentBoat.isActive) {
            HandleGunControl();
        }
    }

    private void HandleGunControl() {
        Vector3 targetPoint = parentBoat.shipController.GetAimPoint();
        HandleAim(targetPoint, out Vector3 desiredFiringAngle);
        _hasAllowedFiringAngle = CheckAllowedFiringAngle(desiredFiringAngle);

        if (parentBoat.shipController.GetFireInput()) {
            HandleGunFire(_hasAllowedFiringAngle);
        }
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
        if (hasAllowedFiringAngle &&
            IsLoaded
        ) {
            _lastFired = Time.time;
            Quaternion muzzleRotation = gunElevationTransform.rotation;
            Shell firedShell = Instantiate(ammunitionPrefab, MuzzlePosition, muzzleRotation);
            firedShell.shellOwner = transform.parent;
            var firedShellRigidBody = firedShell.GetComponent<Rigidbody>();
            firedShellRigidBody.velocity = firedShell.transform.forward * muzzleVelocity;

            Instantiate(muzzleParticleSystemPrefab, MuzzlePosition, muzzleRotation);
        }
    }

    private Vector3 PredictGunImpact() {
        Vector3 origin = MuzzlePosition;
        Vector3 v0 = gunElevationTransform.forward * muzzleVelocity;
        var success = Raycasting.TraceTrajectoryUntilImpact(
            origin,
            v0,
            out RaycastHit hit,
            GameCamera.GunTargetingMask
        );
        if (!success) {
            throw new UnityException($"Failed to trace trajectory from gun {gameObject.GetInstanceID()}");
        }

        return hit.point;
    }
}