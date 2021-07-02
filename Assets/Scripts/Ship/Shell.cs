using System;
using PhysicsUtilities;
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
        

        private void FixedUpdate() {
            CheckForImpact();
        }

        private void CheckForImpact() {
            Vector3 velocity = Rigidbody.velocity;
            RaycastHit[] hits = Raycasting.SortedRaycast(
                Rigidbody.position,
                velocity,
                velocity.magnitude * Time.fixedDeltaTime,
                5,
                (int) LayerMasks.ShipGunTarget
            );
            foreach (RaycastHit hit in hits) {
                bool destructiveHit = CalculateImpact(hit);
                if (destructiveHit) {
                    Destroy(gameObject);
                    return;
                }
            }
        }

        /// <summary>
        /// Calculates a shell impact.
        /// </summary>
        /// <param name="hit"></param>
        /// <returns>Returns true if hit should destroy the projectile</returns>
        private bool CalculateImpact(RaycastHit hit) {
            Transform hitTransform = hit.transform;
            Vector3 hitPosition = hit.point;
            
            int collisionLayerMask = 1 << hitTransform.gameObject.layer;
            
            if ((collisionLayerMask & (int) LayerMasks.Water) != 0) {
                Instantiate(waterSplashPrefab, hitPosition, Quaternion.Euler(-90f, 0f, 0f));
            }
            
            if ((collisionLayerMask & (int) LayerMasks.Land) != 0) {
                Instantiate(explosionPrefab, hitPosition, Quaternion.identity);
                return true;
            }
            
            if ((collisionLayerMask & ShipDamageModule.ShellTargetableLayerMask) != 0 &&
                hitTransform.transform != shellOwner) {
                Transform thisTransform = transform;
                Vector3 shellVelocity = Rigidbody.velocity;
                Vector3 traceStartPoint = thisTransform.position;

                ShipDamageModule damageModule = hitTransform.GetComponentInParent<ShipDamageModule>();
                
                Instantiate(explosionPrefab, hitPosition, Quaternion.identity);

                damageModule.CalculateImpact(traceStartPoint, shellVelocity.normalized, ShellPower);
                return true;
            }

            return false;
        }
    }
}