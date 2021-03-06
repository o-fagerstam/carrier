using UnityEngine;

namespace Ship {
    public class ShipDamageableComponent : MonoBehaviour {
        public float armor;
        public float damagePointValue;
        public ShipDamageModule parent;
        public float shellPowerReduction;
    }
}