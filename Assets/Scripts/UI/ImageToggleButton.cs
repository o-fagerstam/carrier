using System;
using UnityEngine;
using UnityEngine.UI;

namespace UI {
    public class ImageToggleButton : MonoBehaviour {
        public bool IsOn { get; private set; }
        public bool startOn;
        private Button _button;
        private Image _image;
        public Sprite toggleOnSprite;
        public Sprite toggleOffSprite;
        public event Action<bool> OnToggleChanged; 

        private void Awake() {
            IsOn = startOn;
            _button = GetComponent<Button>();
            _image = GetComponent<Image>();
            
            _button.onClick.AddListener(OnButtonClicked);
        }

        private void OnButtonClicked() {
            if (IsOn) {
                ClickOff();
            }
            else {
                ClickOn();
            }
            OnToggleChanged?.Invoke(IsOn);
        }

        private void ClickOn() {
            IsOn = true;
            _image.sprite = toggleOnSprite;
        }

        private void ClickOff() {
            IsOn = false;
            _image.sprite = toggleOffSprite;
        }
    }
}