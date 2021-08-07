using System;
using UnityEngine;
using UnityEngine.UI;

namespace UI {
    public class ScreenProjectedObject : MonoBehaviour {
        private Image _image;
        private RectTransform _rectTransform;

        public bool Visible {
            get => _image.enabled;
            set => _image.enabled = value;
        }

        private void Awake() {
            _rectTransform = GetComponent<RectTransform>();
            _image = GetComponent<Image>();
            Visible = true;
        }

        public void SetColor(Color color) {
            _image.color = color;
        }

        public void SetScale(float scale) {
            _rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, scale * 12f);
            _rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, scale * 12f);
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