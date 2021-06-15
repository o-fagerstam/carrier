using UnityEngine;

public class BoatComponentDamage : MonoBehaviour {
    public float armor;
    public float damagePointValue;
    [SerializeField] private ShellImpact parent;
    public float shellPowerReduction;
}