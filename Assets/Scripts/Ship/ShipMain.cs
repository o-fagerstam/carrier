using System;
using System.Collections.Generic;
using ServiceLocator;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Ship {
    public class ShipMain : GameUnit {
        private float _rudderInput;
        private int _gearLevel;
        private float _steeringAngle;
        public float rudderAnglesPerSecond;
        public float maxSteerAngle;

        public IShipController shipController;

        public float maxSpeed;
        [SerializeField] private float enginePower;
        [SerializeField] private List<WheelCollider> engineWheels;
        [SerializeField] private List<WheelCollider> rudderWheels;
        
        public ShipGun[] MainGuns { get; private set; }
        public ShipDamageModule ShipDamageModule { get; private set; }

        private const int maxGearLevel = 4;
        private const int minGearlevel = -2;

        private const float OnDestructionDrag = 0.4f;
        private const float DestructionMinDrag = 0.2f;
        private const float TimeFromDestructionToMinDrag = 40f;
        private readonly Vector3 OnDestructionGravityMitigationForce = Vector3.up * (-Physics.gravity.y * 0.995f);
        
        public event Action<float> OnChangeSteeringAngle;
        public event Action<float> OnChangeGearLevel;


        protected override void Awake() {
            base.Awake();
            MainGuns = GetComponentsInChildren<ShipGun>();
            foreach (ShipGun mainGun in MainGuns) {
                mainGun.parentBoat = this;
            }
            
            Rigidbody.centerOfMass = Vector3.down * transform.localScale.y * 0.4f;

            ShipDamageModule = GetComponent<ShipDamageModule>();
            DamageModule = ShipDamageModule;
            AiController = gameObject.AddComponent<AiShipController>();
        }

        private void Start() {
            UpdateControllerType(vehicleUserType);
        }

        public void UpdateControllerType(VehicleUserType type) {
            shipController = type switch {
                VehicleUserType.Human => MonoBehaviourServiceLocator.Current.Get<PlayerShipController>().AcquireShip(this),
                VehicleUserType.Ai => (AiShipController) AiController,
                VehicleUserType.None => null,
                _ => shipController
            };
            vehicleUserType = type;
        }

        private void Update() {
            if (IsActive) {
                GetInput();
            }
        }

        private void FixedUpdate() {
            if (IsActive) {
                Steer();
                Accelerate();
                ResetInputAccumulators();
            }

            if (!alive) {
                Rigidbody.AddForce(OnDestructionGravityMitigationForce, ForceMode.Acceleration);

                float timeSinceDestruction = Time.time - timeOfDestruction;
                if (timeSinceDestruction < TimeFromDestructionToMinDrag) {
                    float dragFactor = Mathf.InverseLerp(
                        0,
                        20f,
                        Mathf.Max(timeSinceDestruction, TimeFromDestructionToMinDrag)
                    );
                    Rigidbody.drag = Mathf.Lerp(OnDestructionDrag, DestructionMinDrag, dragFactor);
                }
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
            } else if (gearInput == ShipGearInput.Zero) {
                _gearLevel = 0;
                OnChangeGearLevel?.Invoke(Mathf.InverseLerp(minGearlevel, maxGearLevel, _gearLevel));
            }
            _rudderInput = shipController.GetHorizontalInput();
        }

        private void Steer() {
            float steeringAngleFactor = Mathf.InverseLerp(-maxSteerAngle, maxSteerAngle, _steeringAngle) * 2f - 1f;
            if ((_rudderInput > 0f && _rudderInput > steeringAngleFactor) ||
                (_rudderInput < 0f && _rudderInput < steeringAngleFactor)) {
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

        public override void Destroy() {
            base.Destroy();

            WheelCollider[] allWheels = GetComponentsInChildren<WheelCollider>();
            foreach (WheelCollider wheel in allWheels) {
                wheel.enabled = false;
            }

            Rigidbody.ResetCenterOfMass();
            Rigidbody.drag = OnDestructionDrag;
            Rigidbody.AddRelativeTorque(Vector3.forward * Random.Range(-0.3f, 0.3f), ForceMode.VelocityChange);
        }
    }
}