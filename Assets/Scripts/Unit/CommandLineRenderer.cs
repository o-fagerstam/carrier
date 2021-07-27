using System;
using UnityEngine;

namespace Unit {
    public class CommandLineRenderer : MonoBehaviour {
        private LineRenderer _lineRenderer;
        private const float DistanceScaleFactor = 500f;

        private void Awake() {
            _lineRenderer = GetComponent<LineRenderer>();
        }

        public void UpdateLine(Vector3 startPos, Vector3 endPos, Color color) {
            _lineRenderer.SetPosition(0, startPos);
            _lineRenderer.SetPosition(1, endPos);
            _lineRenderer.startColor = color;
            _lineRenderer.endColor = color;
            Vector3 cameraPosition = PlayerCamera.Position;
            _lineRenderer.startWidth = Vector3.Distance(startPos, cameraPosition) / DistanceScaleFactor;
            _lineRenderer.endWidth = Vector3.Distance(endPos, cameraPosition) / DistanceScaleFactor;
        }

        public void SetVisible(bool visible) {
            _lineRenderer.enabled = visible;
        }
    }
}