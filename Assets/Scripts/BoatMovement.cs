using System.Collections.Generic;
using UnityEngine;

public class BoatMovement : MonoBehaviour {
    [SerializeField] private Rigidbody hullRigidbody;
    [SerializeField] private List<WheelCollider> engineWheels;
    [SerializeField] private List<WheelCollider> rudderWheels;
    [SerializeField] private float maxSpeed;
    [SerializeField] private float enginePower;
    public Controller controller = Controller.None;
    private float _verticalInputAccumulator = 0f;
    private float _horizontalInputAccumulator = 0f;
    private float _steeringAngle;

    public float maxSteerAngle;

    private void Awake() {
        hullRigidbody.centerOfMass = Vector3.down * transform.localScale.y * 0.4f;
    }

    private void Update() {
        if (controller == Controller.Human) {
            GetInput();
        }
    }

    private void FixedUpdate() {
        if (controller == Controller.Human) {
            Steer();
            ReduceHorizontalDrift();
            Accelerate();
            ResetInputAccumulators();
        }
    }

    private void GetInput() {
        _verticalInputAccumulator = Input.GetAxis("Vertical");
        _horizontalInputAccumulator = Input.GetAxis("Horizontal");
    }

    private void Steer() {
        _steeringAngle = maxSteerAngle * _horizontalInputAccumulator;
        foreach (WheelCollider rudderWheel in rudderWheels) {
            rudderWheel.steerAngle = -_steeringAngle;
        }
    }

    private void Accelerate() {
        var torquePerWheel = _verticalInputAccumulator * enginePower / engineWheels.Count;
        var localVelocity = transform.InverseTransformDirection(hullRigidbody.velocity);
        var speed = localVelocity.z;
        var speedTorqueModifier = Mathf.Lerp(0f, maxSpeed, Mathf.Abs(speed) / maxSpeed);
        torquePerWheel -= speedTorqueModifier * Mathf.Sign(speed);

        foreach (WheelCollider engineWheel in engineWheels) {
            engineWheel.motorTorque = torquePerWheel;
            if (speed > maxSpeed) {
                engineWheel.brakeTorque = torquePerWheel;
            }
            else {
                engineWheel.brakeTorque = 0f;
            }
        }

    }

    private void ReduceHorizontalDrift() {
        hullRigidbody.angularVelocity *= 1f;
    }
    

    private void ResetInputAccumulators() {
        _verticalInputAccumulator = 0f;
        _horizontalInputAccumulator = 0f;
    }
}