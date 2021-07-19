using System;
using UnityEngine;

public abstract class GameUnit : MonoBehaviour {
    public VehicleUserType vehicleUserType = VehicleUserType.None;
    public int team;


    public bool alive = true;
    protected float timeOfDestruction;

    public bool IsActive => vehicleUserType != VehicleUserType.None && alive;

    public event Action<GameUnit> OnDeath;

    public virtual void Destroy() {
        alive = false;
        timeOfDestruction = Time.time;
        OnDeath?.Invoke(this);
    }
}
