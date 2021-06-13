using UnityEngine;

namespace PhysicsUtilities {
    public static class ProjectileMotion {
        public static readonly float G = -Physics.gravity.y;
    
        public static bool CalculateFiringAngle(Vector3 deltaPosition, float v0, out float angle, bool directFire = true) {
            var d = new Vector2(deltaPosition.x, deltaPosition.z).magnitude;
            var h = deltaPosition.y;
            
            CalculateFiringAngle(d, h, v0, G, out angle, directFire);
            CalculateFiringAngle(d, -h, v0, -G, out var reverseAngle, directFire);

            return CalculateFiringAngle(d, h, v0, G, out angle, directFire);
        }

        private static bool CalculateFiringAngle(float d, float h, float v0, float g, out float angle, bool directFire) {
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

        public static float CalculateProjectileDistance(float angle, float h, float v0) {
            return CalculateProjectileDistance(angle, h, v0, G);
        }

        private static float CalculateProjectileDistance(float angle, float h, float v0, float g) {
            var a = angle * Mathf.Deg2Rad;
            var sqrt = Mathf.Sqrt(v0 * v0 * Mathf.Sin(a) * Mathf.Sin(a) + 2 * g * h);
            return v0 * Mathf.Cos(a) * (v0 * Mathf.Sin(a) + sqrt) / g;
        }
    }
}
