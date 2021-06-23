using Ship;
using UnityEngine;
using UnityEngine.UI;

namespace UI {
    public class GearSlider : MonoBehaviour, ShipTrackingUIComponent {
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
            _trackedShip.OnChangeGearLevel += OnChangeGearLevel;
        }

        public void ReleaseShip() {
            _trackedShip.OnChangeGearLevel -= OnChangeGearLevel;
            _trackedShip = null;
            _slider.value = 0.5f;
        }

        private void OnChangeGearLevel(float gearLerp) {
            _slider.value = gearLerp;
        }
    }
}