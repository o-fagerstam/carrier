using System;
using UnityEngine;

namespace Ship {
    public class DamageParticleSystem : MonoBehaviour {
        private ParticleSystem[] _particleSystems;
        public DamageParticleSystemType type;
        public bool isPlaying = false;

        private void Awake() {
            _particleSystems = GetComponentsInChildren<ParticleSystem>();
            Stop();
        }

        private void Update() {
            // Stop producing smoke when system goes too far below the surface
            if (type == DamageParticleSystemType.Smoke && isPlaying && transform.position.y < 0f) {
                isPlaying = false;
                foreach (ParticleSystem system in _particleSystems) {
                    ParticleSystem.EmissionModule emissionModule = system.emission;
                    emissionModule.enabled = false;
                }
            }
        }

        public void Play() {
            isPlaying = true;
            foreach (ParticleSystem system in _particleSystems) {
                system.Play();
            }
        }

        public void Stop() {
            isPlaying = false;
            foreach (ParticleSystem system in _particleSystems) {
                system.Stop();
            }
        }

        public enum DamageParticleSystemType {
            Smoke,
            Explosion
        }
    }
}