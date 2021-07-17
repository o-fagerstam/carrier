using System;
using System.Linq;
using UnityEngine;

namespace Ship {
    public class ShipSmokeModule : MonoBehaviour {
        private const float FirstSmokePercentage = 0.5f;
        private ShipMain _shipMain;
        private ShipDamageModule _damageModule;
        private DamageParticleSystem[] _smokeParticleSystems;
        private DamageParticleSystem[] _explosionParticleSystems;


        private void Awake() {
            _shipMain = GetComponentInParent<ShipMain>();
            _shipMain.OnShipDestroyed += OnShipDestroyed;
        
            _damageModule = GetComponentInParent<ShipDamageModule>();
            _damageModule.OnDamageTaken += OnShipDamaged;

            DamageParticleSystem[] allSystems = GetComponentsInChildren<DamageParticleSystem>();
            _smokeParticleSystems = allSystems
                .Where(e => e.type == DamageParticleSystem.DamageParticleSystemType.Smoke)
                .ToArray();
            _explosionParticleSystems = allSystems
                .Where(e => e.type == DamageParticleSystem.DamageParticleSystemType.Explosion)
                .ToArray();
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

            if (args.healthRemaining < 0f) {
                foreach (DamageParticleSystem system in _explosionParticleSystems) {
                    system.Play();
                }
            }
        }

        private void OnShipDestroyed(ShipMain _) {
            _shipMain.OnShipDestroyed -= OnShipDestroyed;
            foreach (DamageParticleSystem system in _explosionParticleSystems) {
                system.Play();
            }
        }

        private void OnDestroy() {
            _damageModule.OnDamageTaken -= OnShipDamaged;
            _shipMain.OnShipDestroyed -= OnShipDestroyed;
        }
    }
}