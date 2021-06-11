using UnityEngine;

public class GameCamera : MonoBehaviour {
    public static Ray MouseRay;
    public static RaycastHit RaycastHit;
    public static bool RayCastMadeHit;
    public static Camera currentCamera { get; private set; }
    private static float xRotation;
    [SerializeField] private LayerMask layerMask;
    public float MouseSensitivity = 10f;

    public Transform objectToFollow;

    public Transform swivel, stick, cameraTransform;

    public static float YRotation { get; private set; }

    private void Start() {
        Debug.Log(layerMask.value);
        currentCamera = Camera.main;

        swivel = transform.GetChild(0);
        stick = swivel.GetChild(0);
        cameraTransform = stick.GetChild(0);
        xRotation = swivel.rotation.eulerAngles.x;
        YRotation = swivel.rotation.eulerAngles.y;

        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Awake() {
        currentCamera = Camera.main;
    }

    private void Update() {
        MouseRay = currentCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f));
        RayCastMadeHit = Physics.Raycast(MouseRay, out RaycastHit, 10000f, layerMask);

        var mouseX = Input.GetAxis("Mouse X") * MouseSensitivity * Time.deltaTime;
        var mouseY = Input.GetAxis("Mouse Y") * MouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, 10f, 100f);
        var xSwivel = xRotation * 0.9f;
        var xCameraTransform = xRotation * -0.5f - 5f;

        YRotation += mouseX;
        YRotation = Mathf.Repeat(YRotation, 360f);

        swivel.localRotation = Quaternion.Euler(xSwivel, YRotation, 0f);
        cameraTransform.localRotation = Quaternion.Euler(xCameraTransform, 0f, 0f);

        swivel.position = objectToFollow.position;
    }

    private void OnDrawGizmos() {
        if (RayCastMadeHit) {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(RaycastHit.point, 1);
        }
    }
}