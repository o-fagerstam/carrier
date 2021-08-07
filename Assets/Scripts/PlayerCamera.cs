using System;
using ServiceLocator;
using UnityEngine;

public class PlayerCamera : MonoBehaviourService {
    public Vector3 Position => transform.position;
    
    public Camera Camera { get; private set; }

    public const float StandardFov = 60f;


    protected override void Awake() {
        base.Awake();
        Camera = GetComponent<Camera>();
    }

    public void FollowTransform(Transform t) {
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
