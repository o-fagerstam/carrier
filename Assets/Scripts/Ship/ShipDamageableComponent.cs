using UnityEngine;

namespace Ship {
    public class ShipDamageableComponent : MonoBehaviour {
        public float armor;
        public float damagePointValue;
        [SerializeField] private ShipDamageModule parent;
        public float shellPowerReduction;
    }
}