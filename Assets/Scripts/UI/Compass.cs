using System;
using ServiceLocator;
using Ship;
using Unity.Mathematics;
using UnityEngine;

namespace UI {
    public class Compass : MonoBehaviour, IShipTrackingUiComponent {
        private bool _isTracking;
        private ShipMain _trackedShip;
        [SerializeField] private Transform _compassRing;
        [SerializeField] private Transform _compassArrow;
        private PlayerCamera _playerCamera;

        private void LateUpdate() {
            if (_isTracking) {
                UpdateCompass();
            }
        }

        private void Start() {
            _playerCamera = MonoBehaviourServiceLocator.Current.Get<PlayerCamera>();
        }

        private void UpdateCompass() {
            Camera cam = _playerCamera.Camera;
            float cameraAngle = cam.transform.eulerAngles.y;
            _compassRing.rotation = Quaternion.Euler(0f, 0f, cameraAngle);
            
            float shipAngle = _trackedShip.transform.eulerAngles.y;
            _compassArrow.rotation = Quaternion.Euler(0f, 0f, Mathf.Repeat(cameraAngle - shipAngle, 360f));
        }

        public void AcquireShip(ShipMain ship) {
            if (_isTracking) {
                ReleaseShip();
            }

            _trackedShip = ship;
            _isTracking = true;
        }

        public void ReleaseShip() {
            _isTracking = false;
            _trackedShip = null;
            
            _compassArrow.rotation = Quaternion.identity;
            _compassRing.rotation = Quaternion.identity;
        }
    }
}