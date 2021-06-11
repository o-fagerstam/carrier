using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GunMarkerDrawer : MonoBehaviour {
    private RectTransform canvasRectTransform;
    private readonly Dictionary<BoatGun, RectTransform> gunIdToMarkerDict = new Dictionary<BoatGun, RectTransform>();
    [SerializeField] private RectTransform gunMarkerPrefab;
    [SerializeField] private List<BoatGun> startingGuns = new List<BoatGun>();
    private Vector2 uiOffset;

    private void Start() {
        canvasRectTransform = GetComponent<RectTransform>();
        var sizeDelta = canvasRectTransform.sizeDelta;
        uiOffset = new Vector2(sizeDelta.x * 0.5f, sizeDelta.y * 0.5f);
        foreach (var gun in startingGuns) {
            AddMarker(gun);
        }
    }

    private void Update() {
        foreach (var gun in gunIdToMarkerDict.Keys) {
            MoveMarkerToWorldPoint(gunIdToMarkerDict[gun], gun.CurrentAimPoint);
        }
    }

    public void AddMarker(BoatGun gun) {
        if (gunIdToMarkerDict.ContainsKey(gun)) {
            throw new ArgumentException($"Gun with id {gun.GetInstanceID()} already has marker", nameof(gun));
        }

        gunIdToMarkerDict[gun] = Instantiate(gunMarkerPrefab, Vector3.zero, Quaternion.identity, transform);
    }
    
     /*

     public void EnableMarker(int gunId) {
         gunIdToMarkerDict[gunId].GetComponent<Image>().enabled = true;
     }

     public void DisableMarker(int gunId) {
         gunIdToMarkerDict[gunId].GetComponent<Image>().enabled = false;
     }*/

     private void MoveMarkerToWorldPoint(RectTransform marker, Vector3 worldPosition) {
         Vector2 viewPortPosition = GameCamera.currentCamera.WorldToViewportPoint(worldPosition);
         var sizeDelta = canvasRectTransform.sizeDelta;
         var proportionalPosition = new Vector2(
             viewPortPosition.x * sizeDelta.x,
             viewPortPosition.y * sizeDelta.y
         );
         marker.localPosition = proportionalPosition - uiOffset;
     }
     /*

     public void MoveMarkerToWorldPoint(int gunId, Vector3 worldPosition) {
         MoveMarkerToWorldPoint(gunIdToMarkerDict[gunId], worldPosition);
     }

     public void RemoveMarker(int gunId) {
         Destroy(gunIdToMarkerDict[gunId].gameObject);
         gunIdToMarkerDict.Remove(gunId);
     }*/
}