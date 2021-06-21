using UnityEngine;

namespace PhysicsUtilities {
    public static class ProjectileMotion {
        public static readonly float G = -Physics.gravity.y;

        public static bool FiringAngle(Vector3 deltaPosition, float v0, out float angle, bool directFire = true) {
            var d = new Vector2(deltaPosition.x, deltaPosition.z).magnitude;
            var h = deltaPosition.y;

            FiringAngle(d, h, v0, G, out angle, directFire);

            return FiringAngle(d, h, v0, G, out angle, directFire);
        }

        public static bool FiringAngle(Vector3 deltaPosition, float v0, out float angle, float minElevation,
            float maxElevation, bool directFire = true) {
            var valid = FiringAngle(deltaPosition, v0, out angle, directFire);

            return valid && angle >= minElevation && angle <= maxElevation;
        }

        private static bool FiringAngle(float d, float h, float v0, float g, out float angle, bool directFire) {
            var toSqrt = v0 * v0 * v0 * v0 - g * (g * d * d + 2 * h * v0 * v0);
            if (toSqrt <= 0f) {
                angle = 0f;
                return false;
            }

            if (directFire) {
                angle = Mathf.Atan((v0 * v0 - Mathf.Sqrt(toSqrt)) / (g * d)) * Mathf.Rad2Deg;
            }
            else {
                angle = Mathf.Atan((v0 * v0 + Mathf.Sqrt(toSqrt)) / (g * d)) * Mathf.Rad2Deg;
            }

            return true;
        }

        public static float ProjectileDistance(float angle, float h, float v0) {
            return ProjectileDistance(angle, h, v0, G);
        }

        private static float ProjectileDistance(float angle, float h, float v0, float g) {
            var a = angle * Mathf.Deg2Rad;
            var sqrt = Mathf.Sqrt(v0 * v0 * Mathf.Sin(a) * Mathf.Sin(a) + 2 * g * h);
            return v0 * Mathf.Cos(a) * (v0 * Mathf.Sin(a) + sqrt) / g;
        }

        public static Vector3 PointAtTime(Vector3 launchPos, Vector3 launchVelocity, float t) {
            var v0 = launchVelocity.magnitude;
            Vector3 horizontalDirection = VectorTools.HorizontalComponent(launchVelocity).normalized;
            Vector3 axis = Quaternion.AngleAxis(90, Vector3.up) * horizontalDirection;
            var angle = Vector3.SignedAngle(launchVelocity.normalized, horizontalDirection, axis);
            Vector2 new2dLocation = PointAtTime(angle, v0, t, G);
            return new Vector3(
                launchPos.x + horizontalDirection.x * new2dLocation.x,
                launchPos.y + new2dLocation.y,
                launchPos.z + horizontalDirection.z * new2dLocation.x
            );
        }

        private static Vector2 PointAtTime(float angle, float v0, float t, float g) {
            var a = angle * Mathf.Deg2Rad;
            var v0x = v0 * Mathf.Cos(a);
            var v0y = v0 * Mathf.Sin(a);
            var x = v0x * t;
            var y = v0y * t - 0.5f * g * t * t;
            return new Vector2(x, y);
        }

        /// <summary>
        ///     The expected time a projectile will stay in the air, assuming flat ground.
        /// </summary>
        public static float ExpectedTimeOfFlight(Vector3 launchVelocity) {
            return 2 * launchVelocity.y / G;
        }

        public static float ExpectedTimeOfFlight(float angle, float muzzleVelocity) {
            return 2 * Mathf.Sin(angle * Mathf.Deg2Rad) * muzzleVelocity / G;
        }
    }
}