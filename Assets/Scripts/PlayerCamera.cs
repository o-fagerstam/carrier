using System;
using ServiceLocator;
using Ship;
using UnityEngine;

public class PlayerCamera : MonoBehaviourService {
    public Vector3 Position => transform.position;
    
    public Camera Camera { get; private set; }

    public const float StandardFov = 60f;
    private IPlayerCameraAcquirable _playerCameraAcquirable;


    protected override void Awake() {
        base.Awake();
        Camera = GetComponent<Camera>();
    }

    public void FollowTransform(Transform t) {
        transform.parent = t;
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }

    public void SwitchController(IPlayerCameraAcquirable target) {
        Release();
        
        _playerCameraAcquirable = target;
        _playerCameraAcquirable.AcquireCamera();
        
    }

    public void Release() {
        _playerCameraAcquirable?.ReleaseCamera();
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
            default:
                throw new NotImplementedException("PlayerCamera does not support this Camera Mode");
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
