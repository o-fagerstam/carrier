using System;
using UnityEngine;

namespace Ship {
    public class DamageParticleSystem : MonoBehaviour {
        private ParticleSystem[] _particleSystems;
        public DamageParticleSystemType type;

        private void Awake() {
            _particleSystems = GetComponentsInChildren<ParticleSystem>();
            foreach (ParticleSystem system in _particleSystems) {
                system.Stop();
            }
        }

        public void Play() {
            foreach (ParticleSystem system in _particleSystems) {
                system.Play();
            }
        }

        public enum DamageParticleSystemType {
            Smoke,
            Explosion
        }
    }
}