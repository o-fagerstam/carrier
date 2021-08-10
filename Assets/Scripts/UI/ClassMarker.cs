using System;
using Unit;
using UnityEngine;
using UnityEngine.UI;

namespace UI {
    public class ClassMarker : ScreenProjectedTrackingObject {
        [SerializeField] private Image _healthBar;
        [SerializeField] private RectTransform _maskTransform;
        [SerializeField] private RectTransform _healthBarTransform; 
        [SerializeField] private Image _backgroundImage;
        public GameUnit trackedUnit;
        private float _scaleFactor = 1f;

        public override void SetVisible(bool visible) {
            base.SetVisible(visible);
            _healthBar.enabled = visible;
            _backgroundImage.enabled = visible;
        }

        public void SetColor(Color color) {
            _healthBar.color = color;
        }

        protected override void LateUpdate() {
            base.LateUpdate();
            DamageModule d = trackedUnit.DamageModule;
            float maskOffsetFactor = Mathf.Clamp01(1f - d.health / d.maxHealth);
            _maskTransform.localPosition = Vector2.down * (maskOffsetFactor * 20f);
            _healthBarTransform.localPosition = Vector2.up * (maskOffsetFactor * 20f);
        }
    }
}