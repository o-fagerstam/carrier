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
    public Vector3 CurrentAimPoint => GetCurrentAimPoint();

    private void Update() {
        if (GameCamera.RayCastMadeHit) {
            var desiredFiringAngle = Vector3.zero;
            var deltaPosition = GameCamera.RaycastHit.point - transform.position;
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
        var angleOfLaunch = CalculateFiringAngle(deltaPosition.magnitude);
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
        if (_timeSinceLastFired >= reloadTime &&
            _hasAllowedFiringAngle
        ) {
            _timeSinceLastFired = 0f; // Should be made more accurate with a subtraction instead
            var instantiateLocation = gunElevationTransform.position + transform.forward;
            var firedShell = Instantiate(ammunition, instantiateLocation, gunElevationTransform.rotation);
            var firedShellRigidBody = firedShell.GetComponent<Rigidbody>();
            firedShellRigidBody.velocity = firedShell.transform.forward * _gunPower;
        }
    }

    private Vector3 GetCurrentAimPoint() {
        var gunForward = gunElevationTransform.forward;
        var groundForward = new Vector3(gunForward.x, 0f, gunForward.z).normalized;
        var angle = Vector3.Angle(gunForward, groundForward);
        var firingDistance = CalculateFiringDistance(angle);
        Debug.Log($"gunForward {gunForward}\ngroundForward {groundForward}\nangle {angle}\nfiringDistance {firingDistance}");
        Debug.Log(gunElevationTransform.position + " " + gunElevationTransform + groundForward * firingDistance);
        return gunElevationTransform.position + groundForward * firingDistance;
    }

    private float CalculateFiringAngle(float distance) {
        // Assumes 0 height difference
        return Mathf.Asin(Physics.gravity.y * distance / (_gunPower * _gunPower)) * 0.5f *
               Mathf.Rad2Deg;
    }

    private float CalculateFiringDistance(float angle) {
        // Assumes 0 height difference
        var Vx = Mathf.Sin(angle * Mathf.Deg2Rad) * _gunPower;
        var Vy = Mathf.Cos(angle * Mathf.Deg2Rad) * _gunPower;
        var g = -Physics.gravity.y;
        var h = 0;
        Debug.Log($"angle {angle} Vx {Vx} Vy {Vy}");
        return (Vx / g) * (Vy + Mathf.Sqrt(Vy * Vy + 2 * g * h));
    }
}