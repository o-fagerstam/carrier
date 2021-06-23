using System.Collections.Generic;
using Ship;
using UnityEngine;

public class VehiclesManager : MonoBehaviour {
    public HashSet<ShipMain> AllShips { get; private set; }
    public Dictionary<int, HashSet<ShipMain>> ShipsByTeam { get; private set; }

    public static VehiclesManager Instance { get; private set; }

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
        }
        else {
            Instance = this;
        }

        AllShips = new HashSet<ShipMain>();
        ShipsByTeam = new Dictionary<int, HashSet<ShipMain>>();
        foreach (ShipMain ship in FindObjectsOfType<ShipMain>()) {
            AddShip(ship);
        }
    }

    private void AddShip(ShipMain ship) {
        AllShips.Add(ship);
        if (!ShipsByTeam.ContainsKey(ship.team)) {
            ShipsByTeam[ship.team] = new HashSet<ShipMain>();
        }

        ShipsByTeam[ship.team].Add(ship);

        ship.OnShipDestruction += RemoveShip;
    }

    public void RemoveShip(ShipMain ship) {
        AllShips.Remove(ship);
        ShipsByTeam[ship.team].Remove(ship);
    }
}