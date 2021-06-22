using Unity.Mathematics;
using UnityEngine;

namespace Ship {
    public class Shell : MonoBehaviour {
        public Transform shellOwner;
        public float ShellPower;
        public Rigidbody Rigidbody { get; private set; }
        public ParticleSystem explosionPrefab;
        public ParticleSystem waterSplashPrefab;

        private void Awake() {
            Rigidbody = GetComponent<Rigidbody>();
        }

        protected virtual void Update() {
            transform.forward = Rigidbody.velocity.normalized;

            if (transform.position.y < -5) {
                Destroy(gameObject);
            }
        }

        private void OnTriggerEnter(Collider other) {

            int collisionLayerMask = 1 << other.gameObject.layer;

            if ((collisionLayerMask & ShipCamera.WaterMask) != 0) {
                Instantiate(waterSplashPrefab, transform.position, quaternion.Euler(-90f, 0f, 0f));
            }
            
            if ((collisionLayerMask & ShellImpact.ShellTargetableLayerMask) != 0 &&
                other.transform != shellOwner) {
                Transform thisTransform = transform;
                Vector3 shellVelocity = Rigidbody.velocity;
                Vector3 traceStartPoint = thisTransform.position - shellVelocity * Time.deltaTime * 3f;

                Transform targetTransform = other.transform;
                var targetShellImpact = targetTransform.GetComponent<ShellImpact>();
                while (targetShellImpact == null) {
                    targetTransform = targetTransform.parent;
                    targetShellImpact = targetTransform.GetComponent<ShellImpact>();
                }
                
                Instantiate(explosionPrefab, transform.position - Rigidbody.velocity * Time.fixedDeltaTime * 2, Quaternion.identity);

                targetShellImpact.CalculateImpact(traceStartPoint, shellVelocity.normalized, ShellPower);
                Destroy(gameObject);
            }
        }
    }
}