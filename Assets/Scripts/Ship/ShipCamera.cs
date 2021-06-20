using System;
using System.Linq;
using UnityEngine;

namespace Ship {
    public abstract class ShipCamera : MonoBehaviour {
        public static RaycastHit RayCastGunTargetingHit;
        public static bool RayCastMadeGunTargetingHit;
        public static LayerMask GunTargetingMask;
        protected float XRotation, YRotation, ScrollLevel;
        protected Camera CameraComponent;
        protected AudioListener AudioListenerComponent;
        [SerializeField] protected float mouseSensitivity = 100f;
        [SerializeField] protected float MouseScrollSensitivity = 100f;
        [SerializeField] private bool activateOnStart;

        public Transform objectToFollow;
        public static Camera CurrentCamera { get; private set; }

        protected virtual void Awake() {
            CameraComponent = GetComponentInChildren<Camera>();
            AudioListenerComponent = GetComponentInChildren<AudioListener>();
            CameraComponent.enabled = activateOnStart;
            GunTargetingMask = LayerMask.GetMask("Water", "Targetable");
            if (CurrentCamera == null) {
                CurrentCamera = Camera.main;
            }

        }

        private void LateUpdate() {
            if (CameraComponent.enabled) {
                UpdateMouseTarget();
                UpdateCameraPosition();
                GunMarkerDrawer.Instance.RefreshMarkers();
            }
        }

        protected abstract void UpdateCameraPosition();

        public void Activate() {
            CurrentCamera.enabled = false;
            CameraComponent.enabled = true;
            CurrentCamera = CameraComponent;
            Debug.Log("Activated camera " + name);
            Debug.Log("Set current camera to " + CurrentCamera.name);
        }

        private void UpdateMouseTarget() {
            Ray mouseRay = CameraComponent.ViewportPointToRay(new Vector3(0.5f, 0.5f));
            RaycastHit[] hits = new RaycastHit[10];
            int numHits = Physics.RaycastNonAlloc(mouseRay, hits, CameraComponent.farClipPlane, GunTargetingMask);


            if (numHits == 0) {
                RayCastMadeGunTargetingHit = false;
                return;
            }

            Vector3 cameraPos = CameraComponent.transform.position;
            Array.Resize(ref hits, numHits);
            hits = hits.OrderBy(h => (h.point - cameraPos).magnitude).ToArray();

            foreach (RaycastHit hit in hits) {
                Transform t = hit.transform;
                if (t == objectToFollow) {
                    continue;
                }

                bool isValid = true;
                while (t.parent != null) {
                    t = t.parent;
                    if (t != objectToFollow) {
                        continue;
                    }

                    isValid = false;
                    break;
                }

                if (isValid) {
                    RayCastMadeGunTargetingHit = true;
                    RayCastGunTargetingHit = hit;
                    return;
                }
            }
            
            RayCastMadeGunTargetingHit = false;
        }

        private void OnDrawGizmos() {
            if (CameraComponent.enabled) {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(RayCastGunTargetingHit.point, 1f);
            }
        }
    }
}