using System;
using ServiceLocator;
using Ship;
using UnityEngine;

public class GameManager : MonoBehaviourService {
    public const int PlayerTeam = 0;
    private GameSpeed _lastGameSpeed = GameSpeed.Normal;
    private bool _lateInitFinished;
    
    private void Update() {
        if (!_lateInitFinished) {
            _lateInitFinished = true;
            
            PlayerShipController psc = MonoBehaviourServiceLocator.Current.Get<PlayerShipController>();
            PlayerCamera playerCamera = MonoBehaviourServiceLocator.Current.Get<PlayerCamera>();
            playerCamera.SwitchController(psc);
        }
    }

    public void SetGameSpeed(GameSpeed speed) {
        if (speed != GameSpeed.Paused) {
            _lastGameSpeed = speed;
        }
        ChangeTimeScale(speed);
    }

    private void ChangeTimeScale(GameSpeed speed) {
        switch (speed) {
            case GameSpeed.Normal:
                Time.timeScale = 1f;
                break;
            case GameSpeed.Paused:
                Time.timeScale = 0f;
                break;
        }
    }

    public void Resume() {
        ChangeTimeScale(_lastGameSpeed);
    }

    public enum GameSpeed {
        Paused, Normal
    }
}
