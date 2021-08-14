using Ship;
using UnityEngine;
using UnityEngine.UI;

namespace UI {
    public class GunTargetMarker : PositionTrackingWorldSpaceMarker {
        public Image image;
        public ShipGun trackedGun;
        
        private static readonly Color ReadyColor = new Color(25f / 255f, 191f / 255f, 70 / 255f);
        private static readonly Color LoadingColor = new Color(219f / 255f, 143f / 255f, 29f / 255f);

        protected override float DistanceScalingPower => 0f;

        public override void SetColor(Color c) {
            image.color = c;
        }

        protected override void LateUpdate() {
            GunImpactPrediction prediction = trackedGun.GunImpactPrediction;
            if (!prediction.willImpact) {
                SetVisible(false);
                return;
            }

            trackedPosition = prediction.impactPosition;
            base.LateUpdate();
            SetColor(trackedGun.IsLoaded ? ReadyColor : LoadingColor);
        }

        protected override void SetVisible(bool visible) {
            image.enabled = visible;
        }
    }
}