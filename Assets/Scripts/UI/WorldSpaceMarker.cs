using ServiceLocator;
using UnityEngine;

namespace UI {
    public abstract class WorldSpaceMarker : MonoBehaviour {
        public Vector3 onScreenOffset = Vector3.zero;
        
        protected abstract Vector3 TrackedPosition { get; }

        private RectTransform _canvasRectTransform;
        private PlayerCamera _playerCamera;

        private float _layerDepth;
        private const float ON_SCREEN_SCALE_FACTOR = 0.015f;

        private void Awake() {
            _layerDepth = 3f + Random.value;
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
            Vector3 directionFromCamera = (trackedPos - cameraPos).normalized;
            Vector3 newPos = cameraPos + directionFromCamera * _layerDepth;
            _canvasRectTransform.position = newPos;
            _canvasRectTransform.LookAt(cameraPos, Vector3.up);

            float cameraFovFactor = _playerCamera.Camera.fieldOfView / 60f;
            _canvasRectTransform.localScale = Vector3.one * (_layerDepth * ON_SCREEN_SCALE_FACTOR * cameraFovFactor);
        }
        
        protected abstract void SetVisible(bool visible);
        public abstract void SetColor(Color c);
    }
}