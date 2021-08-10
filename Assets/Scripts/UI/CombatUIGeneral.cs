using System;
using System.Collections.Generic;
using UnityEngine;

namespace UI {
    public class CombatUIGeneral : MonoBehaviour {
        [SerializeField] private GameObject unitMarker;
        [SerializeField] private Color friendlyColor;
        [SerializeField] private Color hostileColor;
        
        private Dictionary<GameUnit, ClassMarker> _markedUnits = new Dictionary<GameUnit, ClassMarker>();

        private void Start() {
            VehiclesManager.Instance.OnUnitAdded += AddUnit;
            VehiclesManager.Instance.OnUnitRemoved += RemoveUnit;

            foreach (GameUnit unit in VehiclesManager.AllUnits) {
                AddUnit(unit);
            }
        }

        public void AddUnit(GameUnit gameUnit) {
            ClassMarker marker = Instantiate(unitMarker, transform).GetComponent<ClassMarker>();
            _markedUnits[gameUnit] = marker;
            
            marker.trackedTransform = gameUnit.transform;
            marker.offset = new Vector3(0f, 20f, 0f);
            marker.onScreenOffset = new Vector3(0f, 20f, 0f);
            marker.trackedUnit = gameUnit;

            if (gameUnit.team == GameManager.PlayerTeam) {
                marker.SetColor(friendlyColor);
            }
            else {
                marker.SetColor(hostileColor);
            }
        }

        public void RemoveUnit(GameUnit g) {
            if (_markedUnits.ContainsKey(g)) {
                Destroy(_markedUnits[g].gameObject);
                _markedUnits.Remove(g);
            }
        }

        private void OnDestroy() {
            foreach (ClassMarker o in _markedUnits.Values) {
                Destroy(o.gameObject);
            }
        }
    }
}