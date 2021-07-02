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

            if ((collisionLayerMask & (int) LayerMasks.Water) != 0) {
                Instantiate(waterSplashPrefab, transform.position, Quaternion.Euler(-90f, 0f, 0f));
            }
            
            if ((collisionLayerMask & ShipDamageModule.ShellTargetableLayerMask) != 0 &&
                other.transform != shellOwner) {
                Transform thisTransform = transform;
                Vector3 shellVelocity = Rigidbody.velocity;
                Vector3 traceStartPoint = thisTransform.position - shellVelocity * Time.fixedDeltaTime;

                Transform targetTransform = other.transform;
                ShipDamageModule damageModule = targetTransform.GetComponentInParent<ShipDamageModule>();
                
                Instantiate(explosionPrefab, transform.position - shellVelocity * Time.fixedDeltaTime * 2, Quaternion.identity);

                damageModule.CalculateImpact(traceStartPoint, shellVelocity.normalized, ShellPower);
                Destroy(gameObject);
            }
        }
    }
}