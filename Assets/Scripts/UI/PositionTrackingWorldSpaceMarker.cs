using UnityEngine;

namespace UI {
    public abstract class PositionTrackingWorldSpaceMarker : WorldSpaceMarker {
        protected override Vector3 TrackedPosition => trackedPosition;
        protected Vector3 trackedPosition;
    }
}