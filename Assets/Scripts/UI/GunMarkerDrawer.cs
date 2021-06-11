using System;
using System.Collections.Generic;
using UI;
using UnityEngine;

public class GunMarkerDrawer : MonoBehaviour {
    private readonly Dictionary<BoatGun, GunMarker> _gunIdToMarkerDict = new Dictionary<BoatGun, GunMarker>();
    private RectTransform _canvasRectTransform;
    [SerializeField] private GunMarker gunMarkerPrefab;
    [SerializeField] private List<BoatGun> startingGuns = new List<BoatGun>();
    private Vector2 uiOffset;

    private void Start() {
        _canvasRectTransform = GetComponent<RectTransform>();
        var sizeDelta = _canvasRectTransform.sizeDelta;
        uiOffset = new Vector2(sizeDelta.x * 0.5f, sizeDelta.y * 0.5f);
        foreach (var gun in startingGuns) {
            AddMarker(gun);
        }
    }

    private void Update() {
        foreach (var gun in _gunIdToMarkerDict.Keys) {
            MoveMarkerToWorldPoint(_gunIdToMarkerDict[gun], gun.CurrentAimPoint);
        }
    }

    public void AddMarker(BoatGun gun) {
        if (_gunIdToMarkerDict.ContainsKey(gun)) {
            throw new ArgumentException($"Gun with id {gun.GetInstanceID()} already has marker", nameof(gun));
        }

        _gunIdToMarkerDict[gun] = Instantiate(gunMarkerPrefab, Vector3.zero, Quaternion.identity, transform);
    }

    private void MoveMarkerToWorldPoint(GunMarker marker, Vector3 worldPosition) {
        Vector2 viewPortPosition = GameCamera.CurrentCamera.WorldToViewportPoint(worldPosition);
        var canvasSizeDelta = _canvasRectTransform.sizeDelta;
        var proportionalPosition = new Vector2(
            viewPortPosition.x * canvasSizeDelta.x,
            viewPortPosition.y * canvasSizeDelta.y
        );
        var distanceToCamera = (worldPosition - GameCamera.CurrentCamera.transform.position).magnitude;
        var markerScale = 1 / Mathf.Sqrt(distanceToCamera) * 100f;
        marker.SetScale(markerScale);
        marker.SetLocalPosition(proportionalPosition - uiOffset);
    }

    public void RemoveMarker(BoatGun gun) {
        Destroy(_gunIdToMarkerDict[gun].gameObject);
        _gunIdToMarkerDict.Remove(gun);
    }
}