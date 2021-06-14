using System;
using System.Linq;
using UnityEngine;

namespace PhysicsUtilities {
    public static class Raycasting {
        /// <summary>
        ///     Max number of subdivisions above y = 0 when tracing a trajectory
        /// </summary>
        private const int
            TrajectoryTraceMaxAboveGroundSubdivisions =
                20;

        /// <summary>
        ///     Max number of subdivisions that are below y = 0 when tracing a trajectory
        /// </summary>
        private const int
            TrajectoryTraceMaxBelowGroundSubdivisions = 5; // 

        /// <summary>
        ///     Sets a minimum of expected air time for a projectile. Gives us a minimum Raycasting length.
        /// </summary>
        private const float
            MinimumProjectileAirTime = 0.5f;

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

        /// <summary>
        ///     Traces a trajectory and returns the first (guaranteed first) hit.
        /// </summary>
        public static bool TraceTrajectoryUntilImpact(Vector3 origin, Vector3 v0, out RaycastHit hit, LayerMask mask) {
            var expectedAirTime = ProjectileMotion.ExpectedTimeOfFlight(v0);
            expectedAirTime = Math.Max(expectedAirTime, MinimumProjectileAirTime);
            var numAboveGroundSubdivisions = 1 + Mathf.RoundToInt(expectedAirTime * 5);
            numAboveGroundSubdivisions = Math.Min(
                numAboveGroundSubdivisions,
                TrajectoryTraceMaxAboveGroundSubdivisions
            );


            var step = 1;
            Vector3 p0 = origin;
            Vector3 p1 = ProjectileMotion.PointAtTime(
                p0,
                v0,
                (float) step++ / numAboveGroundSubdivisions * expectedAirTime
            );
            Debug.DrawLine(p0, p1, Color.red);
            var madeHit = ClosestRaycastHit(p0, p1, 10, mask, out hit);
            while (!madeHit &&
                   step <= TrajectoryTraceMaxAboveGroundSubdivisions + TrajectoryTraceMaxBelowGroundSubdivisions
            ) {
                p0 = p1;
                p1 = ProjectileMotion.PointAtTime(
                    origin,
                    v0,
                    (float) step++ / numAboveGroundSubdivisions * expectedAirTime
                );
                Debug.DrawLine(p0, p1, Color.red);
                madeHit = ClosestRaycastHit(p0, p1, 10, mask, out hit);
            }

            return madeHit;
        }
    }
}