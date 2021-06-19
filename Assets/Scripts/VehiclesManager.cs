using System.Collections.Generic;
using Ship;
using UnityEngine;

public class VehiclesManager : MonoBehaviour {
    private static VehiclesManager _instance;
    public HashSet<Ship.Ship> AllShips { get; private set; }
    public Dictionary<int, HashSet<Ship.Ship>> ShipsByTeam { get; private set; }

    public static VehiclesManager Instance => _instance;

    private void Awake() {
        if (_instance != null && _instance != this) {
            Destroy(gameObject);
        }
        else {
            _instance = this;
        }
        
        AllShips = new HashSet<Ship.Ship>();
        ShipsByTeam = new Dictionary<int, HashSet<Ship.Ship>>();
        foreach (Ship.Ship ship in FindObjectsOfType<Ship.Ship>()) {
            AddShip(ship);
        }
    }

    public void AddShip(Ship.Ship ship) {
        AllShips.Add(ship);
        if (!ShipsByTeam.ContainsKey(ship.team)) {
            ShipsByTeam[ship.team] = new HashSet<Ship.Ship>();
        }

        ShipsByTeam[ship.team].Add(ship);
    }

    public void RemoveShip(Ship.Ship ship) {
        AllShips.Remove(ship);
        ShipsByTeam[ship.team].Remove(ship);
    }
}