using System.Collections.Generic;
using PhysicsUtilities;
using UnityEngine;

public class ShellImpact : MonoBehaviour {
    public const float ShellRayMaxDistance = 70f;
    public const int ShellTargetableLayerMask = 1 << 3;
    public float health = 3000f;

    public void CalculateImpact(Vector3 impactPosition, Vector3 directionVector, float shellPower) {
        var hitComponents = GenerateHitComponentsList(impactPosition, directionVector);
        CalculateDamage(hitComponents, shellPower);
    }


    private List<BoatComponentDamage> GenerateHitComponentsList(Vector3 impactPosition, Vector3 directionVector) {
        var sortedHits = Raycasting.SortedRaycast(
            impactPosition,
            directionVector,
            ShellRayMaxDistance,
            20,
            ShellTargetableLayerMask
        );

        var hitComponents = new List<BoatComponentDamage>();
        for (var i = 0; i < sortedHits.Length; i++) {
            Transform hitTransform = sortedHits[i].collider.transform;
            if (hitTransform == transform || hitTransform.parent != transform) {
                continue;
            }

            var hitComponent = hitTransform.GetComponent<BoatComponentDamage>();
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

    private void CalculateDamage(List<BoatComponentDamage> hitComponents, float shellPower) {
        var currentShellPower = shellPower;
        foreach (BoatComponentDamage hitComponent in hitComponents) {
            currentShellPower -= hitComponent.armor;
            if (currentShellPower <= 0f) {
                break;
            }

            health -= hitComponent.damagePointValue;
            currentShellPower -= hitComponent.shellPowerReduction;
            if (currentShellPower < 0f) {
                break;
            }
        }

        if (health <= 0f) {
            Destroy(gameObject);
        }
    }
}