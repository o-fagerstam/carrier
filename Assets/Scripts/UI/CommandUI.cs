using System;
using CommandMode;
using UnityEngine;
using UnityEngine.UI;

namespace UI {
    public class CommandUI : MonoBehaviour {
        [SerializeField] private Toggle alwaysDisplaySelectionToggle;
        private static CommandUI _instance;
        public static CommandUI Instance => _instance;
        public bool DisplaySelection { get; private set; }
        private bool _commandModeActive;
        public event Action<bool> OnDisplaySelectionSettingsChanged;

        private void Awake() {
            if (_instance != null && _instance != this) {
                Destroy(gameObject);
            }
            else {
                _instance = this;
            }

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
            RecalculateDisplaySelection();
        }

        private void OnExitCommandMode() {
            _commandModeActive = false;
            RecalculateDisplaySelection();
        }

        private void OnDestroy() {
            CommandModeController.OnEnterCommandMode -= OnEnterCommandMode;
            CommandModeController.OnExitCommandMode -= OnExitCommandMode;
        }
    }
}