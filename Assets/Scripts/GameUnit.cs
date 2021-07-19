using System;
using UnityEngine;

public abstract class GameUnit : MonoBehaviour {
    public VehicleUserType vehicleUserType = VehicleUserType.None;
    public int team;
    
    public bool alive = true;
    protected float timeOfDestruction;

    public bool IsActive => vehicleUserType != VehicleUserType.None && alive;
    public bool IsSelected { get; private set; }

    public event Action<GameUnit> OnDeath;
    public event Action<GameUnit> OnSelected;
    public event Action<GameUnit> OnDeselected;

    public virtual void Destroy() {
        alive = false;
        timeOfDestruction = Time.time;
        OnDeath?.Invoke(this);
    }
    
    public void Select() {
        IsSelected = true;
        OnSelected?.Invoke(this);
    }

    public void Deselect() {
        IsSelected = false;
        OnDeselected?.Invoke(this);
    }
} 
