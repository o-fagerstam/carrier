using System;
using PhysicsUtilities;
using UnityEngine;

public class BoatGun : MonoBehaviour {
    private readonly float _gunInitialVelocity = 50f;
    private bool _hasAllowedFiringAngle;
    private float _timeSinceLastFired;
    [SerializeField] public Shell ammunitionPrefab;
    [SerializeField] private Transform gunElevationTransform;
    [SerializeField] private float horizontalRotationSpeed = 2f;
    public BoatMovement parentBoat;
    [SerializeField] private float reloadTime = 3f;
    [SerializeField] private float verticalElevationSpeed = 1f;
    private Vector3 MuzzlePosition => gunElevationTransform.position + gunElevationTransform.forward * 2;
    public Vector3 CurrentAimPoint => GetCurrentAimPoint();

    private void Update() {
        if (parentBoat.controller == Controller.Human) {
            HandlePlayerControl();
        }
    }

    private void HandlePlayerControl() {
        if (GameCamera.RayCastMadeWaterHit) {
            var targetPoint = GameCamera.RayCastWaterHit.point;
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
        var validAngle = ProjectileMotion.CalculateFiringAngle(deltaPosition, _gunInitialVelocity, out var angle);

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
            var firedShell = Instantiate(ammunitionPrefab, MuzzlePosition, gunElevationTransform.rotation);
            firedShell.shellOwner = transform.parent;
            var firedShellRigidBody = firedShell.GetComponent<Rigidbody>();
            firedShellRigidBody.velocity = firedShell.transform.forward * _gunInitialVelocity;
        }
    }

    private Vector3 GetCurrentAimPoint() {
        var forward = gunElevationTransform.forward;
        var firingDistance = ProjectileMotion.CalculateFiringDistance(forward, MuzzlePosition.y, _gunInitialVelocity);
        var groundForward = new Vector3(forward.x, 0f, forward.z).normalized;
        return MuzzlePosition + groundForward * firingDistance;
    }

    private void OnDrawGizmos() {
        Gizmos.DrawSphere(MuzzlePosition, 0.5f);
    }
}