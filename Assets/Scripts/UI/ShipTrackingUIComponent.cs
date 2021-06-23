using Ship;

namespace UI {
    public interface ShipTrackingUIComponent {
        public void AcquireShip(ShipMain ship);
        public void ReleaseShip();
    }
}