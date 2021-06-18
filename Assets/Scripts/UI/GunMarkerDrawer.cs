using System;
using System.Collections.Generic;
using UI;
using UnityEngine;

public class GunMarkerDrawer : MonoBehaviour {
    private static readonly Color ReadyColor = new Color(25f / 255f, 191f / 255f, 70 / 255f);
    private static readonly Color LoadingColor = new Color(219f / 255f, 143f / 255f, 29f / 255f);
    private readonly Dictionary<ShipGun, GunMarker> _gunIdToMarkerDict = new Dictionary<ShipGun, GunMarker>();
    private RectTransform _canvasRectTransform;
    [SerializeField] private GunMarker gunMarkerPrefab;
    [SerializeField] private List<ShipGun> startingGuns = new List<ShipGun>();
    private Vector2 uiOffset;

    private void Start() {
        _canvasRectTransform = GetComponent<RectTransform>();
        Vector2 sizeDelta = _canvasRectTransform.sizeDelta;
        uiOffset = new Vector2(sizeDelta.x * 0.5f, sizeDelta.y * 0.5f);
        foreach (ShipGun gun in startingGuns) {
            AddMarker(gun);
        }
    }

    private void Update() {
        UpdateMarkers();
    }

    private void UpdateMarkers() {
        foreach (ShipGun gun in _gunIdToMarkerDict.Keys) {
            GunMarker marker = _gunIdToMarkerDict[gun];
            UpdateMarker(marker, gun);
        }
    }


    private void UpdateMarker(GunMarker marker, ShipGun gun) {
        Vector3 gunImpactPoint = gun.CurrentImpactPoint;
        Camera currentCamera = GameCamera.CurrentCamera;
        var angleIsValid = CheckValidAngle(currentCamera, gunImpactPoint);
        if (angleIsValid) {
            marker.IsHidden = false;
            MoveMarkerToWorldPoint(currentCamera, marker, gunImpactPoint);
            SetMarkerColor(marker, gun.IsLoaded);
        }
        else {
            marker.IsHidden = true;
        }
    }

    private static bool CheckValidAngle(Camera currentCamera, Vector3 gunImpactPoint) {
        Vector3 cameraPosition = currentCamera.WorldToViewportPoint(gunImpactPoint);
        return cameraPosition.x >= 0 && cameraPosition.x <= 1 &&
               cameraPosition.y >= 0 && cameraPosition.y <= 1 &&
               cameraPosition.z > 0;
    }

    public void AddMarker(ShipGun gun) {
        if (_gunIdToMarkerDict.ContainsKey(gun)) {
            throw new ArgumentException($"Gun with id {gun.GetInstanceID()} already has marker", nameof(gun));
        }

        _gunIdToMarkerDict[gun] = Instantiate(gunMarkerPrefab, Vector3.zero, Quaternion.identity, transform);
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

    public void RemoveMarker(ShipGun gun) {
        Destroy(_gunIdToMarkerDict[gun].gameObject);
        _gunIdToMarkerDict.Remove(gun);
    }
}