using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class ShellImpact : MonoBehaviour {
    public const float ShellRayMaxDistance = 30f;
    public const int ShellTargetableLayerMask = 1 << 3;
    public float health = 3000f;

    public void CalculateImpact(Vector3 impactPosition, Vector3 directionVector, float shellPower) {
        var impactRay = new Ray(impactPosition, directionVector);
        var hits = new RaycastHit[20];
        var numHits = Physics.RaycastNonAlloc(impactRay, hits, ShellRayMaxDistance, ShellTargetableLayerMask);
        if (numHits == 0) {
            return;
        }

        var hitComponents = new List<BoatComponentDamage>();
        for (var i = 0; i < numHits; i++) {
            var hitTransform = hits[i].collider.transform;
            if (hitTransform == transform || hitTransform.parent != transform) {
                continue;
            }

            var hitComponent = hitTransform.GetComponent<BoatComponentDamage>();
            if (hitComponent == null) {
                throw new UnityException("Failed to find Damage Component. Forgot to set it in editor?");
            }

            hitComponents.Add(hitComponent);
        }

        hitComponents = hitComponents.OrderBy(c => (c.transform.position - impactPosition).magnitude).ToList();
        StringBuilder b = new StringBuilder();
        foreach (var hit in hitComponents) {
            b.Append(hit.name).Append(" ");
        }
        Debug.Log("Hit components: " + b );

        var currentShellPower = shellPower;
        foreach (var hitComponent in hitComponents) {
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

    public void TakeDamage(float damage) {
        health -= damage;
    }
}