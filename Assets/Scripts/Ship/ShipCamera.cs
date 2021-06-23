using System;
using System.Linq;
using UnityEngine;

namespace Ship {
    public class ShipCamera : MonoBehaviour, IShipController {
        public static Ray MouseRay;
        public static RaycastHit RayCastGunTargetingHit;
        public static bool RayCastMadeGunTargetingHit;
        public static LayerMask GunTargetingMask;
        public static LayerMask WaterMask;
        private readonly float _mouseScrollSensitivity = 100f;
        private float _xRotation, _yRotation, _thirdPersonScrollLevel;
        private float _scopedScrollLevel = 1f;
        private bool _inScopeMode = false;
        public float mouseSensitivity = 100f;
        private Camera _cameraComponent;

        public float thirdPersonFov = 60f;
        public float scopeMinFov = 2f;
        public float scopeMaxFov = 55f;

        public ShipMain shipToFollow;

        public Transform swivel, stick, cameraTransform;
        public static Camera CurrentCamera { get; private set; }

        private static ShipCamera _instance;
        public static ShipCamera Instance => _instance;

        private void Awake() {
            if (_instance != null && _instance != this) {
                Destroy(gameObject);
            }
            else {
                _instance = this;
                Debug.Log("Set instance to " + _instance.name);    
            }
        
            _cameraComponent = GetComponentInChildren<Camera>();
            GunTargetingMask = LayerMask.GetMask("Water", "Targetable");
            WaterMask = LayerMask.GetMask("Water");
        
            CurrentCamera = Camera.main;

            swivel = transform.GetChild(0);
            stick = swivel.GetChild(0);
            cameraTransform = stick.GetChild(0);
        
            _xRotation = cameraTransform.localRotation.eulerAngles.x;
            _yRotation = swivel.rotation.eulerAngles.y;
        
            _thirdPersonScrollLevel = 0.3f;

            Cursor.lockState = CursorLockMode.Locked;
        }

        private void LateUpdate() {
            UpdateMouseTarget();

            if (Input.GetKeyDown(KeyCode.LeftShift)) {
                SwitchCameraMode();
            }
            else {
                if (_inScopeMode) {
                    ScopedCameraUpdate();
                }
                else {
                    ThirdPersonCameraUpdate();
                }
            }

            ShipUI.Instance.RefreshMarkers();
        }

        private void SwitchCameraMode() {
            _inScopeMode = !_inScopeMode;

            if (_inScopeMode) {
                PostProcessor.Instance.EnableVignette();
                ScopedCameraUpdate();
            }
            else {
                PostProcessor.Instance.DisableVignette();
                _cameraComponent.fieldOfView = thirdPersonFov;
                ThirdPersonCameraUpdate();
            }
        }

        private void ThirdPersonCameraUpdate() {
            swivel.position = shipToFollow.transform.position;
        
            var mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
            var mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
            var mouseScrollWheel = Input.GetAxis("Mouse ScrollWheel") * _mouseScrollSensitivity * Time.deltaTime;


            _yRotation += mouseX;
            _yRotation = Mathf.Repeat(_yRotation, 360f);

            _thirdPersonScrollLevel += -mouseScrollWheel;
            _thirdPersonScrollLevel = Mathf.Clamp(_thirdPersonScrollLevel, 0f, 1f);


            Vector3 oldCameraPosition = cameraTransform.position;

            var swivelXAngle = Mathf.Lerp(15f, 30f, _thirdPersonScrollLevel);
            swivel.localRotation = Quaternion.Euler(swivelXAngle, _yRotation, 0f);

            if (mouseScrollWheel != 0f) {
                var stickDistance = Mathf.Lerp(20f, 120f, _thirdPersonScrollLevel);
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
        }

        private void ScopedCameraUpdate() {
            swivel.position = shipToFollow.transform.position;
        
            var mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
            var mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
            var mouseScrollWheel = Input.GetAxis("Mouse ScrollWheel") * _mouseScrollSensitivity * Time.deltaTime;

        
            _scopedScrollLevel += -mouseScrollWheel;
            _scopedScrollLevel = Mathf.Clamp(_scopedScrollLevel, 0f, 1f);
            float currentFov = Mathf.Lerp(scopeMinFov, scopeMaxFov, _scopedScrollLevel);
        
            _yRotation += mouseX * currentFov / scopeMaxFov;
            _yRotation = Mathf.Repeat(_yRotation, 360f);
        
            float cameraHeight = Mathf.Lerp(10f, 60f, 1-_scopedScrollLevel);
        
            _xRotation -= mouseY * currentFov / scopeMaxFov;
            _xRotation = Mathf.Clamp(_xRotation, -1f, 45f);

            swivel.localRotation = Quaternion.Euler(0f, _yRotation, 0f);
            _cameraComponent.fieldOfView = currentFov;
            stick.localPosition = new Vector3(0f, cameraHeight, 0f);
            cameraTransform.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);

            float vignetteLevel = Mathf.Lerp(0.3f, 0.5f, 1f-_scopedScrollLevel);
            PostProcessor.Instance.SetVignetteIntensity(vignetteLevel);
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
                if (t == shipToFollow.transform) {
                    continue;
                }

                var isValid = true;
                while (t.parent != null) {
                    t = t.parent;
                    if (t != shipToFollow.transform) {
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
    
        /*
         * SHIP CONTROLLER
         */

        public ShipGearInput GetVerticalInput() {
            if (Input.GetKeyDown(KeyCode.W)) {
                return ShipGearInput.Raise;
            } else if (Input.GetKeyDown(KeyCode.S)) {
                return ShipGearInput.Lower;
            }
            else {
                return ShipGearInput.None;
            }
        }

        public float GetHorizontalInput() {
            return Input.GetAxis("Horizontal");
        }

        public Vector3 GetAimPoint() {
            return RayCastGunTargetingHit.point;
        }

        public bool GetFireInput() {
            return Input.GetMouseButton(0);
        }

        public bool GetTorpedoInput() {
            return Input.GetKeyDown(KeyCode.Q);
        }

        public static IShipController AcquireCamera(ShipMain shipToFollow) {
            Instance.shipToFollow = shipToFollow;
            ShipUI.Instance.AcquireShip(shipToFollow);
            return Instance;
        }
    }
}