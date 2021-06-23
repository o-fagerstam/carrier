using System;
using UnityEngine;

namespace Ship {
    public class TorpedoControlModule : MonoBehaviour {
        public TorpedoLauncher[] TorpedoLaunchers { get; private set; }
        public ShipMain parentShip;

        private int _currentlyActiveLauncher;
        private bool _isActive = true;
        
        
        private void Awake() {
            parentShip = GetComponentInParent<ShipMain>();
            TorpedoLaunchers = GetComponentsInChildren<TorpedoLauncher>();
            foreach (TorpedoLauncher launcher in TorpedoLaunchers) {
                launcher.parentShip = parentShip;
            }
            
            TorpedoLaunchers[0].Activate();
        }

        private void Update() {
            if (_isActive && parentShip.shipController.GetTorpedoInput()) {
                FireTorpedo();
            }
        }

        private void FireTorpedo() {
            TorpedoLaunchers[_currentlyActiveLauncher++].FireTorpedo();
            if (_currentlyActiveLauncher < TorpedoLaunchers.Length) {
                TorpedoLaunchers[_currentlyActiveLauncher].Activate();
            }
            else {
                Deactivate();
            }
        }

        private void Deactivate() {
            _isActive = false;
        }
    }
}