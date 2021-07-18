using System;
using System.Collections.Generic;
using Ship;
using UI;
using UnityEngine;

public class ShipUI : MonoBehaviour {
    private static readonly Color ReadyColor = new Color(25f / 255f, 191f / 255f, 70 / 255f);
    private static readonly Color LoadingColor = new Color(219f / 255f, 143f / 255f, 29f / 255f);
    private readonly Dictionary<ShipGun, GunMarker> _gunIdToMarkerDict = new Dictionary<ShipGun, GunMarker>();
    private RectTransform _canvasRectTransform;
    [SerializeField] private GunMarker gunMarkerPrefab;
    private Vector2 uiOffset;

    private static ShipUI _instance;
    public static ShipUI Instance => _instance;
    private IShipTrackingUiComponent[] _shipTrackingComponents;

    private void Awake() {
        if (_instance != null && _instance != this) {
            Destroy(gameObject);
        }
        else {
            _instance = this;
        }
        _canvasRectTransform = GetComponent<RectTransform>();
        _shipTrackingComponents = GetComponentsInChildren<IShipTrackingUiComponent>();
        Vector2 sizeDelta = _canvasRectTransform.sizeDelta;
        uiOffset = new Vector2(sizeDelta.x * 0.5f, sizeDelta.y * 0.5f);
    }

    public void RefreshMarkers() {
        foreach (ShipGun gun in _gunIdToMarkerDict.Keys) {
            GunMarker marker = _gunIdToMarkerDict[gun];
            RefreshMarker(marker, gun);
        }
    }


    private void RefreshMarker(GunMarker marker, ShipGun gun) {
        GunImpactPrediction prediction = gun.GunImpactPrediction;
        if (!prediction.willImpact) {
            marker.IsHidden = true;
            return;
        }
        Camera currentCamera = PlayerCamera.Instance.Camera;
        var angleIsValid = CheckValidAngle(currentCamera, prediction.impactPosition);
        if (!angleIsValid) {
            marker.IsHidden = true;
            return;
        }
        marker.IsHidden = false;
        MoveMarkerToWorldPoint(currentCamera, marker, prediction.impactPosition);
        SetMarkerColor(marker, gun.IsLoaded);
    }

    private static bool CheckValidAngle(Camera currentCamera, Vector3 gunImpactPoint) {
        Vector3 cameraPosition = currentCamera.WorldToViewportPoint(gunImpactPoint);
        return cameraPosition.x >= 0 && cameraPosition.x <= 1 &&
               cameraPosition.y >= 0 && cameraPosition.y <= 1 &&
               cameraPosition.z > 0;
    }

    private void AddMarker(ShipGun gun) {
        if (_gunIdToMarkerDict.ContainsKey(gun)) {
            throw new ArgumentException($"Gun with id {gun.GetInstanceID()} already has marker", nameof(gun));
        }

        _gunIdToMarkerDict[gun] = Instantiate(gunMarkerPrefab, Vector3.zero, Quaternion.identity, transform);
    }

    public void AcquireShip(ShipMain ship) {
        foreach (ShipGun oldGun in _gunIdToMarkerDict.Keys) {
            RemoveMarker(oldGun);
        }

        foreach (ShipGun newGun in ship.MainGuns) {
            AddMarker(newGun);
        }

        foreach (IShipTrackingUiComponent component in _shipTrackingComponents) {
            component.AcquireShip(ship);
        }
    }

    private void MoveMarkerToWorldPoint(Camera currentCamera, GunMarker marker, Vector3 worldPosition) {
        Vector2 viewPortPosition = currentCamera.WorldToViewportPoint(worldPosition);
        Vector2 canvasSizeDelta = _canvasRectTransform.sizeDelta;
        var proportionalPosition = new Vector2(
            viewPortPosition.x * canvasSizeDelta.x,
            viewPortPosition.y * canvasSizeDelta.y
        );
        var distanceToCamera = (worldPosition - currentCamera.transform.position).magnitude;
        var markerScale = 1 - Mathf.Sqrt(distanceToCamera / 5000) / 2;
        marker.SetScale(markerScale);
        marker.SetLocalPosition(proportionalPosition - uiOffset);
    }

    private static void SetMarkerColor(GunMarker marker, bool isLoaded) {
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
}