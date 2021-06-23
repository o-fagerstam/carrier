using Ship;

namespace UI {
    public interface IShipTrackingUiComponent {
        public void AcquireShip(ShipMain ship);
        public void ReleaseShip();
    }
}