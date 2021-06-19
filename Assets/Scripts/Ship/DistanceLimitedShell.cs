using UnityEngine;

namespace Ship {
    public class DistanceLimitedShell : Shell {
        public float maxDistance;
        public float distanceTravelled = 0f;

        protected override void Update() {
            base.Update();
            if (distanceTravelled > maxDistance) {
                Destroy(gameObject);
            }

            distanceTravelled += Rigidbody.velocity.magnitude * Time.deltaTime;
        }
    }
}