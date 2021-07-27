using UnityEngine;

namespace Ship {
    public interface IShipController {
        public ShipGearInput GetVerticalInput();
        public float GetHorizontalInput();
        public Vector3 GetAimPoint();
        public bool GetFireInput();
        public bool GetTorpedoInput();
    }

    public enum ShipGearInput {
        Raise,
        Lower,
        NoInput,
        Zero
    }
}