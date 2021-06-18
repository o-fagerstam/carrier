using System;
using System.Collections.Generic;
using UnityEngine;

public class ShipMain : MonoBehaviour {
    private float _horizontalInputAccumulator;
    private float _steeringAngle;
    private float _verticalInputAccumulator;
    public VehicleUserType vehicleUserType = VehicleUserType.None;
    [SerializeField] private float enginePower;
    [SerializeField] private List<WheelCollider> engineWheels;
    [SerializeField] private Rigidbody hullRigidbody;
    [SerializeField] private float maxSpeed;

    public float maxSteerAngle;
    [SerializeField] private List<WheelCollider> rudderWheels;
    public ShipController shipController;
    public bool isActive;

    private void Awake() {
        hullRigidbody.centerOfMass = Vector3.down * transform.localScale.y * 0.4f;
        UpdateControllerType();
    }
    
    private void UpdateControllerType() {
        switch (vehicleUserType) {
            case VehicleUserType.Human:
                shipController = new HumanShipController();
                isActive = true;
                break;
            case VehicleUserType.Ai:
                throw new NotImplementedException("Ship gun AI not implemented");
            case VehicleUserType.None:
                isActive = false;
                break;
        }
    }

    private void Update() {
        if (isActive) {
            GetInput();
        }
    }

    private void FixedUpdate() {
        if (isActive) {
            Steer();
            Accelerate();
            ResetInputAccumulators();
        }
        ReduceHorizontalDrift();
    }

    private void GetInput() {
        _verticalInputAccumulator = shipController.GetVerticalInput();
        _horizontalInputAccumulator = shipController.GetHorizontalInput();
    }

    private void Steer() {
        _steeringAngle = maxSteerAngle * _horizontalInputAccumulator;
        foreach (WheelCollider rudderWheel in rudderWheels) {
            rudderWheel.steerAngle = -_steeringAngle;
        }
    }

    private void Accelerate() {
        var torquePerWheel = _verticalInputAccumulator * enginePower / engineWheels.Count;
        Vector3 localVelocity = transform.InverseTransformDirection(hullRigidbody.velocity);
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