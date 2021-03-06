using System;
using UnityEngine;

namespace Ship {
    public class TorpedoControlModule : MonoBehaviour {
        private TorpedoLauncher[] _torpedoLaunchers;
        public ShipMain parentShip;

        private int _currentlyActiveLauncher;


        private void Awake() {
            parentShip = GetComponentInParent<ShipMain>();
            _torpedoLaunchers = GetComponentsInChildren<TorpedoLauncher>();
            foreach (TorpedoLauncher launcher in _torpedoLaunchers) {
                launcher.parentShip = parentShip;
            }
            
            _torpedoLaunchers[0].Activate();
            parentShip.OnDeath += OnParentDeath;
        }

        public void ReceiveTorpedoInput () {
            if (parentShip.IsActive &&
                _currentlyActiveLauncher < _torpedoLaunchers.Length) {
                FireTorpedo();
            }
        }

        private void FireTorpedo() {
            _torpedoLaunchers[_currentlyActiveLauncher++].FireTorpedo();
            if (_currentlyActiveLauncher < _torpedoLaunchers.Length) {
                _torpedoLaunchers[_currentlyActiveLauncher].Activate();
            }
        }

        private void OnParentDeath(GameUnit _) {
            parentShip.OnDeath -= OnParentDeath;
            foreach (TorpedoLauncher torpedoLauncher in _torpedoLaunchers) {
                torpedoLauncher.Deactivate();
            }
        }

        private void OnDestroy() {
            parentShip.OnDeath -= OnParentDeath;
        }

    }
}