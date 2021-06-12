﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class ShellImpact : MonoBehaviour {
    public const float ShellRayMaxDistance = 30f;
    public const int ShellTargetableLayerMask = 1 << 3;
    public float health = 3000f;

    public void CalculateImpact(Vector3 impactPosition, Vector3 directionVector, float shellPower) {
        var hitComponents = GenerateHitComponentsList(impactPosition, directionVector);

        #if UNITY_EDITOR
        var b = new StringBuilder();
        foreach (var hit in hitComponents) {
            b.Append(hit.name).Append(" ");
        }
        Debug.Log("Hit components: " + b);
        #endif
        
        CalculateDamage(hitComponents, shellPower);
    }


    private List<BoatComponentDamage> GenerateHitComponentsList(Vector3 impactPosition, Vector3 directionVector) {
        var impactRay = new Ray(impactPosition, directionVector);
        var hits = new RaycastHit[20];
        var numHits = Physics.RaycastNonAlloc(impactRay, hits, ShellRayMaxDistance, ShellTargetableLayerMask);

        var hitsSortedByDistance = new RaycastHit[numHits];
        Array.Copy(hits, hitsSortedByDistance, numHits);
        hitsSortedByDistance = hitsSortedByDistance.OrderBy(h => (h.transform.position - impactPosition).magnitude)
            .ToArray();

        var hitComponents = new List<BoatComponentDamage>();
        for (int i = 0; i < numHits; i++) {
            var hitTransform = hitsSortedByDistance[i].collider.transform;
            if (hitTransform == transform || hitTransform.parent != transform) {
                continue;
            }
            var hitComponent = hitTransform.GetComponent<BoatComponentDamage>();
            if (hitComponent == null) {
                throw new UnityException("Failed to find Damage Component. Forgot to set it in editor?");
            }
            hitComponents.Add(hitComponent);
        }
        return hitComponents;
    }

    private void CalculateDamage(List<BoatComponentDamage> hitComponents, float shellPower) {
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
}