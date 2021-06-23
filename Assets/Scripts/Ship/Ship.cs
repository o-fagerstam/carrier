using System.Collections.Generic;
using UnityEngine;

namespace Ship {
    public class Ship : MonoBehaviour {
        private float _horizontalInputAccumulator;
        private float _verticalInputAccumulator;
        private float _steeringAngle;
        public float maxSteerAngle;

        public ShipController shipController;
        public VehicleUserType vehicleUserType = VehicleUserType.None;
        public bool isActive;
        public int team;
        
        [SerializeField] private float maxSpeed;
        [SerializeField] private float enginePower;
        [SerializeField] private List<WheelCollider> engineWheels;
        [SerializeField] private List<WheelCollider> rudderWheels;

        public Rigidbody Rigidbody { get; private set; }
        public ShipGun[] MainGuns { get; private set; }


        private void Awake() {
            MainGuns = GetComponentsInChildren<ShipGun>();
            foreach (ShipGun mainGun in MainGuns) {
                mainGun.parentBoat = this;
            }

            Rigidbody = GetComponent<Rigidbody>();
            Rigidbody.centerOfMass = Vector3.down * transform.localScale.y * 0.4f;
        }

        private void Start() {
            UpdateControllerType();
        }

        private void UpdateControllerType() {
            switch (vehicleUserType) {
                case VehicleUserType.Human:
                    shipController = ShipCamera.AcquireCamera(this);
                    isActive = true;
                    break;
                case VehicleUserType.Ai:
                    shipController = new AiShipController(this);
                    isActive = true;
                    break;
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
            float torquePerWheel = _verticalInputAccumulator * enginePower / engineWheels.Count;
            Vector3 localVelocity = transform.InverseTransformDirection(Rigidbody.velocity);
            float speed = localVelocity.z;
            float speedTorqueModifier = Mathf.Lerp(0f, maxSpeed, Mathf.Abs(speed) / maxSpeed);
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
            Rigidbody.angularVelocity *= 0.98f;
        }


        private void ResetInputAccumulators() {
            _verticalInputAccumulator = 0f;
            _horizontalInputAccumulator = 0f;
        }
    }
}