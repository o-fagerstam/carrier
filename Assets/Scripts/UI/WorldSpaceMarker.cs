using ServiceLocator;
using UnityEngine;

namespace UI {
    public abstract class WorldSpaceMarker : MonoBehaviour {
        public Vector3 onScreenOffset = Vector3.zero;
        
        protected abstract Vector3 TrackedPosition { get; }
        /// <summary>
        /// The power by which the marker is scaled down at distance. Set to 1 or less to ignore distance scaling.
        /// </summary>
        protected abstract float DistanceScalingPower { get; }

        private RectTransform _canvasRectTransform;
        private PlayerCamera _playerCamera;

        private float _layeringLevel;
        private const float ON_SCREEN_SCALE_FACTOR = 0.015f;

        private void Awake() {
            _layeringLevel = Random.value;
            _canvasRectTransform = GetComponent<RectTransform>();
        }

        private void Start() {
            _playerCamera = MonoBehaviourServiceLocator.Current.Get<PlayerCamera>();
        }

        private bool IsVisibleInWorldSpace(Vector3 position) {
            Vector3 viewPortPosition = _playerCamera.Camera.WorldToViewportPoint(position);
            return viewPortPosition.x >= 0 && viewPortPosition.x <= 1 &&
                   viewPortPosition.y >= 0 && viewPortPosition.y <= 1 &&
                   viewPortPosition.z > 0;
        }

        protected virtual void LateUpdate() {
            Vector3 trackedPos = TrackedPosition;
            bool isVisible = IsVisibleInWorldSpace(trackedPos);
            SetVisible(isVisible);
            
            if (!isVisible) {
                return;
            }

            Vector3 cameraPos = _playerCamera.transform.position;
            Vector3 fromCameraVector = trackedPos - cameraPos;
            Vector3 directionFromCamera = (fromCameraVector).normalized;

            float depthOnScreen = 1f + Mathf.Pow(fromCameraVector.magnitude, 1f / 4f) + _layeringLevel * 0.0001f;
            Vector3 newPos = cameraPos + directionFromCamera * depthOnScreen;
            _canvasRectTransform.position = newPos;
            _canvasRectTransform.LookAt(cameraPos, Vector3.up);

            float cameraFovFactor = _playerCamera.Camera.fieldOfView / 60f;
            float scaleFactor = depthOnScreen * ON_SCREEN_SCALE_FACTOR * cameraFovFactor;
            if (DistanceScalingPower > 1f) {
                scaleFactor /= Mathf.Pow(fromCameraVector.magnitude * cameraFovFactor, 1f /DistanceScalingPower);
            }
            _canvasRectTransform.localScale = Vector3.one * scaleFactor;
        }

        
        protected abstract void SetVisible(bool visible);
        public abstract void SetColor(Color c);
    }
}