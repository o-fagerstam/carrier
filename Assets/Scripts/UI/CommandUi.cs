using System;
using CommandMode;
using ServiceLocator;
using UnityEngine;
using UnityEngine.UI;

namespace UI {
    public class CommandUi : MonoBehaviourService {
        [SerializeField] private Toggle alwaysDisplaySelectionToggle;
        [SerializeField] private GameObject commandViewControlPanel;
        public bool DisplaySelection { get; private set; }
        private bool _commandModeActive;
        public event Action<bool> OnDisplaySelectionSettingsChanged;

        protected override void Awake() {
            base.Awake();
            commandViewControlPanel.SetActive(_commandModeActive);
            
            DisplaySelection = false;
            alwaysDisplaySelectionToggle.onValueChanged.AddListener(OnDisplaySelectionToggleChanged);
        }

        private void Start() {
            CommandModeController.OnEnterCommandMode += OnEnterCommandMode;
            CommandModeController.OnExitCommandMode += OnExitCommandMode;
        }

        private void RecalculateDisplaySelection() {
            DisplaySelection = _commandModeActive || alwaysDisplaySelectionToggle.isOn;
            OnDisplaySelectionSettingsChanged?.Invoke(DisplaySelection);
        }

        private void OnDisplaySelectionToggleChanged(bool value) {
            RecalculateDisplaySelection();
        }

        private void OnEnterCommandMode() {
            _commandModeActive = true;
            commandViewControlPanel.SetActive(true);
            RecalculateDisplaySelection();
        }

        private void OnExitCommandMode() {
            _commandModeActive = false;
            commandViewControlPanel.SetActive(false);
            RecalculateDisplaySelection();
        }

        private void OnDestroy() {
            CommandModeController.OnEnterCommandMode -= OnEnterCommandMode;
            CommandModeController.OnExitCommandMode -= OnExitCommandMode;
        }
    }
}