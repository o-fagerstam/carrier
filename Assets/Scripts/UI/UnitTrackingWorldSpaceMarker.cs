using UnityEngine;

namespace UI {
    public abstract class UnitTrackingWorldSpaceMarker : WorldSpaceMarker {
        protected override Vector3 TrackedPosition => trackedUnit.Rigidbody.position + worldOffset;
        public GameUnit trackedUnit;
        public Vector3 worldOffset = Vector3.zero;
    }
}