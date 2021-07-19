using System;
using System.Collections.Generic;
using Ship;
using UnityEngine;

namespace CommandMode {
    public class CommandModeController : MonoBehaviour {
        private static CommandModeController _instance;
        public static CommandModeController Instance => _instance;
        private PlayerCamera _playerCamera;

        private bool _hasControl;
        private bool _acquiredControlThisFrame;
        
        private const float HorizontalScrollSpeed = 2000f;
        private const float VerticalScrollSpeed = 2000f;

        private HashSet<GameUnit> _selectedUnits = new HashSet<GameUnit>();

        private void Awake() {
            if (_instance != null && _instance != this) {
                Destroy(gameObject);
            }
            else {
                _instance = this;
            }
            
            _playerCamera = FindObjectOfType<PlayerCamera>();
        }

        public void LateUpdate() {
            if (!_hasControl) {
                return;
            }

            if (!_acquiredControlThisFrame && Input.GetKeyDown(KeyCode.Tab)) {
                ReleaseControl();
                PlayerShipController.Instance.AcquireControl();
            }

            Scroll();
            if (Input.GetMouseButtonDown(0)) {
                TraceSelection();
            }
            
            if (_acquiredControlThisFrame) {
                _acquiredControlThisFrame = false;
            }
        }

        public void AcquireControl() {
            _hasControl = true;
            _acquiredControlThisFrame = true;
            Cursor.lockState = CursorLockMode.None;
            GameManager.Instance.SetGameSpeed(GameManager.GameSpeed.Paused);
            _playerCamera.SetMode(PlayerCamera.CameraMode.Command);
            _playerCamera.FollowTransform(transform);
        }

        public void ReleaseControl() {
            _hasControl = false;
            _playerCamera.Release();
            GameManager.Instance.Resume();
        }
        
        /*
         * CAMERA MOVEMENT
         */

        private void Scroll() {
            float xScroll = Input.GetAxisRaw("Horizontal");
            float zScroll = Input.GetAxisRaw("Vertical");
            float yScroll = Input.GetAxisRaw("Mouse ScrollWheel");

            float horizontalScrollFactor = HorizontalScrollSpeed * Time.unscaledDeltaTime;
            
            transform.Translate(new Vector3(
                xScroll * horizontalScrollFactor,
                -yScroll * VerticalScrollSpeed,
                zScroll * horizontalScrollFactor
                ), Space.World);
        }
        
        /*
         * SELECTION
         */

        private void TraceSelection() {
            Ray mouseRay = _playerCamera.Camera.ScreenPointToRay(Input.mousePosition);
            bool madeHit = Physics.Raycast(
                mouseRay,
                out RaycastHit hit,
                _playerCamera.Camera.farClipPlane,
                (int) LayerMasks.Selectable
            );
            if (!madeHit) {
                return;
            }

            GameUnit hitUnit = hit.transform.GetComponent<GameUnit>();
            if (hitUnit.IsSelected) {
                DeselectUnit(hitUnit);
            }
            else {
                SelectUnit(hitUnit);
            }
        }

        private void SelectUnit(GameUnit u) {
            _selectedUnits.Add(u);
            u.Select();
            u.OnDeath += OnSelectedUnitDeath;
        }

        private void DeselectUnit(GameUnit u) {
            _selectedUnits.Remove(u);
            u.Deselect();
            u.OnDeath -= OnSelectedUnitDeath;
        }

        private void OnSelectedUnitDeath(GameUnit u) {
            DeselectUnit(u);
        }

        private void OnDestroy() {
            foreach (GameUnit unit in _selectedUnits) {
                unit.OnDeath -= OnSelectedUnitDeath;
            }
        }
    }
}