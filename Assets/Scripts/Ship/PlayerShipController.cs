using System;
using System.Linq;
using CommandMode;
using ServiceLocator;
using UnityEngine;

namespace Ship {
    public class PlayerShipController : MonoBehaviourService, IShipController, IPlayerCameraAcquirable {
        public static Ray MouseRay;
        public static RaycastHit RayCastGunTargetingHit;
        public static bool RayCastMadeGunTargetingHit;
        private readonly float _mouseScrollSensitivity = 100f;
        private float _xRotation, _yRotation, _thirdPersonScrollLevel;
        private float _scopedScrollLevel = 1f;
        private bool _inScopeMode = false;
        public float mouseSensitivity = 100f;
        private PlayerCamera _playerCamera;
        private bool _hasControl = false;
        private bool _acquiredControlThisFrame;

        public ShipMain shipToFollow;

        public Transform swivel, stick, cameraHolderTransform;

        private ShipUi _shipUi;

        private const float ScopeMinFov = 2f;
        private const float ScopeMaxFov = 55f;

        public event Action<ShipMain> OnAcquireCamera;
        public event Action OnReleaseCamera;

        protected override void Awake() {
            base.Awake();

            swivel = transform.GetChild(0);
            stick = swivel.GetChild(0);
            cameraHolderTransform = stick.GetChild(0);
        
            _xRotation = cameraHolderTransform.localRotation.eulerAngles.x;
            _yRotation = swivel.rotation.eulerAngles.y;
        
            _thirdPersonScrollLevel = 0.3f;

            _playerCamera = FindObjectOfType<PlayerCamera>();
        }

        private void Start() {
            _shipUi = MonoBehaviourServiceLocator.Current.Get<ShipUi>();
        }

        private void Update()
        {
            if (!_hasControl) {
                return;
            }

            shipToFollow.ReceiveSteeringInput(GetVerticalInput(), Input.GetAxis("Horizontal"));
            shipToFollow.ReceiveAimInput(RayCastGunTargetingHit.point);

            if (Input.GetMouseButton(0)) {
                shipToFollow.ReceiveFireInput();
            }

            if (Input.GetKeyDown(KeyCode.Q)) {
                shipToFollow.ReceiveTorpedoInput();
            }

        }

        private void LateUpdate() {
            if (!_hasControl) {
                return;
            }

            if (!_acquiredControlThisFrame && Input.GetKeyDown(KeyCode.Tab)) {
                _playerCamera.SwitchController(MonoBehaviourServiceLocator.Current.Get<CommandModeController>());
                return;
            }
            
            UpdateMouseTarget();

            if (Input.GetKeyDown(KeyCode.LeftShift)) {
                SwitchScopeMode();
            }
            else {
                if (_inScopeMode) {
                    ScopedCameraUpdate();
                }
                else {
                    ThirdPersonCameraUpdate();
                }
            }

            if (_acquiredControlThisFrame) {
                _acquiredControlThisFrame = false;
            }
            
            
        }

        public void AcquireCamera() {
            _hasControl = true;
            _acquiredControlThisFrame = true;
            _playerCamera.FollowTransform(cameraHolderTransform);
            Cursor.lockState = CursorLockMode.Locked;
            if (_inScopeMode) {
                _playerCamera.SetMode(PlayerCamera.CameraMode.ShipScope);
            }
            else {
                _playerCamera.SetMode(PlayerCamera.CameraMode.ShipNormal);
            }

            OnAcquireCamera?.Invoke(shipToFollow);
        }

        public void ReleaseCamera() {
            _hasControl = false;
            OnReleaseCamera?.Invoke();
        }


        /*
         * CAMERA CONTROLLER
         */

        private void SwitchScopeMode() {
            _inScopeMode = !_inScopeMode;

            if (_inScopeMode) {
                _playerCamera.SetMode(PlayerCamera.CameraMode.ShipScope);
            }
            else {
                _playerCamera.SetMode(PlayerCamera.CameraMode.ShipNormal);
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
            
            Vector3 oldCameraPosition = cameraHolderTransform.position;

            var swivelXAngle = Mathf.Lerp(15f, 30f, _thirdPersonScrollLevel);
            swivel.localRotation = Quaternion.Euler(swivelXAngle, _yRotation, 0f);

            if (mouseScrollWheel != 0f) {
                var stickDistance = Mathf.Lerp(20f, 120f, _thirdPersonScrollLevel);
                stick.localPosition = new Vector3(0f, 0f, -stickDistance);

                Vector3 newCameraPosition = cameraHolderTransform.position;
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

            cameraHolderTransform.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);
        }

        private void ScopedCameraUpdate() {
            swivel.position = shipToFollow.transform.position;
        
            var mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
            var mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
            var mouseScrollWheel = Input.GetAxis("Mouse ScrollWheel") * _mouseScrollSensitivity * Time.deltaTime;

            _scopedScrollLevel += -mouseScrollWheel;
            _scopedScrollLevel = Mathf.Clamp(_scopedScrollLevel, 0f, 1f);
            float currentFov = Mathf.Lerp(ScopeMinFov, ScopeMaxFov, _scopedScrollLevel);
        
            _yRotation += mouseX * currentFov / ScopeMaxFov;
            _yRotation = Mathf.Repeat(_yRotation, 360f);
        
            float cameraHeight = Mathf.Lerp(10f, 60f, 1-_scopedScrollLevel);
        
            _xRotation -= mouseY * currentFov / ScopeMaxFov;
            _xRotation = Mathf.Clamp(_xRotation, -1f, 45f);

            swivel.localRotation = Quaternion.Euler(0f, _yRotation, 0f);
            _playerCamera.SetFov(currentFov);
            stick.localPosition = new Vector3(0f, cameraHeight, 0f);
            cameraHolderTransform.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);

            float vignetteLevel = Mathf.Lerp(0.3f, 0.5f, 1f-_scopedScrollLevel);
            PostProcessor.Instance.SetVignetteIntensity(vignetteLevel);
        }

        private void UpdateMouseTarget() {
            MouseRay = _playerCamera.Camera.ViewportPointToRay(new Vector3(0.5f, 0.5f));
            var hits = new RaycastHit[10];
            var numHits = Physics.RaycastNonAlloc(MouseRay, hits, _playerCamera.Camera.farClipPlane, (int) LayerMasks.ShipGunTarget);


            if (numHits == 0) {
                RayCastMadeGunTargetingHit = false;
                return;
            }
            
            Array.Resize(ref hits, numHits);
            hits = hits.OrderBy(h => (h.point - cameraHolderTransform.position).magnitude).ToArray();

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

        private ShipGearInput GetVerticalInput() {
            if (Input.GetKeyDown(KeyCode.W)) {
                return ShipGearInput.Raise;
            } else if (Input.GetKeyDown(KeyCode.S)) {
                return ShipGearInput.Lower;
            } else {
                return ShipGearInput.NoInput;
            }
        }
        
        public IShipController AcquireShip(ShipMain newShipToFollow) {
            if (shipToFollow != null) {
                shipToFollow.UpdateControllerType(VehicleUserType.Ai);
            }
            
            shipToFollow = newShipToFollow;
            if (_shipUi == null) {
                _shipUi = MonoBehaviourServiceLocator.Current.Get<ShipUi>();
            }
            _shipUi.AcquireShip(newShipToFollow);
            return this;
        }
    }
}