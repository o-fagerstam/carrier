using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ship {
    public class ShipMain : MonoBehaviour {
        private float _rudderInput;
        private int _gearLevel;
        private float _steeringAngle;
        public float rudderAnglesPerSecond;
        public float maxSteerAngle;

        public IShipController shipController;
        public VehicleUserType vehicleUserType = VehicleUserType.None;
        public bool isActive;
        public int team;
        
        [SerializeField] private float maxSpeed;
        [SerializeField] private float enginePower;
        [SerializeField] private List<WheelCollider> engineWheels;
        [SerializeField] private List<WheelCollider> rudderWheels;

        public Rigidbody Rigidbody { get; private set; }
        public ShipGun[] MainGuns { get; private set; }
        public ShipDamageModule DamageModule { get; private set; }

        private const int maxGearLevel = 4;
        private const int minGearlevel = -2;

        public event Action<ShipMain> OnShipDestruction;
        public event Action<float> OnChangeSteeringAngle;
        public event Action<float> OnChangeGearLevel;


        private void Awake() {
            MainGuns = GetComponentsInChildren<ShipGun>();
            foreach (ShipGun mainGun in MainGuns) {
                mainGun.parentBoat = this;
            }

            Rigidbody = GetComponent<Rigidbody>();
            Rigidbody.centerOfMass = Vector3.down * transform.localScale.y * 0.4f;

            DamageModule = GetComponent<ShipDamageModule>();
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
            ShipGearInput gearInput = shipController.GetVerticalInput();
            if (gearInput == ShipGearInput.Raise) {
                _gearLevel += 1;
                _gearLevel = Math.Min(maxGearLevel, _gearLevel);
                OnChangeGearLevel?.Invoke(Mathf.InverseLerp(minGearlevel, maxGearLevel, _gearLevel));
            }
            else if (gearInput == ShipGearInput.Lower){
                _gearLevel -= 1;
                _gearLevel = Math.Max(minGearlevel, _gearLevel);
                OnChangeGearLevel?.Invoke(Mathf.InverseLerp(minGearlevel, maxGearLevel, _gearLevel));
            }
            _rudderInput = shipController.GetHorizontalInput();
        }

        private void Steer() {
            if (Mathf.Abs(_rudderInput) > 0.02f) {
                float steeringDiff = rudderAnglesPerSecond * Time.deltaTime * _rudderInput;
                _steeringAngle = Mathf.Clamp(_steeringAngle + steeringDiff, -maxSteerAngle, maxSteerAngle);
            }
            else if (Mathf.Abs(_steeringAngle) > rudderAnglesPerSecond / 10f){
                float steeringDiff = rudderAnglesPerSecond * Time.deltaTime * Mathf.Sign(_steeringAngle) * -1f;
                _steeringAngle += steeringDiff;
            }
            else {
                _steeringAngle = 0f;
            }
            
            OnChangeSteeringAngle?.Invoke(Mathf.InverseLerp(-maxSteerAngle, maxSteerAngle, _steeringAngle));

            foreach (WheelCollider rudderWheel in rudderWheels) {
                rudderWheel.steerAngle = -_steeringAngle;
            }
        }

        private void Accelerate() {
            float torquePerWheel = ((float) _gearLevel / 4) * enginePower / engineWheels.Count;
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
            _rudderInput = 0f;
        }

        private void OnDestroy() {
            OnShipDestruction?.Invoke(this);
        }
    }
}