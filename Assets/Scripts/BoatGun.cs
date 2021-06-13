using System;
using PhysicsUtilities;
using UnityEngine;

public class BoatGun : MonoBehaviour {
    
    private bool _hasAllowedFiringAngle;
    private float _timeSinceLastFired;
    [SerializeField] public Shell ammunitionPrefab;
    [SerializeField] private Transform gunElevationTransform;
    [SerializeField] private float horizontalRotationSpeed = 2f;
    public BoatMovement parentBoat;
    [SerializeField] private float reloadTime = 3f;
    [SerializeField] private float verticalElevationSpeed = 1f;
    [SerializeField] private ParticleSystem muzzleParticleSystemPrefab;
    [SerializeField] private float muzzleVelocity = 100f;
    private Vector3 MuzzlePosition => gunElevationTransform.position + gunElevationTransform.forward * 3;
    public Vector3 CurrentAimPoint => GetCurrentAimPoint();

    private void Update() {
        if (parentBoat.controller == Controller.Human) {
            HandlePlayerControl();
        }
    }

    private void HandlePlayerControl() {
        if (GameCamera.RayCastMadeGunTargetingHit) {
            var targetPoint = GameCamera.RayCastGunTargetingHit.point;
            HandleAim(targetPoint, out var desiredFiringAngle);
            _hasAllowedFiringAngle = CheckAllowedFiringAngle(desiredFiringAngle);
        }

        if (Input.GetMouseButton(0)) {
            HandleGunFire();
        }
    }

    private void HandleAim(Vector3 targetPoint, out Vector3 desiredFiringAngle) {
        desiredFiringAngle.z = 0f;
        desiredFiringAngle.y = RotateTurret(targetPoint);
        desiredFiringAngle.x = RotateGunElevation(targetPoint);
    }

    private float RotateTurret(Vector3 targetPoint) {
        var thisTransform = transform;
        var position = thisTransform.position;
        var dir = targetPoint - position;
        dir.y = 0f;
        dir = dir.normalized;
        var lookRotation = Quaternion.LookRotation(dir);
        var nextRotation = Quaternion.RotateTowards(
                transform.rotation,
                lookRotation,
                Time.deltaTime * horizontalRotationSpeed
            )
            .eulerAngles;
        transform.rotation = Quaternion.Euler(nextRotation);

        return lookRotation.eulerAngles.y;
    }

    private float RotateGunElevation(Vector3 targetPoint) {
        var deltaPosition = targetPoint - MuzzlePosition;
        var validAngle = ProjectileMotion.CalculateFiringAngle(deltaPosition, muzzleVelocity, out var angle);
        angle = -angle;
        if (!validAngle) {
            return -90;
        }

        var targetGunElevation = Quaternion.Euler(angle, 0f, 0f);
        var nextGunElevation = Quaternion.RotateTowards(
            gunElevationTransform.localRotation,
            targetGunElevation,
            Time.deltaTime * verticalElevationSpeed
        );
        gunElevationTransform.localRotation = nextGunElevation;
        return angle;
    }

    private bool CheckAllowedFiringAngle(Vector3 desiredFiringVector) {
        var e = Quaternion.Euler(desiredFiringVector);
        var angleToTarget = Quaternion.Angle(e, gunElevationTransform.rotation);
        if (_hasAllowedFiringAngle) {
            return angleToTarget < 3f;
        }

        return angleToTarget < 2f;
    }

    private void HandleGunFire() {
        _timeSinceLastFired += Time.deltaTime;
        if (_hasAllowedFiringAngle &&
            _timeSinceLastFired >= reloadTime
        ) {
            _timeSinceLastFired = 0f; // Should be made more accurate with a subtraction instead
            var muzzleRotation = gunElevationTransform.rotation;
            var firedShell = Instantiate(ammunitionPrefab, MuzzlePosition, muzzleRotation);
            firedShell.shellOwner = transform.parent;
            var firedShellRigidBody = firedShell.GetComponent<Rigidbody>();
            firedShellRigidBody.velocity = firedShell.transform.forward * muzzleVelocity;

            Instantiate(muzzleParticleSystemPrefab, MuzzlePosition, muzzleRotation);
        }
    }

    private Vector3 GetCurrentAimPoint() {
        var angle = -gunElevationTransform.localRotation.eulerAngles.x;
        var firingDistance = ProjectileMotion.CalculateProjectileDistance(angle, MuzzlePosition.y, muzzleVelocity);
        var muzzleHorizontalPos = new Vector3(MuzzlePosition.x, 0f, MuzzlePosition.z);
        var muzzleForward = gunElevationTransform.forward;
        var muzzleGroundForward = new Vector3(muzzleForward.x, 0f, muzzleForward.z).normalized;
        return muzzleHorizontalPos + muzzleGroundForward * firingDistance;
    }
}