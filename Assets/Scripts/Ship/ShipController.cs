using UnityEngine;

namespace Ship {
    public interface ShipController {
        public float GetVerticalInput();
        public float GetHorizontalInput();
        public Vector3 GetAimPoint();
        public bool GetFireInput();
    }
}