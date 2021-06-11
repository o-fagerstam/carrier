using UnityEngine;

public class Shell : MonoBehaviour {
    public Rigidbody ShellRigidBody { get; private set; }

    private void Awake() {
        ShellRigidBody = GetComponent<Rigidbody>();
    }

    private void Update() {
        transform.forward = ShellRigidBody.velocity.normalized;
    }
}