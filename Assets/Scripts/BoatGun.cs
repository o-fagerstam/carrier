using System;
using UnityEngine;

public class BoatGun : MonoBehaviour {
    private readonly float _gunPower = 50f;
    private bool _gunTargetMarkerValid;
    private bool _hasAllowedFiringAngle;
    private float _timeSinceLastFired;
    [SerializeField] public GameObject ammunition;
    [SerializeField] private Transform gunElevationTransform;
    [SerializeField] private float horizontalRotationSpeed = 2f;
    [SerializeField] private float reloadTime = 3f;
    [SerializeField] private float verticalElevationSpeed = 1f;
    private Vector3 MuzzlePosition => gunElevationTransform.position + gunElevationTransform.forward;
    public Vector3 CurrentAimPoint => GetCurrentAimPoint();

    private void Update() {
        if (GameCamera.RayCastMadeWaterHit) {
            var desiredFiringAngle = Vector3.zero;
            var deltaPosition = GameCamera.RayCastWaterHit.point - MuzzlePosition;
            HandleAim(deltaPosition, out desiredFiringAngle);
            _hasAllowedFiringAngle = CheckAllowedFiringAngle(desiredFiringAngle);
        }

        if (Input.GetMouseButton(0)) {
            HandleGunFire();
        }
    }

    private void HandleAim(Vector3 deltaPosition, out Vector3 desiredFiringAngle) {
        desiredFiringAngle.z = 0f;
        desiredFiringAngle.y = RotateTurret(deltaPosition);
        desiredFiringAngle.x = RotateGunElevation(deltaPosition);
    }

    private float RotateTurret(Vector3 deltaPosition) {
        var turretLookRotation = Quaternion.LookRotation(deltaPosition);
        var turretRotation = Quaternion.Lerp(
                transform.rotation,
                turretLookRotation,
                Time.deltaTime * horizontalRotationSpeed
            )
            .eulerAngles;
        transform.rotation = Quaternion.Euler(0f, turretRotation.y, 0f);
        return turretLookRotation.eulerAngles.y;
    }

    private float RotateGunElevation(Vector3 deltaPosition) {
        var angleOfLaunch = CalculateFiringAngle(deltaPosition);
        Debug.Log($"Calculated angle of launch {angleOfLaunch}");
        if (float.IsNaN(angleOfLaunch)) {
            return -90;
        }

        var targetGunElevation = Quaternion.Euler(angleOfLaunch, 0f, 0f);
        var gunElevation = Quaternion.Lerp(
            gunElevationTransform.localRotation,
            targetGunElevation,
            Time.deltaTime * verticalElevationSpeed
        );
        gunElevationTransform.localRotation = gunElevation;
        return angleOfLaunch;
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
            var firedShell = Instantiate(ammunition, MuzzlePosition, gunElevationTransform.rotation);
            var firedShellRigidBody = firedShell.GetComponent<Rigidbody>();
            firedShellRigidBody.velocity = firedShell.transform.forward * _gunPower;
        }
    }

    private Vector3 GetCurrentAimPoint() {
        var forward = gunElevationTransform.forward;
        var firingDistance = CalculateFiringDistance(forward, MuzzlePosition.y);
        var groundForward = new Vector3(forward.x, 0f, forward.z);
        return MuzzlePosition + groundForward * firingDistance;
    }

    private float CalculateFiringAngle(float distance) {
        // Assumes 0 height difference
        return Mathf.Asin(Physics.gravity.y * distance / (_gunPower * _gunPower)) * 0.5f *
               Mathf.Rad2Deg;
    }

    private float CalculateFiringAngle(Vector3 deltaPosition) {
        var d = new Vector2(deltaPosition.x, deltaPosition.z).magnitude;
        var h = -deltaPosition.y;
        var g = -Physics.gravity.y;
        var twiceOfAngleMinusFaceAngle = Mathf.Acos(
                                             (g * d * d / (_gunPower * _gunPower) - h) /
                                             Mathf.Sqrt(h * h + d * d)
                                         ) * Mathf.Rad2Deg;
        var faceAngle = Mathf.Atan(d / h) * Mathf.Rad2Deg;
        return (twiceOfAngleMinusFaceAngle + faceAngle) / 2 - 90f;
    }

    private float CalculateFiringDistance(float angle) {
        // Assumes 0 height difference
        var angleRadians = angle * Mathf.Deg2Rad;
        var Vx = Mathf.Sin(angleRadians) * _gunPower;
        var Vy = Mathf.Cos(angleRadians) * _gunPower;
        var g = -Physics.gravity.y;
        var h = 0;
        return Vx / g * (Vy + Mathf.Sqrt(Vy * Vy + 2 * g * h));
    }
    
    private float CalculateFiringDistance(Vector3 directionOfFire, float heightDiff) {
        var directionOfGround = new Vector3(directionOfFire.x, 0f, directionOfFire.z).normalized;
        var angleRadians = Vector3.Angle(directionOfFire, directionOfGround) * Mathf.Deg2Rad;
        var vx = Mathf.Sin(angleRadians) * _gunPower;
        var vy = Mathf.Cos(angleRadians) * _gunPower;
        var g = -Physics.gravity.y;
        var h = heightDiff;
        return vx / g * (vy + Mathf.Sqrt(vy * vy + 2 * g * h));
    }

    private void OnDrawGizmos() {
        if (GameCamera.RayCastMadeWaterHit) {
            var hit = GameCamera.RayCastWaterHit;
            Gizmos.DrawLine(gunElevationTransform.position, hit.point);
        }
    }
}