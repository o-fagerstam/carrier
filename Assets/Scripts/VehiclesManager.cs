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
            AddUnit(ship);
        }
    }

    private void AddUnit(GameUnit unit) {
        _allUnits.Add(unit);

        if (unit is ShipMain) {
            _allShips.Add((ShipMain) unit);
        }
        unit.OnDeath += RemoveUnit;
        OnUnitAdded?.Invoke(unit);
    }

    private void RemoveUnit(GameUnit unit) {
        unit.OnDeath -= RemoveUnit;
        _allUnits.Remove(unit);
        
        if (unit is ShipMain) {
            _allShips.Remove((ShipMain) unit);
        }
        
        OnUnitRemoved?.Invoke(unit);
    }
}