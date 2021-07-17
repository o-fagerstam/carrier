using System;
using UnityEngine;

namespace Ship {
    public class SmokeParticleSystem : MonoBehaviour {
        private ParticleSystem _particleSystem;
        private void Awake() {
            _particleSystem = GetComponent<ParticleSystem>();
            _particleSystem.Stop();
        }

        public void Play() {
            _particleSystem.Play();
        }

        public void StopEmitting() {
            _particleSystem.Stop();
        }
    }
}