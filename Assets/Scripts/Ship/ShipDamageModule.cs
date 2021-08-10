using System;
using System.Collections.Generic;
using PhysicsUtilities;
using Unit;
using UnityEngine;

namespace Ship {
    public class ShipDamageModule : DamageModule {
        private ShipMain _shipMain;
        
        public const float ShellRayMaxDistance = 70f;

        private void Awake() {
            _shipMain = GetComponent<ShipMain>();
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
                (int) LayerMasks.Targetable
            );

            var hitComponents = new List<ShipDamageableComponent>();
            for (var i = 0; i < sortedHits.Length; i++) {
                Transform hitTransform = sortedHits[i].collider.transform;
                if (hitTransform == transform ) {
                    continue;
                }

                var hitComponent = hitTransform.GetComponent<ShipDamageableComponent>();

                if (hitComponent == null) {
                    throw new UnityException("Failed to find Damage Component. Forgot to set it in editor?");
                }

                if (hitComponents.Contains(hitComponent) || hitComponent.parent != this) {
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

            if (!_shipMain.alive) {
                return;
            }

            health -= totalDamage;
            InvokeOnDamageTaken(this, new OnDamageTakenArgs(totalDamage, health, maxHealth));

            if (health <= 0f) {
                _shipMain.Destroy();
            }
        }
    }
}