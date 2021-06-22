using System;
using PhysicsUtilities;
using UnityEngine;

namespace Ship {
    public class Torpedo : MonoBehaviour {
        private Rigidbody _rigidbody;
        [SerializeField] private float acceleration;
        [SerializeField] private float maxSpeed;
        [SerializeField] private float maxRange;
        [SerializeField] private Transform clockwiseRotor;
        [SerializeField] private Transform counterClockwiseRotor;
        [SerializeField] private ParticleSystem waterSprayParticleSystem;
        private bool _waterSprayActivated = false;
        private Quaternion _forwardRotation;
        private float distanceTravelled;
        private bool isRunning;

        private void Awake() {
            _rigidbody = GetComponent<Rigidbody>();
            _forwardRotation = Quaternion.LookRotation(new Vector3(transform.forward.x, 0f, transform.forward.z).normalized);
            Activate();
        }

        private void FixedUpdate() {
            if (!isRunning) {
                return;
            }
            if (transform.position.y < 0f) {
                Quaternion newRotation = Quaternion.RotateTowards(transform.rotation, _forwardRotation, 5f * Time.fixedDeltaTime);
                transform.rotation = newRotation;
                
                if (_rigidbody.velocity.magnitude < maxSpeed) {
                    _rigidbody.AddForce(Vector3.forward * acceleration, ForceMode.Acceleration);
                }
            }

            if (_rigidbody.velocity.y < 0.2f && transform.position.y < -0.3f) {
                _rigidbody.useGravity = false;
                _rigidbody.AddForce(Vector3.up * 15f, ForceMode.Acceleration);
            } else if (_rigidbody.velocity.y > 0f && transform.position.y > -0.3f) {
                _rigidbody.useGravity = true;
            }

            if (!_waterSprayActivated && VectorTools.HorizontalComponent(_rigidbody.velocity).magnitude > 10f) {
                _waterSprayActivated = true;
                waterSprayParticleSystem.Play();
            }

            distanceTravelled += _rigidbody.velocity.magnitude * Time.fixedDeltaTime;
            if (distanceTravelled > maxRange) {
                Destroy(gameObject);
            }
        }

        private void Update() {
            if (!isRunning) {
                return;
            }
            float degreesRotation = 360f * 4f * Time.deltaTime;
            clockwiseRotor.Rotate(clockwiseRotor.forward, degreesRotation);
            counterClockwiseRotor.Rotate(counterClockwiseRotor.forward, -degreesRotation);
        }

        public void Activate() {
            isRunning = true;
        }
    }
}