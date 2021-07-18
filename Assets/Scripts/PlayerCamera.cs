using System;
using UnityEngine;

public class PlayerCamera : MonoBehaviour {
    private static PlayerCamera _instance;
    public static PlayerCamera Instance => _instance;
    
    public Camera Camera { get; private set; }
    private Transform _transformToFollow;
    
    public const float StandardFov = 60f;


    private void Awake() {
        if (_instance != null && _instance != this) {
            Destroy(gameObject);
        }
        else {
            _instance = this;
        }
        Camera = GetComponent<Camera>();
    }

    public void FollowTransform(Transform t) {
        _transformToFollow = t;
        transform.parent = t;
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }

    public void Release() {
        transform.parent = null;
    }

    public void SetMode(CameraMode mode) {
        switch (mode) {
            case CameraMode.ShipNormal:
                PostProcessor.Instance.DisableVignette();
                ResetFov();
                break;
            case CameraMode.ShipScope:
                PostProcessor.Instance.EnableVignette();
                break;
            case CameraMode.Command:
                PostProcessor.Instance.DisableVignette();
                ResetFov();
                break;
        }
    }

    public void ResetFov() {
        Camera.fieldOfView = StandardFov;
    }

    public void SetFov(float fov) {
        Camera.fieldOfView = fov;
    }
    
    
    public enum CameraMode {
        ShipNormal, ShipScope, Command 
    }
}
