using UnityEngine;
using UnityEngine.UI;

namespace UI {
    public class GunMarker : MonoBehaviour {
        private Image _image;
        private RectTransform _rectTransform;

        public bool IsHidden {
            get => _image.enabled;
            set => _image.enabled = !value;
        }

        private void Awake() {
            _rectTransform = GetComponent<RectTransform>();
            _image = GetComponent<Image>();
            IsHidden = false;
        }

        public void SetColor(Color color) {
            _image.color = color;
        }

        public void SetScale(float scale) {
            _rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, scale * 12f);
        }

        public void SetLocalPosition(Vector2 position) {
            _rectTransform.localPosition = position;
        }
    }
}