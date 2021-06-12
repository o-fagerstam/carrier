using UnityEngine;

public class Shell : MonoBehaviour {
    public float ShellPower;
    public Transform shellOwner;
    public Rigidbody ShellRigidBody { get; private set; }

    private void Awake() {
        ShellRigidBody = GetComponent<Rigidbody>();
    }

    private void Update() {
        transform.forward = ShellRigidBody.velocity.normalized;

        if (transform.position.y < -5) {
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter(Collision other) {
        Debug.Log(other.gameObject.name + " layer " + other.gameObject.layer);
        if (((1 << other.gameObject.layer) & ShellImpact.ShellTargetableLayerMask) != 0 &&
            other.transform != shellOwner) {
            Debug.Log("Hit another boat");
            var forward = transform.forward;
            other.gameObject.GetComponent<ShellImpact>()
                .CalculateImpact(other.GetContact(0).point - forward, forward, ShellPower);
            Destroy(gameObject);
        }
    }
}