using UnityEngine;

public class HumanShipController : ShipController{
    public float GetVerticalInput() {
        return Input.GetAxis("Vertical");
    }

    public float GetHorizontalInput() {
        return Input.GetAxis("Horizontal");
    }

    public Vector3 GetAimPoint() {
        return GameCamera.RayCastGunTargetingHit.point;
    }

    public bool GetFireInput() {
        return Input.GetMouseButton(0);
    }
}
