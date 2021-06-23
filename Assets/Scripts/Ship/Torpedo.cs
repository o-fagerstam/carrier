using PhysicsUtilities;
using UnityEngine;

namespace Ship {
    public class Torpedo : MonoBehaviour {
        private Rigidbody _rigidbody;
        [SerializeField] private float acceleration;
        [SerializeField] private float maxSpeed;
        [SerializeField] private float maxRange;
        [SerializeField] private float blastPower;
        [SerializeField] private float fuseLength;
        
        [SerializeField] private Transform clockwiseRotor;
        [SerializeField] private Transform counterClockwiseRotor;
        [SerializeField] private ParticleSystem movementWaterSpray;
        [SerializeField] private ParticleSystem onImpactWaterSplashPrefab;

        private bool _waterSprayActivated = false;
        private Quaternion _forwardRotation;
        private float distanceTravelled;
        private float activationTime;
        private bool isRunning;

        private bool PayloadIsActive => activationTime + fuseLength <= Time.time;

        private void Awake() {
            _rigidbody = GetComponent<Rigidbody>();
            _rigidbody.isKinematic = true;
            _forwardRotation = Quaternion.LookRotation(new Vector3(transform.forward.x, 0f, transform.forward.z).normalized);
        }

        private void FixedUpdate() {
            if (!isRunning) {
                return;
            }
            if (transform.position.y < 0f) {
                Quaternion newRotation = Quaternion.RotateTowards(transform.rotation, _forwardRotation, 5f * Time.fixedDeltaTime);
                transform.rotation = newRotation;
                
                if (_rigidbody.velocity.magnitude < maxSpeed) {
                    _rigidbody.AddForce(transform.forward * acceleration, ForceMode.Acceleration);
                }
            }

            if (_rigidbody.velocity.y < 0.2f && transform.position.y < -0.3f) {
                _rigidbody.useGravity = false;
                _rigidbody.AddForce(Vector3.up * 15f, ForceMode.Acceleration);
            } else if (_rigidbody.velocity.y > 0f && transform.position.y > -0.3f) {
                _rigidbody.useGravity = true;
            }

            if (!_waterSprayActivated && PayloadIsActive) {
                _waterSprayActivated = true;
                movementWaterSpray.Play();
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
            if (transform.parent != null) {
                transform.parent = null;
            }
            _forwardRotation = Quaternion.LookRotation(VectorTools.HorizontalComponent(transform.forward).normalized);
            _rigidbody.isKinematic = false;
            isRunning = true;
            activationTime = Time.time;
        }

        private void OnTriggerEnter(Collider other) {
            int collisionLayerMask = 1 << other.gameObject.layer;
            
            if ((collisionLayerMask & ShipDamageModule.ShellTargetableLayerMask) != 0 &&
                isRunning &&
                PayloadIsActive) {
                Debug.Log("Boom");
                Transform thisTransform = transform;
                Transform targetTransform = other.transform;
                Vector3 torpedoVelocity = _rigidbody.velocity;
                
                ShipDamageModule damageModule = targetTransform.GetComponentInParent<ShipDamageModule>();

                Vector3 traceStartPoint = thisTransform.position - torpedoVelocity * Time.fixedDeltaTime;
                Vector3 splashPosition = VectorTools.HorizontalComponent(transform.position);
                
                Instantiate(onImpactWaterSplashPrefab, splashPosition, Quaternion.Euler(-90f, 0f, 0f));
                damageModule.CalculateImpact(traceStartPoint, torpedoVelocity.normalized, blastPower);
                Destroy(gameObject);
            }
        }
    }
}