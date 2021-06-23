using System.Collections.Generic;
using Ship;
using UnityEngine;

public class VehiclesManager : MonoBehaviour {
    private static VehiclesManager _instance;
    public HashSet<Ship.ShipMain> AllShips { get; private set; }
    public Dictionary<int, HashSet<Ship.ShipMain>> ShipsByTeam { get; private set; }

    public static VehiclesManager Instance => _instance;

    private void Awake() {
        if (_instance != null && _instance != this) {
            Destroy(gameObject);
        }
        else {
            _instance = this;
        }
        
        AllShips = new HashSet<Ship.ShipMain>();
        ShipsByTeam = new Dictionary<int, HashSet<Ship.ShipMain>>();
        foreach (Ship.ShipMain ship in FindObjectsOfType<Ship.ShipMain>()) {
            AddShip(ship);
        }
    }

    public void AddShip(Ship.ShipMain ship) {
        AllShips.Add(ship);
        if (!ShipsByTeam.ContainsKey(ship.team)) {
            ShipsByTeam[ship.team] = new HashSet<Ship.ShipMain>();
        }

        ShipsByTeam[ship.team].Add(ship);
    }

    public void RemoveShip(Ship.ShipMain ship) {
        AllShips.Remove(ship);
        ShipsByTeam[ship.team].Remove(ship);
    }
}