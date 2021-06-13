using System;
using UnityEngine;

namespace PhysicsUtilities {
    public static class ProjectileMotion {
        public static float G = Physics.gravity.y;
    
        public static bool CalculateFiringAngle(Vector3 deltaPosition, float v0, out float angle, bool directFire = true) {
            var d = new Vector2(deltaPosition.x, deltaPosition.z).magnitude;
            var h = -deltaPosition.y;
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

        public static float CalculateFiringDistance(Vector3 directionOfFire, float heightDiff, float v0) {
            var directionOfGround = new Vector3(directionOfFire.x, 0f, directionOfFire.z).normalized;
            var angleRadians = Vector3.Angle(directionOfFire, directionOfGround) * Mathf.Deg2Rad;
            var vx = Mathf.Sin(angleRadians) * v0;
            var vy = Mathf.Cos(angleRadians) * v0;
            var g = -UnityEngine.Physics.gravity.y;
            var h = heightDiff;
            return vx / g * (vy + Mathf.Sqrt(vy * vy + 2 * g * h));
        }


    }
}
