using System;
using UnityEngine;
using UnityEngine.UI;

namespace UI {
    public abstract class ScreenProjectedObject : MonoBehaviour {
        private RectTransform _rectTransform;

        public bool Visible { get; private set; }


        protected virtual void Awake() {
            _rectTransform = GetComponent<RectTransform>();
        }

        public virtual void SetVisible(bool visible) {
            Visible = visible;
        }
        
        public void SetScale(float scale) {
            _rectTransform.localScale = Vector3.one * scale;
        }

        public void SetLocalPosition(Vector2 position) {
            _rectTransform.localPosition = position;
        }
        
        public bool CheckVisibleWorldPosition(Camera currentCamera, Vector3 point) {
            Vector3 cameraPosition = currentCamera.WorldToViewportPoint(point);
            return cameraPosition.x >= 0 && cameraPosition.x <= 1 &&
                   cameraPosition.y >= 0 && cameraPosition.y <= 1 &&
                   cameraPosition.z > 0;
        }

        public bool CheckVisibleScreenPosition(Vector3 point) {
            Rect screenRect = new Rect(0, 0, Screen.width, Screen.height);
            return screenRect.Contains(point);
        }
    }
}