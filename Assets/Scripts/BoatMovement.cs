using System;
using UnityEngine;

public class BoatMovement : MonoBehaviour {
    [SerializeField] private float enginePower = 50000f;
    [SerializeField] private Transform hull;
    [SerializeField] private Rigidbody hullRigidbody;
    [SerializeField] private float rudderPower = 10000f;
    public Controller controller = Controller.None;
    private float _verticalInputAccumulator = 0f;
    private float _horizontalInputAccumulator = 0f;

    private void Update() {
        if (controller == Controller.Human) {
            _verticalInputAccumulator += Input.GetAxis("Vertical");
            _horizontalInputAccumulator += Input.GetAxis("Horizontal");
        }
    }

    private void FixedUpdate() {
        if (controller == Controller.Human) {
            HandleHumanMovement();
        }
    }

    private void HandleHumanMovement() {
        var forwardForce = new Vector3(0, 0, _verticalInputAccumulator * Time.deltaTime * enginePower);
        hullRigidbody.AddRelativeForce(forwardForce, ForceMode.Force);

        var rudderForce = new Vector3(0, _horizontalInputAccumulator * Time.deltaTime * rudderPower);
        hullRigidbody.AddRelativeTorque(rudderForce, ForceMode.Force);

        var brakingForce = -10f * Time.deltaTime * hullRigidbody.velocity;
        hullRigidbody.AddForce(brakingForce);

        var rotationBrakingForce = -3f * Time.deltaTime * hullRigidbody.angularVelocity;
        hullRigidbody.AddTorque(rotationBrakingForce);

        _verticalInputAccumulator = 0f;
        _horizontalInputAccumulator = 0f;
    }
}