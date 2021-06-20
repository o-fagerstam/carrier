using System;
using System.Linq;
using UnityEngine;

public class GameCamera : MonoBehaviour {
    public static Ray MouseRay;
    public static RaycastHit RayCastGunTargetingHit;
    public static bool RayCastMadeGunTargetingHit;
    public static LayerMask GunTargetingMask;
    private readonly float _mouseScrollSensitivity = 100f;
    private float _xRotation, _yRotation, _scrollLevel;
    public float mouseSensitivity = 100f;

    public Transform objectToFollow;

    public Transform swivel, stick, cameraTransform;
    public static Camera CurrentCamera { get; private set; }

    private void Start() {
        CurrentCamera = Camera.main;

        swivel = transform.GetChild(0);
        stick = swivel.GetChild(0);
        cameraTransform = stick.GetChild(0);
        _xRotation = cameraTransform.localRotation.eulerAngles.x;
        _yRotation = swivel.rotation.eulerAngles.y;
        _scrollLevel = 0.3f;

        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Awake() {
        GunTargetingMask = LayerMask.GetMask("Water", "Targetable");
        CurrentCamera = Camera.main;
    }

    private void LateUpdate() {
        UpdateMouseTarget();

        var mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        var mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
        var mouseScrollWheel = Input.GetAxis("Mouse ScrollWheel") * _mouseScrollSensitivity * Time.deltaTime;


        _yRotation += mouseX;
        _yRotation = Mathf.Repeat(_yRotation, 360f);

        _scrollLevel += -mouseScrollWheel;
        _scrollLevel = Mathf.Clamp(_scrollLevel, 0f, 1f);


        Vector3 oldCameraPosition = cameraTransform.position;

        var swivelXAngle = Mathf.Lerp(15f, 30f, _scrollLevel);
        swivel.localRotation = Quaternion.Euler(swivelXAngle, _yRotation, 0f);

        if (mouseScrollWheel != 0f) {
            var stickDistance = Mathf.Lerp(20f, 120f, _scrollLevel);
            stick.localPosition = new Vector3(0f, 0f, -stickDistance);

            Vector3 newCameraPosition = cameraTransform.position;
            var angularChange = Vector3.Angle(
                (newCameraPosition - RayCastGunTargetingHit.point).normalized,
                (oldCameraPosition - RayCastGunTargetingHit.point).normalized
            );
            if (oldCameraPosition.y > newCameraPosition.y) {
                angularChange = -angularChange;
            }

            _xRotation -= angularChange;
        }

        _xRotation -= mouseY;
        _xRotation = Mathf.Clamp(_xRotation, -swivelXAngle, 90f - swivelXAngle);

        cameraTransform.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);

        swivel.position = objectToFollow.position;
        
        GunMarkerDrawer.Instance.RefreshMarkers();
    }

    private void UpdateMouseTarget() {
        MouseRay = CurrentCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f));
        var hits = new RaycastHit[10];
        var numHits = Physics.RaycastNonAlloc(MouseRay, hits, CurrentCamera.farClipPlane, GunTargetingMask);


        if (numHits == 0) {
            RayCastMadeGunTargetingHit = false;
            return;
        }

        Vector3 cameraPos = CurrentCamera.transform.position;
        Array.Resize(ref hits, numHits);
        hits = hits.OrderBy(h => (h.point - cameraPos).magnitude).ToArray();

        foreach (RaycastHit hit in hits) {
            Transform t = hit.transform;
            if (t == objectToFollow) {
                continue;
            }

            var isValid = true;
            while (t.parent != null) {
                t = t.parent;
                if (t != objectToFollow) {
                    continue;
                }

                isValid = false;
                break;
            }

            if (isValid) {
                RayCastMadeGunTargetingHit = true;
                RayCastGunTargetingHit = hit;
                return;
            }
        }
            
        RayCastMadeGunTargetingHit = false;
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(RayCastGunTargetingHit.point, 1f);
    }
}