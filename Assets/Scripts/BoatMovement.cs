using System;
using UnityEngine;

public class BoatMovement : MonoBehaviour {
    [SerializeField] private float enginePower = 5000f;
    [SerializeField] private Rigidbody hullRigidbody;
    [SerializeField] private float rudderPower = 10000f;
    public Controller controller = Controller.None;
    private float _verticalInputAccumulator = 0f;
    private float _horizontalInputAccumulator = 0f;
    private float _steeringAngle;

    [SerializeField] private WheelCollider frontLeftW, frontRightW, backLeftW, backRightW;

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
        frontLeftW.steerAngle = _steeringAngle;
        frontRightW.steerAngle = _steeringAngle;
    }

    private void Accelerate() {
        var torque = _verticalInputAccumulator * enginePower;
        frontLeftW.motorTorque = torque;
        frontRightW.motorTorque = torque;
        backLeftW.motorTorque = torque;
        backRightW.motorTorque = torque;
    }

    private void ReduceHorizontalDrift() {
        hullRigidbody.angularVelocity *= 0.95f;
    }
    

    private void ResetInputAccumulators() {
        _verticalInputAccumulator = 0f;
        _horizontalInputAccumulator = 0f;
    }
}