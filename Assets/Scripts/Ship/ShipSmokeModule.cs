using System;
using UnityEngine;

namespace Ship {
    public class ShipSmokeModule : MonoBehaviour {
        private const float FirstSmokePercentage = 0.5f;
        private ShipDamageModule _damageModule;
        private SmokeParticleSystem[] _smokeParticleSystems;


        private void Awake() {
            _damageModule = GetComponentInParent<ShipDamageModule>();
            _damageModule.OnDamageTaken += OnShipDamaged;

            _smokeParticleSystems = GetComponentsInChildren<SmokeParticleSystem>();
        }

        private void OnShipDamaged(object sender, ShipDamageModule.OnDamageTakenArgs args) {
            float healthPercent = args.healthRemaining / args.maxHealth;

            if (healthPercent < FirstSmokePercentage) {
                int numOfSystems = _smokeParticleSystems.Length;
                float smokeFactor = (FirstSmokePercentage - healthPercent) / FirstSmokePercentage;
                int numSmokes = Mathf.CeilToInt(smokeFactor * numOfSystems);
                numSmokes = Math.Min(numSmokes, numOfSystems);

                for (int i = 0; i < numSmokes; i++) {
                    _smokeParticleSystems[i].Play();
                }
            }
        }

        private void OnDestroy() {
            _damageModule.OnDamageTaken -= OnShipDamaged;
        }
    }
}