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
    
    private static readonly Color ReadyColor = new Color(25f/255f, 191f/255f, 70/255f);
    private static readonly Color LoadingColor = new Color(219f/255f, 143f/255f, 29f/255f);
    private void Start() {
        _canvasRectTransform = GetComponent<RectTransform>();
        Vector2 sizeDelta = _canvasRectTransform.sizeDelta;
        uiOffset = new Vector2(sizeDelta.x * 0.5f, sizeDelta.y * 0.5f);
        foreach (BoatGun gun in startingGuns) {
            AddMarker(gun);
        }
    }

    private void Update() {
        foreach (BoatGun gun in _gunIdToMarkerDict.Keys) {
            GunMarker marker = _gunIdToMarkerDict[gun];
            MoveMarkerToWorldPoint(marker, gun.CurrentImpactPoint);
            SetMarkerColor(marker, gun.GunState);
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
        Vector2 canvasSizeDelta = _canvasRectTransform.sizeDelta;
        var proportionalPosition = new Vector2(
            viewPortPosition.x * canvasSizeDelta.x,
            viewPortPosition.y * canvasSizeDelta.y
        );
        var distanceToCamera = (worldPosition - GameCamera.CurrentCamera.transform.position).magnitude;
        var markerScale = 1 - Mathf.Sqrt(distanceToCamera / 5000) / 2;
        marker.SetScale(markerScale);
        marker.SetLocalPosition(proportionalPosition - uiOffset);
    }

    private static void SetMarkerColor(GunMarker marker, GunState state) {
        switch (state) {
            case GunState.Ready: {
                marker.SetColor(ReadyColor);
                break;
            }
            case GunState.Loading: {
                marker.SetColor(LoadingColor);
                break;
            }
        }
    }

    public void RemoveMarker(BoatGun gun) {
        Destroy(_gunIdToMarkerDict[gun].gameObject);
        _gunIdToMarkerDict.Remove(gun);
    }
}