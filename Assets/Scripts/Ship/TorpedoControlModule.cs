using System;
using UnityEngine;

namespace Ship {
    public class TorpedoControlModule : MonoBehaviour {
        public TorpedoLauncher[] TorpedoLaunchers { get; private set; }
        public Ship parentShip;

        private int _currentlyActiveLauncher;
        private bool _isActive = true;
        
        
        private void Awake() {
            parentShip = GetComponentInParent<Ship>();
            TorpedoLaunchers = GetComponentsInChildren<TorpedoLauncher>();
            foreach (TorpedoLauncher launcher in TorpedoLaunchers) {
                launcher.parentShip = parentShip;
            }
            
            TorpedoLaunchers[0].Activate();
        }

        private void Update() {
            if (_isActive && Input.GetKeyDown(KeyCode.Q)) {
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