using System;
using UnityEngine;

public class UpdateableData : ScriptableObject {
    public event Action OnValuesUpdated;
    public bool autoUpdate = true;


    #if UNITY_EDITOR
    protected virtual void OnValidate() {
        if (autoUpdate) {
            UnityEditor.EditorApplication.update += NotifyOfUpdatedValues;
        }
    }

    public void NotifyOfUpdatedValues() {
        UnityEditor.EditorApplication.update -= NotifyOfUpdatedValues;
        OnValuesUpdated?.Invoke();
    }
    #endif

}
