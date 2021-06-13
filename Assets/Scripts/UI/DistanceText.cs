using UnityEngine;
using UnityEngine.UI;

public class DistanceText : MonoBehaviour {
    [SerializeField] private Transform _gameCamera;
    private Text _text;

    private void Start() {
        _text = GetComponent<Text>();
    }

    private void Update() {
        if (GameCamera.RayCastMadeGunTargetingHit) {
            var distance = (GameCamera.RayCastGunTargetingHit.point - _gameCamera.position).magnitude;
            _text.text = distance.ToString();
        }
        else {
            _text.text = "N/A";
        }
    }
}