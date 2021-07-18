using System;
using Ship;
using UnityEngine;

namespace CommandMode {
    public class CommandModeCamera : MonoBehaviour {
        private static CommandModeCamera _instance;
        public static CommandModeCamera Instance => _instance;
        private PlayerCamera _playerCamera;

        private bool _hasControl;
        private bool _acquiredControlThisFrame;
        
        private const float HorizontalScrollSpeed = 2000f;
        private const float VerticalScrollSpeed = 2000f;

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
                ShipCamera.Instance.AcquireControl();
            }

            Scroll();
            
            if (_acquiredControlThisFrame) {
                _acquiredControlThisFrame = false;
            }
        }

        public void AcquireControl() {
            _hasControl = true;
            _acquiredControlThisFrame = true;
            GameManager.Instance.SetGameSpeed(GameManager.GameSpeed.Paused);
            _playerCamera.SetMode(PlayerCamera.CameraMode.Command);
            _playerCamera.FollowTransform(transform);
        }

        public void ReleaseControl() {
            Debug.Log("Releasing control");
            _hasControl = false;
            _playerCamera.Release();
            GameManager.Instance.Resume();
        }

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
    }
}