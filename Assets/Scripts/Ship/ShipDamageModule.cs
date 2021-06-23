using System;
using System.Collections.Generic;
using PhysicsUtilities;
using UnityEngine;

namespace Ship {
    public class ShipDamageModule : MonoBehaviour {
        public const float ShellRayMaxDistance = 70f;
        public const int ShellTargetableLayerMask = 1 << 3;
        public float maxHealth = 3000f;
        public float health;

        public delegate void OnDamageTaken(float damageTaken, float healthRemaining, float maxHealth);

        public OnDamageTaken onDamageTaken;

        private void Awake() {
            health = maxHealth;
        }

        public void CalculateImpact(Vector3 impactPosition, Vector3 directionVector, float shellPower) {
            var hitComponents = GenerateHitComponentsList(impactPosition, directionVector);
            CalculateDamage(hitComponents, shellPower);
        }


        private List<ShipDamageableComponent> GenerateHitComponentsList(Vector3 impactPosition, Vector3 directionVector) {
            var sortedHits = Raycasting.SortedRaycast(
                impactPosition,
                directionVector,
                ShellRayMaxDistance,
                20,
                ShellTargetableLayerMask
            );

            var hitComponents = new List<ShipDamageableComponent>();
            for (var i = 0; i < sortedHits.Length; i++) {
                Transform hitTransform = sortedHits[i].collider.transform;
                if (hitTransform == transform || hitTransform.parent != transform) {
                    continue;
                }

                var hitComponent = hitTransform.GetComponent<ShipDamageableComponent>();
                if (hitComponent == null) {
                    throw new UnityException("Failed to find Damage Component. Forgot to set it in editor?");
                }

                if (hitComponents.Contains(hitComponent)) {
                    continue;
                }

                hitComponents.Add(hitComponent);
            }

            return hitComponents;
        }

        private void CalculateDamage(List<ShipDamageableComponent> hitComponents, float shellPower) {
            var currentShellPower = shellPower;
            float totalDamage = 0f;
            foreach (ShipDamageableComponent hitComponent in hitComponents) {
                currentShellPower -= hitComponent.armor;
                if (currentShellPower <= 0f) {
                    break;
                }

                totalDamage += hitComponent.damagePointValue;
                currentShellPower -= hitComponent.shellPowerReduction;
                if (currentShellPower < 0f) {
                    break;
                }
            }

            health -= totalDamage;
            onDamageTaken?.Invoke(totalDamage, health, maxHealth);

            if (health <= 0f) {
                Destroy(gameObject);
            }
        }
    }
}