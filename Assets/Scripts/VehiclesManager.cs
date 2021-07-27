using System;
using System.Collections.Generic;
using Ship;
using UnityEngine;

public class VehiclesManager : MonoBehaviour {
    
    private HashSet<GameUnit> _allUnits = new HashSet<GameUnit>();
    public static IReadOnlyCollection<GameUnit> AllUnits => _instance._allUnits;
    private HashSet<ShipMain> _allShips = new HashSet<ShipMain>();
    public static IReadOnlyCollection<ShipMain> AllShips => _instance._allShips;

    private static VehiclesManager _instance;
    public static VehiclesManager Instance => _instance;
    public event Action<GameUnit> OnUnitAdded;
    public event Action<GameUnit> OnUnitRemoved;

    private void Awake() {
        if (_instance != null && _instance != this) {
            Destroy(gameObject);
        }
        else {
            _instance = this;
        }
        
        foreach (ShipMain ship in FindObjectsOfType<ShipMain>()) {
            AddShip(ship);
        }
    }

    private void AddShip(ShipMain ship) {
        _allUnits.Add(ship);
        _allShips.Add(ship);

        ship.OnDeath += RemoveShip;
        OnUnitAdded?.Invoke(ship);
    }

    private void RemoveShip(GameUnit shipUnit) {
        ShipMain ship = (ShipMain) shipUnit; // This cast is a code smell, but I don't know how else to get inheritance to work with Actions
        ship.OnDeath -= RemoveShip;
        _allUnits.Remove(ship);
        _allShips.Remove(ship);
        OnUnitRemoved?.Invoke(ship);
    }
}