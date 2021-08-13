using Unit;
using UnityEngine;
using UnityEngine.UI;

namespace UI {
    public class ClassMarker : UnitTrackingWorldSpaceMarker {
        [SerializeField] private Image _healthBar;
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private RectTransform _maskTransform;
        [SerializeField] private RectTransform _healthBarTransform; 

        private float _scaleFactor = 1f;

        protected override void SetVisible(bool visible) {
            _healthBar.enabled = visible;
            _backgroundImage.enabled = visible;
        }

        public override void SetColor(Color color) {
            _healthBar.color = color;
        }

        protected override void LateUpdate() {
            base.LateUpdate();
            DamageModule d = trackedUnit.DamageModule;
            float maskOffsetFactor = Mathf.Clamp01(1f - d.health / d.maxHealth);
            _maskTransform.localPosition = Vector2.down * (maskOffsetFactor * 5f);
            _healthBarTransform.localPosition = Vector2.up * (maskOffsetFactor * 5f);
        }
    }
}