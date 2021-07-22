using UnityEngine;

namespace UI {
    public class ScreenProjectedTrackingObject : ScreenProjectedObject {
        public Transform trackedTransform;
        public Vector3 offset = Vector3.zero;
        public Vector3 onScreenOffset = Vector3.zero;

        private void Update() {
            Camera camera = PlayerCamera.Instance.Camera;
            Vector3 trackedPosition = trackedTransform.position;
            Vector3 worldScreenPoint = camera.WorldToScreenPoint(trackedPosition + offset) + onScreenOffset;
            float scaleFactor = 50f / Mathf.Sqrt(worldScreenPoint.z) * (Mathf.Cos(camera.fieldOfView * Mathf.Deg2Rad) / Mathf.Cos(60f * Mathf.Deg2Rad));
            
            Vector3 flatScreenPoint = new Vector3(worldScreenPoint.x, worldScreenPoint.y, 0f);
            transform.position = flatScreenPoint;
            bool visibleAngle = CheckVisibleScreenPosition(flatScreenPoint);
            Visible = visibleAngle;
            SetScale(scaleFactor);
        }
    }
}