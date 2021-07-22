using System;
using System.Collections.Generic;
using UnityEngine;

namespace UI {
    public abstract class ScreenProjector : MonoBehaviour {
        private RectTransform _canvasRectTransform;
        protected Dictionary<Transform, ScreenProjectedObject> _projectedObjects;

        protected virtual void Awake() {
            _canvasRectTransform = GetComponent<RectTransform>();
        }
    }
}