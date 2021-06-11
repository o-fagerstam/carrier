using UnityEngine;

public class BoatMovement : MonoBehaviour {
    [SerializeField] private float enginePower = 50000f;
    [SerializeField] private Transform hull;
    [SerializeField] private Rigidbody hullRigidbody;
    [SerializeField] private float rudderPower = 10000f;

    private void FixedUpdate() {
        var verticalInput = Input.GetAxis("Vertical");
        var horizontalInput = Input.GetAxis("Horizontal");

        var forwardForce = new Vector3(0, 0, verticalInput * Time.deltaTime * enginePower);
        hullRigidbody.AddRelativeForce(forwardForce, ForceMode.Force);

        var rudderForce = new Vector3(0, horizontalInput * Time.deltaTime * rudderPower);
        hullRigidbody.AddRelativeTorque(rudderForce, ForceMode.Force);

        var brakingForce = -10f * Time.deltaTime * hullRigidbody.velocity;
        hullRigidbody.AddForce(brakingForce);

        var rotationBrakingForce = -3f * Time.deltaTime * hullRigidbody.angularVelocity;
        hullRigidbody.AddTorque(rotationBrakingForce);
    }
}