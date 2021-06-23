using System;
using Ship;
using UnityEngine;
using UnityEngine.UI;

namespace UI {
    public class RudderSlider : MonoBehaviour, ShipTrackingUIComponent {
        private Slider _slider;
        private ShipMain _trackedShip;

        private void Awake() {
            _slider = GetComponent<Slider>();
        }
        
        public void AcquireShip(ShipMain ship) {
            if (_trackedShip != null) {
                ReleaseShip();
            }

            _trackedShip = ship;
            _trackedShip.OnChangeSteeringAngle += OnChangeSteeringAngle;
            _slider.value = 0.5f;
        }

        public void ReleaseShip() {
            _trackedShip.OnChangeSteeringAngle -= OnChangeSteeringAngle;
            _trackedShip = null;
            _slider.value = 0.5f;
        }

        private void OnChangeSteeringAngle(float angleLerp) {
            _slider.value = angleLerp;
        }
    }
}