using UnityEngine;
using UnityEngine.UI;

namespace UI {
    public class ScreenProjectedImage : ScreenProjectedObject {
        [SerializeField] private Image image;

        public override void SetVisible(bool visible) {
            base.SetVisible(visible);
            image.enabled = visible;
        }

        public void SetColor(Color color) {
            image.color = color;
        }
    }
}