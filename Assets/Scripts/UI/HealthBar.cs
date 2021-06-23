using System;
using Ship;
using UnityEngine;
using UnityEngine.UI;

namespace UI {
    public class HealthBar : MonoBehaviour {
        private ShipDamageModule trackedDamageModule;
        private Slider healthBarSlider;

        private void Awake() {
            healthBarSlider = GetComponent<Slider>();
        }

        public void AcquireShip(ShipMain ship) {
            if (trackedDamageModule != null) {
                ReleaseShip();
            }

            trackedDamageModule = ship.DamageModule;
            trackedDamageModule.onDamageTaken += OnShipDamageTaken;
            SetSlider(trackedDamageModule.health, trackedDamageModule.maxHealth);
        }

        public void ReleaseShip() {
            trackedDamageModule.onDamageTaken -= OnShipDamageTaken;
            trackedDamageModule = null;
            SetSlider(0, 1);
        }

        private void OnShipDamageTaken(float damagetaken, float healthRemaining, float maxHealth) {
            SetSlider(healthRemaining, maxHealth);
        }

        private void SetSlider(float currentHealth, float maxHealth) {
            healthBarSlider.value = Mathf.InverseLerp(0f, maxHealth, currentHealth);
        }
    }
}