using UnityEngine;

namespace Ship {
    public class ThirdPersonShipCamera : ShipCamera {

        [SerializeField] private Transform swivel;
        [SerializeField] private Transform stick;
        [SerializeField] private Transform cameraTransform;
        private static ThirdPersonShipCamera _instance;
        public static ThirdPersonShipCamera Instance => _instance;

        protected override void Awake() {
            base.Awake();
            swivel = transform.GetChild(0);
            stick = swivel.GetChild(0);
            cameraTransform = stick.GetChild(0);
            XRotation = cameraTransform.localRotation.eulerAngles.x;
            YRotation = swivel.rotation.eulerAngles.y;
            ScrollLevel = 0.3f;

            Cursor.lockState = CursorLockMode.Locked;
            
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
            
            YRotation += mouseX;
            YRotation = Mathf.Repeat(YRotation, 360f);

            ScrollLevel += -mouseScrollWheel;
            ScrollLevel = Mathf.Clamp(ScrollLevel, 0f, 1f);
            
            Vector3 oldCameraPosition = cameraTransform.position;
            
            swivel.position = objectToFollow.position;

            var swivelXAngle = Mathf.Lerp(30f, 45f, 1f - ScrollLevel);
            swivel.localRotation = Quaternion.Euler(swivelXAngle, YRotation, 0f);

            if (mouseScrollWheel != 0f) {
                var stickDistance = Mathf.Lerp(30f, 120f, ScrollLevel);
                stick.localPosition = new Vector3(0f, 0f, -stickDistance);

                Vector3 newCameraPosition = cameraTransform.position;
                var angularChange = Vector3.Angle(
                    (newCameraPosition - RayCastGunTargetingHit.point).normalized,
                    (oldCameraPosition - RayCastGunTargetingHit.point).normalized
                );
                if (oldCameraPosition.y > newCameraPosition.y) {
                    angularChange = -angularChange;
                }

                XRotation -= angularChange;
            }

            XRotation -= mouseY;
            XRotation = Mathf.Clamp(XRotation, -swivelXAngle, 90f - swivelXAngle);

            cameraTransform.localRotation = Quaternion.Euler(XRotation, 0f, 0f);
        }
    }
}