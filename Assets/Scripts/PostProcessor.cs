using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PostProcessor : MonoBehaviour {
    private static PostProcessor _instance;
    public static PostProcessor Instance => _instance;
    private Volume _volume;
    private Vignette _vignette;

    private void Awake() {
        if (_instance != null && _instance != this) {
            Destroy(gameObject);
        }
        else {
            _instance = this;
        }

        _volume = GetComponent<Volume>();
        _vignette = (Vignette) _volume.profile.components[0];
    }

    public void EnableVignette() {
        _vignette.active = true;
    }

    public void DisableVignette() {
        _vignette.active = false;
    }

    public void SetVignetteIntensity(float intensity) {
        _vignette.intensity.value = intensity;
    }
}
