using UnityEngine;

namespace Unit {
    public class SelectionBulb : MonoBehaviour {
        public GameUnit Unit { get; private set; }
        public MeshRenderer _selectionRingRenderer;

        private void Awake() {
            Unit = GetComponentInParent<GameUnit>();
            Unit.OnSelected += OnParentSelected;
            Unit.OnDeselected += OnParentDeSelected;
            _selectionRingRenderer = GetComponentInChildren<MeshRenderer>();
        }

        private void OnParentSelected(GameUnit parent) {
            _selectionRingRenderer.enabled = true;
        }

        private void OnParentDeSelected(GameUnit parent) {
            _selectionRingRenderer.enabled = false;
        }

        private void OnDestroy() {
            Unit.OnSelected -= OnParentSelected;
            Unit.OnDeselected -= OnParentDeSelected;
        }
    }
}
