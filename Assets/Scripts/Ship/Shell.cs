using UnityEngine;

namespace Ship {
    public class Shell : MonoBehaviour {
        public Transform shellOwner;
        public float ShellPower;
        public Rigidbody Rigidbody { get; private set; }

        private void Awake() {
            Rigidbody = GetComponent<Rigidbody>();
        }

        protected virtual void Update() {
            transform.forward = Rigidbody.velocity.normalized;

            if (transform.position.y < -5) {
                Destroy(gameObject);
            }
        }

        private void OnTriggerEnter(Collider other) {
            if (((1 << other.gameObject.layer) & ShellImpact.ShellTargetableLayerMask) != 0 &&
                other.transform != shellOwner) {
                Transform thisTransform = transform;
                Vector3 shellVelocity = Rigidbody.velocity;
                Vector3 traceStartPoint = thisTransform.position - shellVelocity * Time.deltaTime * 3f;

                Transform targetTransform = other.transform;
                var targetShellImpact = targetTransform.GetComponent<ShellImpact>();
                while (targetShellImpact == null) {
                    targetTransform = targetTransform.parent;
                    targetShellImpact = targetTransform.GetComponent<ShellImpact>();
                }

                //Debug.DrawRay(traceStartPoint, shellVelocity.normalized * ShellImpact.ShellRayMaxDistance, Color.red, 0.2f, false);
                //Debug.Break();

                targetShellImpact.CalculateImpact(traceStartPoint, shellVelocity.normalized, ShellPower);
                Destroy(gameObject);
            }
        }
    }
}