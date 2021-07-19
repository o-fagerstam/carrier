using Ship;
using UnityEngine;
using UnityEngine.UI;

public class DistanceText : MonoBehaviour {
    private Text _text;

    private void Start() {
        _text = GetComponent<Text>();
    }

    private void Update() {
        if (PlayerShipController.RayCastMadeGunTargetingHit) {
            var distance = (PlayerShipController.RayCastGunTargetingHit.point - PlayerShipController.Instance.cameraHolderTransform.position)
                .magnitude;
            _text.text = distance.ToString();
        }
        else {
            _text.text = "N/A";
        }
    }
}