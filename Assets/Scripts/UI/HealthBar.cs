using System;
using Ship;
using UnityEngine;
using UnityEngine.UI;

namespace UI {
    public class HealthBar : MonoBehaviour, IShipTrackingUiComponent {
        private ShipDamageModule trackedDamageModule;
        private Slider healthBarSlider;

        private void Awake() {
            healthBarSlider = GetComponent<Slider>();
        }

        public void AcquireShip(ShipMain ship) {
            if (trackedDamageModule != null) {
                ReleaseShip();
            }

            trackedDamageModule = ship.ShipDamageModule;
            trackedDamageModule.OnDamageTaken += OnShipDamageTaken;
            SetSlider(trackedDamageModule.health, trackedDamageModule.maxHealth);
        }

        public void ReleaseShip() {
            trackedDamageModule.OnDamageTaken -= OnShipDamageTaken;
            trackedDamageModule = null;
            SetSlider(0, 1);
        }

        private void OnShipDamageTaken(object sender, ShipDamageModule.OnDamageTakenArgs e) {
            SetSlider(e.healthRemaining, e.maxHealth);
        }

        private void SetSlider(float currentHealth, float maxHealth) {
            healthBarSlider.value = Mathf.InverseLerp(0f, maxHealth, currentHealth);
        }
    }
}