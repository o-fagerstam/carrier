using System;
using UnityEngine;

public class GameCamera : MonoBehaviour {
    public static Ray MouseRay;
    public static RaycastHit RayCastWaterHit;
    public static bool RayCastMadeWaterHit;
    private float _xRotation, _yRotation, _scrollLevel;
    private LayerMask _waterLayerMask;
    public float MouseSensitivity = 10f;
    private float _mouseScrollSensitivity = 100f;

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
        _waterLayerMask = LayerMask.GetMask("Water");
        CurrentCamera = Camera.main;
    }

    private void Update() {
        MouseRay = CurrentCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f));
        RayCastMadeWaterHit = Physics.Raycast(MouseRay, out RayCastWaterHit, 10000f, _waterLayerMask);

        var mouseX = Input.GetAxis("Mouse X") * MouseSensitivity * Time.deltaTime;
        var mouseY = Input.GetAxis("Mouse Y") * MouseSensitivity * Time.deltaTime;
        var mouseScrollWheel = Input.GetAxis("Mouse ScrollWheel") * _mouseScrollSensitivity * Time.deltaTime;



        _yRotation += mouseX;
        _yRotation = Mathf.Repeat(_yRotation, 360f);

        _scrollLevel += -mouseScrollWheel;
        _scrollLevel = Mathf.Clamp(_scrollLevel, 0f, 1f);
        
        
        var oldCameraPosition = cameraTransform.position;
        
        var swivelXAngle = Mathf.Lerp(15f, 30f, _scrollLevel);
        swivel.localRotation = Quaternion.Euler(swivelXAngle, _yRotation, 0f);

        if (mouseScrollWheel != 0f) {
            var stickDistance = Mathf.Lerp(20f, 120f, _scrollLevel);
            stick.localPosition = new Vector3(0f, 0f, -stickDistance);

            var newCameraPosition = cameraTransform.position;
            var angularChange = Vector3.Angle(
                (newCameraPosition - RayCastWaterHit.point).normalized,
                (oldCameraPosition - RayCastWaterHit.point).normalized
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
    }
}