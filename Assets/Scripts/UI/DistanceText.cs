using Ship;
using UnityEngine;
using UnityEngine.UI;

public class DistanceText : MonoBehaviour {
    private Text _text;

    private void Start() {
        _text = GetComponent<Text>();
    }

    private void Update() {
        if (ShipCamera.RayCastMadeGunTargetingHit) {
            var distance = (ShipCamera.RayCastGunTargetingHit.point - ShipCamera.CurrentCamera.transform.position)
                .magnitude;
            _text.text = distance.ToString();
        }
        else {
            _text.text = "N/A";
        }
    }
}