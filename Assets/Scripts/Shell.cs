using System;
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
            var contactPoint = other.GetContact(0).point;
            var contactDirection = (contactPoint - transform.position).normalized;
            other.gameObject.GetComponent<ShellImpact>()
                .CalculateImpact(contactPoint - contactDirection, contactDirection, ShellPower);
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other) {
        Debug.Log(other.gameObject.name + " layer " + other.gameObject.layer);
        if (((1 << other.gameObject.layer) & ShellImpact.ShellTargetableLayerMask) != 0 &&
            other.transform != shellOwner) {
            Debug.Log("Hit another boat");
            var thisTransform = transform;
            var shellVelocity = ShellRigidBody.velocity;
            var traceStartPoint = thisTransform.position - shellVelocity * Time.deltaTime * 2;

            var targetTransform = other.transform;
            var targetShellImpact = targetTransform.GetComponent<ShellImpact>();
            while (targetShellImpact == null) {
                targetTransform = targetTransform.parent;
                targetShellImpact = targetTransform.GetComponent<ShellImpact>();
            }
            
            Debug.DrawRay(traceStartPoint, shellVelocity.normalized * ShellImpact.ShellRayMaxDistance, Color.red, 1f, false);
            Debug.Break();
            
            targetShellImpact.CalculateImpact(traceStartPoint, shellVelocity.normalized, ShellPower);
            Destroy(gameObject);
        }
    }
}