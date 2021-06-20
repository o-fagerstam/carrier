using UnityEngine;

namespace Ship {
    public class ScopeShipCamera : ShipCamera {

        private Transform _cameraTransform;
        private static ScopeShipCamera _instance;
        public static ScopeShipCamera Instance => _instance;

        protected override void Awake() {
            base.Awake();
            _cameraTransform = transform.GetChild(0);
            AudioListenerComponent.enabled = false;
            
            if (_instance != null && _instance != this) {
                Destroy(gameObject);
            }
            else {
                _instance = this;
            }
        }

        protected override void UpdateCameraPosition() {
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
            float mouseScrollWheel = Input.GetAxis("Mouse ScrollWheel") * MouseScrollSensitivity * Time.deltaTime;

            YRotation += mouseX * (CameraComponent.fieldOfView / 60f);
            YRotation = Mathf.Repeat(YRotation, 360f);

            ScrollLevel += -mouseScrollWheel;
            ScrollLevel = Mathf.Clamp(ScrollLevel, 0f, 1f);
            
            Vector3 oldCameraPosition = _cameraTransform.position;
            
            if (mouseScrollWheel != 0f) {
                float cameraHeight = Mathf.Lerp(10f, 120f, 1f-ScrollLevel);
                _cameraTransform.localPosition = new Vector3(0f, cameraHeight);

                Vector3 newCameraPosition = _cameraTransform.position;
                float angularChange = Vector3.Angle(
                    (newCameraPosition - RayCastGunTargetingHit.point).normalized,
                    (oldCameraPosition - RayCastGunTargetingHit.point).normalized
                );
                if (oldCameraPosition.y < newCameraPosition.y) {
                    angularChange = -angularChange;
                }

                XRotation -= angularChange;

                float zoomLevel = Mathf.Lerp(2f, 50f, ScrollLevel);
                CameraComponent.fieldOfView = zoomLevel;
            }

            XRotation -= mouseY * (CameraComponent.fieldOfView / 60f);
            XRotation = Mathf.Clamp(XRotation, 0f, 45f);

            _cameraTransform.localRotation = Quaternion.Euler(XRotation, YRotation, 0f);
            
        }
    }
}