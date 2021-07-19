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

        ship.OnDeath += RemoveShip;
    }

    private void RemoveShip(GameUnit shipUnit) {
        ShipMain ship = (ShipMain) shipUnit; // This cast is a code smell, but I don't know how else to get inheritance to work with Actions
        ship.OnDeath -= RemoveShip;
        AllShips.Remove(ship);
        ShipsByTeam[ship.team].Remove(ship);
    }
}