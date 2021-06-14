using UnityEngine;

namespace PhysicsUtilities {
    public static class VectorTools {
        public static float LaunchAngle(Vector3 v0) {
            Vector3 horizontalDirection = HorizontalComponent(v0).normalized;
            Vector3 axis = Quaternion.AngleAxis(90, Vector3.up) * horizontalDirection;
            return Vector3.SignedAngle(v0.normalized, horizontalDirection, axis);
        }

        public static Vector3 HorizontalComponent(Vector3 v) {
            return new Vector3(v.x, 0f, v.z);
        }
    }
}