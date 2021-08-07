using System;
using System.Collections.Generic;
using System.Linq;
using CommandMode;
using ServiceLocator;
using UI;
using UnityEngine;

namespace Unit {
    public class SelectionBulb : MonoBehaviour {
        public GameUnit Unit { get; private set; }
        public MeshRenderer _selectionRingRenderer;
        [SerializeField] private GameObject commandLineRendererPrefab;
        private List<CommandLineRenderer> _commandLineRenderers = new List<CommandLineRenderer>();
        private bool _isSelected;
        private bool _displayAllowed;
        private int _numOrdersLastIteration;
        private CommandUi _commandUi;

        private void Awake() {
            Unit = GetComponentInParent<GameUnit>();
            Unit.OnSelected += OnParentSelected;
            Unit.OnDeselected += OnParentDeselected;
            
            _selectionRingRenderer = GetComponentInChildren<MeshRenderer>();
        }

        private void Start() {
            _commandUi = MonoBehaviourServiceLocator.Current.Get<CommandUi>();
            _displayAllowed = _commandUi.DisplaySelection;
            _commandUi.OnDisplaySelectionSettingsChanged += OnUiDisplaySelectionSettingsChanged;
            UpdateSelectionVisibility();
        }

        private void Update() {
            if (_isSelected && _displayAllowed) {
                UpdateLineDrawer();
            }
        }

        private void OnUiDisplaySelectionSettingsChanged(bool value) {
            _displayAllowed = value;
            UpdateSelectionVisibility();
        }

        private void OnParentSelected(GameUnit parent) {
            _isSelected = true;
            UpdateSelectionVisibility();
        }

        private void OnParentDeselected(GameUnit parent) {
            _isSelected = false;
            UpdateSelectionVisibility();
        }

        private void UpdateSelectionVisibility() {
            if (_displayAllowed && _isSelected) {
                _selectionRingRenderer.enabled = true;
            }
            else {
                _selectionRingRenderer.enabled = false;
                foreach (CommandLineRenderer r in _commandLineRenderers) {
                    r.SetVisible(false);
                }
            }
        }

        private void OnDestroy() {
            Unit.OnSelected -= OnParentSelected;
            Unit.OnDeselected -= OnParentDeselected;
            _commandUi.OnDisplaySelectionSettingsChanged -= OnUiDisplaySelectionSettingsChanged;
        }
        
        /*
         * COMMAND LINE DRAWER
         */

        private void UpdateLineDrawer() {
            IReadOnlyCollection<Command> commands = Unit.AiController.CurrentCommands;
            int numCommands = commands.Count;
            
            Vector3[] positions = new Vector3[numCommands + 1];
            Color[] colors = new Color[numCommands];
            bool drawOrders = true;

            positions[0] = transform.position;
            int positionIndex = 1;
            foreach (Command command in commands) {
                CommandDrawInfo drawInfo = command.CreateDrawInfo;
                positions[positionIndex] = drawInfo.Point + new Vector3(0f, 2f, 0f);
                
                if (drawInfo.Instruction == CommandDrawInstruction.DoNotDraw) {
                    drawOrders = false;
                    break;
                } else if (drawInfo.Instruction == CommandDrawInstruction.ColorGreen) {
                    colors[positionIndex-1] = Color.green;
                } else if (drawInfo.Instruction == CommandDrawInstruction.ColorRed) {
                    colors[positionIndex-1] = Color.red;
                }

                positionIndex++;
            }


            if (!drawOrders) {
                ResizeCommandLineRenderersList(0);
                return;
            }
            
            if (_commandLineRenderers.Count != numCommands) {
                ResizeCommandLineRenderersList(numCommands);
            }
            for (int i = 0; i < numCommands; i++) {
                CommandLineRenderer r = _commandLineRenderers[i];
                r.SetVisible(true);
                r.UpdateLine(positions[i], positions[i+1], colors[i]);
            }
        }

        private void ResizeCommandLineRenderersList(int numRenderers) {
            while (numRenderers > _commandLineRenderers.Count) {
                CommandLineRenderer r = Instantiate(commandLineRendererPrefab, transform).GetComponent<CommandLineRenderer>();
                _commandLineRenderers.Add(r);
            }

            while (numRenderers < _commandLineRenderers.Count) {
                Destroy(_commandLineRenderers[_commandLineRenderers.Count - 1].gameObject);
                _commandLineRenderers.RemoveAt(_commandLineRenderers.Count - 1);
            }
        }
    }
}
