using System;
using System.Collections.Generic;
using System.Linq;
using ServiceLocator;
using Ship;
using UI;
using UnityEngine;

public class ShipUi : MonoBehaviourService {
    private static readonly Color ReadyColor = new Color(25f / 255f, 191f / 255f, 70 / 255f);
    private static readonly Color LoadingColor = new Color(219f / 255f, 143f / 255f, 29f / 255f);
    private readonly Dictionary<ShipGun, ScreenProjectedObject> _gunIdToMarkerDict = new Dictionary<ShipGun, ScreenProjectedObject>();
    private RectTransform _canvasRectTransform;
    [SerializeField] private GameObject crosshair;
    [SerializeField] private ScreenProjectedObject gunMarkerPrefab;
    private Vector2 _uiOffset;

    private PlayerCamera _playerCamera;
    private PlayerShipController _playerShipController;
    
    private IShipTrackingUiComponent[] _shipTrackingComponents;

    protected override void Awake() {
        base.Awake();
        _canvasRectTransform = transform.parent.GetComponent<RectTransform>();
        _shipTrackingComponents = GetComponentsInChildren<IShipTrackingUiComponent>();
        Vector2 sizeDelta = _canvasRectTransform.sizeDelta;
        _uiOffset = new Vector2(sizeDelta.x * 0.5f, sizeDelta.y * 0.5f);
    }

    private void Start() {
        _playerShipController = MonoBehaviourServiceLocator.Current.Get<PlayerShipController>();
        _playerShipController.OnAcquireCamera += AcquireShip;
        _playerShipController.OnReleaseCamera += ReleaseCurrentShip;

        _playerCamera = MonoBehaviourServiceLocator.Current.Get<PlayerCamera>();
    }

    public void RefreshMarkers() {
        foreach (KeyValuePair<ShipGun, ScreenProjectedObject> pair in _gunIdToMarkerDict) {
            RefreshMarker(pair.Value, pair.Key);
        }
    }


    private void RefreshMarker(ScreenProjectedObject marker, ShipGun gun) {
        GunImpactPrediction prediction = gun.GunImpactPrediction;
        if (!prediction.willImpact) {
            marker.Visible = false;
            return;
        }
        Camera camera = _playerCamera.Camera;
        bool angleIsValid = marker.CheckVisibleWorldPosition(camera, prediction.impactPosition);
        if (!angleIsValid) {
            marker.Visible = false;
            return;
        }
        marker.Visible = true;
        MoveMarkerToWorldPoint(camera, marker, prediction.impactPosition);
        SetMarkerColor(marker, gun.IsLoaded);
    }



    private void AddMarker(ShipGun gun) {
        if (_gunIdToMarkerDict.ContainsKey(gun)) {
            throw new ArgumentException($"Gun with id {gun.GetInstanceID()} already has marker", nameof(gun));
        }

        _gunIdToMarkerDict[gun] = Instantiate(gunMarkerPrefab, Vector3.zero, Quaternion.identity, transform);
    }

    public void ReleaseCurrentShip() {
        foreach (ShipGun oldGun in _gunIdToMarkerDict.Keys.ToList()) {
            RemoveMarker(oldGun);
        }
        crosshair.SetActive(false);
    }

    public void AcquireShip(ShipMain ship) {
        ReleaseCurrentShip();

        Debug.Log($"Aqcuiring ship {ship}");
        
        foreach (ShipGun newGun in ship.MainGuns) {
            AddMarker(newGun);
        }

        foreach (IShipTrackingUiComponent component in _shipTrackingComponents) {
            component.AcquireShip(ship);
        }

        crosshair.SetActive(true);
    }

    private void MoveMarkerToWorldPoint(Camera currentCamera, ScreenProjectedObject marker, Vector3 worldPosition) {
        Vector2 viewPortPosition = currentCamera.WorldToViewportPoint(worldPosition);
        Vector2 canvasSizeDelta = _canvasRectTransform.sizeDelta;
        var proportionalPosition = new Vector2(
            viewPortPosition.x * canvasSizeDelta.x,
            viewPortPosition.y * canvasSizeDelta.y
        );
        var distanceToCamera = (worldPosition - currentCamera.transform.position).magnitude;
        var markerScale = 1 - Mathf.Sqrt(distanceToCamera / 5000) / 2;
        marker.SetScale(markerScale);
        marker.SetLocalPosition(proportionalPosition - _uiOffset);
    }

    private static void SetMarkerColor(ScreenProjectedObject marker, bool isLoaded) {
        if (isLoaded) {
            marker.SetColor(ReadyColor);
        }
        else {
            marker.SetColor(LoadingColor);
        }
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