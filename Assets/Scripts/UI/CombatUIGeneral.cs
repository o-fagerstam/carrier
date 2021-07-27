using System;
using System.Collections.Generic;
using UnityEngine;

namespace UI {
    public class CombatUIGeneral : MonoBehaviour {
        [SerializeField] private GameObject unitMarker;
        [SerializeField] private Color friendlyColor;
        [SerializeField] private Color hostileColor;
        
        private Dictionary<GameUnit, ScreenProjectedTrackingObject> _markedUnits = new Dictionary<GameUnit, ScreenProjectedTrackingObject>();

        private void Start() {
            VehiclesManager.Instance.OnUnitAdded += AddUnit;
            VehiclesManager.Instance.OnUnitRemoved += RemoveUnit;

            foreach (GameUnit unit in VehiclesManager.AllUnits) {
                AddUnit(unit);
            }
        }

        public void AddUnit(GameUnit g) {
            ScreenProjectedTrackingObject o = Instantiate(unitMarker, transform).GetComponent<ScreenProjectedTrackingObject>();
            _markedUnits[g] = o;

            o.trackedTransform = g.transform;
            o.offset = new Vector3(0f, 20f, 0f);
            o.onScreenOffset = new Vector3(0f, 20f, 0f);

            if (g.team == GameManager.PlayerTeam) {
                o.SetColor(friendlyColor);
            }
            else {
                o.SetColor(hostileColor);
            }
        }

        public void RemoveUnit(GameUnit g) {
            if (_markedUnits.ContainsKey(g)) {
                Destroy(_markedUnits[g].gameObject);
                _markedUnits.Remove(g);
            }
        }

        private void OnDestroy() {
            foreach (ScreenProjectedTrackingObject o in _markedUnits.Values) {
                Destroy(o.gameObject);
            }
        }
    }
}