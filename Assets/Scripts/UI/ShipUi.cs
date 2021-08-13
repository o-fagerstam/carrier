using System;
using System.Collections.Generic;
using System.Linq;
using ServiceLocator;
using Ship;
using UI;
using UnityEngine;

public class ShipUi : MonoBehaviourService {
    private readonly Dictionary<ShipGun, GunTargetMarker> _gunIdToMarkerDict = new Dictionary<ShipGun, GunTargetMarker>();
    [SerializeField] private GameObject crosshair;
    [SerializeField] private GunTargetMarker gunMarkerPrefab;

    private PlayerShipController _playerShipController;
    
    private IShipTrackingUiComponent[] _shipTrackingComponents;

    protected override void Awake() {
        base.Awake();
        _shipTrackingComponents = GetComponentsInChildren<IShipTrackingUiComponent>();
    }

    private void Start() {
        _playerShipController = MonoBehaviourServiceLocator.Current.Get<PlayerShipController>();
        _playerShipController.OnAcquireCamera += AcquireShip;
        _playerShipController.OnReleaseCamera += ReleaseCurrentShip;
    }

    private void AddMarker(ShipGun gun) {
        if (_gunIdToMarkerDict.ContainsKey(gun)) {
            throw new ArgumentException($"Gun with id {gun.GetInstanceID()} already has marker", nameof(gun));
        }

        _gunIdToMarkerDict[gun] = Instantiate(gunMarkerPrefab, Vector3.zero, Quaternion.identity);
        _gunIdToMarkerDict[gun].trackedGun = gun;
    }

    public void ReleaseCurrentShip() {
        foreach (ShipGun oldGun in _gunIdToMarkerDict.Keys.ToList()) {
            RemoveMarker(oldGun);
        }
        crosshair.SetActive(false);
    }

    public void AcquireShip(ShipMain ship) {
        ReleaseCurrentShip();

        foreach (ShipGun newGun in ship.MainGuns) {
            AddMarker(newGun);
        }

        foreach (IShipTrackingUiComponent component in _shipTrackingComponents) {
            component.AcquireShip(ship);
        }

        crosshair.SetActive(true);
    }

    private void RemoveMarker(ShipGun gun) {
        Destroy(_gunIdToMarkerDict[gun].gameObject);
        _gunIdToMarkerDict.Remove(gun);
    }

    private void OnDestroy() {
        _playerShipController.OnAcquireCamera -= AcquireShip;
        _playerShipController.OnReleaseCamera -= ReleaseCurrentShip;
    }
}