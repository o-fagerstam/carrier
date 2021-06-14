using System;
using System.Linq;
using UnityEngine;

namespace PhysicsUtilities {
    public static class Raycasting {
        public static RaycastHit[] SortedRaycast(Vector3 origin, Vector3 direction, float distance, int maxHits,
            LayerMask mask) {
            var impactRay = new Ray(origin, direction);
            var hits = new RaycastHit[maxHits];
            var numHits = Physics.RaycastNonAlloc(impactRay, hits, distance, mask);

            if (numHits == 0) {
                return new RaycastHit[0];
            }

            var hitsSortedByDistance = new RaycastHit[numHits];
            Array.Copy(hits, hitsSortedByDistance, numHits);
            hitsSortedByDistance = hitsSortedByDistance
                .OrderBy(h => (h.point - origin).magnitude)
                .ToArray();
            return hitsSortedByDistance;
        }

        public static bool ClosestRaycastHit(Vector3 from, Vector3 to, int maxHits, LayerMask mask,
            out RaycastHit outHit) {
            Vector3 delta = to - from;
            return ClosestRaycastHit(from, delta, delta.magnitude, maxHits, mask, out outHit);
        }

        public static bool ClosestRaycastHit(Vector3 origin, Vector3 direction, float distance, int maxHits,
            LayerMask mask, out RaycastHit outHit) {
            var impactRay = new Ray(origin, direction);
            var hits = new RaycastHit[maxHits];
            var numHits = Physics.RaycastNonAlloc(impactRay, hits, distance, mask);

            if (numHits == 0) {
                outHit = new RaycastHit();
                return false;
            }

            RaycastHit closestHit = hits[0];
            var closestDistance = (hits[0].point - origin).magnitude;
            for (var i = 1; i < numHits; i++) {
                RaycastHit next = hits[i];
                var nextDistance = (next.point - origin).magnitude;
                if (nextDistance < closestDistance) {
                    closestHit = next;
                    closestDistance = nextDistance;
                }
            }

            outHit = closestHit;
            return true;
        }
    }
}