using System;
using System.Collections.Generic;
using Ship;
using UnityEngine;

public class VehiclesManager : MonoBehaviour {
    
    public HashSet<GameUnit> AllUnits { get; private set; }
    public HashSet<ShipMain> AllShips { get; private set; }

    public static VehiclesManager Instance { get; private set; }
    public event Action<GameUnit> OnUnitAdded;
    public event Action<GameUnit> OnUnitRemoved;

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
        }
        else {
            Instance = this;
        }
        
        AllUnits = new HashSet<GameUnit>();
        AllShips = new HashSet<ShipMain>();
        foreach (ShipMain ship in FindObjectsOfType<ShipMain>()) {
            AddShip(ship);
        }
    }

    private void AddShip(ShipMain ship) {
        AllUnits.Add(ship);
        AllShips.Add(ship);

        ship.OnDeath += RemoveShip;
        OnUnitAdded?.Invoke(ship);
    }

    private void RemoveShip(GameUnit shipUnit) {
        ShipMain ship = (ShipMain) shipUnit; // This cast is a code smell, but I don't know how else to get inheritance to work with Actions
        ship.OnDeath -= RemoveShip;
        AllUnits.Remove(ship);
        AllShips.Remove(ship);
        OnUnitRemoved?.Invoke(ship);
    }
}