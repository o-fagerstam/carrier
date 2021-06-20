using UnityEngine;

namespace Ship {
    public class HumanShipController : ShipController {

        public HumanShipController(Ship ship) : base(ship) { }
        private ShipCameraMode _shipCameraMode = ShipCameraMode.ThirdPerson;

        public override float GetVerticalInput() {
            return Input.GetAxis("Vertical");
        }

        public override float GetHorizontalInput() {
            return Input.GetAxis("Horizontal");
        }

        public override Vector3 GetAimPoint() {
            return ShipCamera.RayCastGunTargetingHit.point;
        }

        public override bool GetFireInput() {
            return Input.GetMouseButton(0);
        }

        public void CheckSwitchCameraEvent() {
            if (Input.GetKeyDown(KeyCode.LeftShift)) {
                SwitchCamera();
            }
        }
        
        private void SwitchCamera() {
            Debug.Log("SwitchCamera called");
            if (_shipCameraMode == ShipCameraMode.ThirdPerson) {
                _shipCameraMode = ShipCameraMode.Scope;
                ScopeShipCamera.Instance.Activate();
            }
            else {
                _shipCameraMode = ShipCameraMode.ThirdPerson;
                ThirdPersonShipCamera.Instance.Activate();
            }
        }
        
        private enum ShipCameraMode {ThirdPerson, Scope}
    }
}
