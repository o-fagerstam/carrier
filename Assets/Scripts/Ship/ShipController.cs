using UnityEngine;

namespace Ship {
    public abstract class ShipController {
        protected readonly Ship _controlledShip;
        protected ShipController(Ship ship) {
            _controlledShip = ship;
        }
        public abstract float GetVerticalInput();
        public abstract float GetHorizontalInput();
        public abstract Vector3 GetAimPoint();
        public abstract bool GetFireInput();
    }
}