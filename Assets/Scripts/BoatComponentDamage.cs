using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoatComponentDamage : MonoBehaviour {
    [SerializeField] private ShellImpact parent;
    public float damagePointValue;
    public float shellPowerReduction;
    public float armor = 0f;
}
