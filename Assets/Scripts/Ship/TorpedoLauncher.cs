using System;
using PhysicsUtilities;
using UnityEngine;

namespace Ship {
    public class TorpedoLauncher : MonoBehaviour {
        [SerializeField] private Torpedo torpedo;
        [SerializeField] private GameObject torpedoArrowPrefab;
        [SerializeField] Transform torpedoLaunchPosition;
        private bool _isActive;

        private GameObject _torpedoArrow;
        public Ship parentShip;

        private void Update() {
            if (!_isActive) {
                return;
            }

            Vector3 torpedoArrowNewPos =
                torpedoLaunchPosition.position + VectorTools.HorizontalComponent(torpedoLaunchPosition.forward).normalized * 15f;
            torpedoArrowNewPos.y = Mathf.Max(0.5f, torpedoArrowNewPos.y);
            _torpedoArrow.transform.position = torpedoArrowNewPos;
            _torpedoArrow.transform.LookAt(torpedoArrowNewPos + torpedoLaunchPosition.forward);
        }

        public void FireTorpedo() {
            torpedo.Activate();
            Deactivate();
        }

        public void Activate() {
            if (_torpedoArrow == null) {
                _torpedoArrow = Instantiate(torpedoArrowPrefab);
            }
            _isActive = true;
        }

        public void Deactivate() {
            if (_torpedoArrow != null) {
                Destroy(_torpedoArrow);
            }
            _isActive = false;
        }
    }
}