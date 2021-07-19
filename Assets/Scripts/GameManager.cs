using System;
using Ship;
using UnityEngine;

public class GameManager : MonoBehaviour {
    private static GameManager _instance;
    public static GameManager Instance => _instance;
    private GameSpeed _lastGameSpeed = GameSpeed.Normal;
    private bool _lateInitFinished;

    private void Awake() {
        if (_instance != null && _instance != this) {
            Destroy(gameObject);
        }
        else {
            _instance = this;
        }
    }

    private void Update() {
        if (!_lateInitFinished) {
            _lateInitFinished = true;
            PlayerShipController.Instance.AcquireControl();
        }
    }

    public void SetGameSpeed(GameSpeed speed) {
        if (speed != GameSpeed.Paused) {
            _lastGameSpeed = speed;
        }
        ChangeTimeScale(speed);
    }

    private void ChangeTimeScale(GameSpeed speed) {
        Debug.Log("Changing game speed to " + speed);
        switch (speed) {
            case GameSpeed.Normal:
                Time.timeScale = 1f;
                Debug.Log($"Timescale is {Time.timeScale}");
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
