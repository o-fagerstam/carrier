using System.Collections.Generic;
using Ship;
using Unit;
using UnityEngine;

namespace CommandMode {
    public class CommandModeController : MonoBehaviour {
        private static CommandModeController _instance;
        public static CommandModeController Instance => _instance;
        private PlayerCamera _playerCamera;

        private bool _hasControl;
        private bool _acquiredControlThisFrame;
        
        private const float HorizontalScrollSpeed = 2000f;
        private const float VerticalScrollSpeed = 2000f;

        private HashSet<GameUnit> _selectedUnits = new HashSet<GameUnit>();

        private void Awake() {
            if (_instance != null && _instance != this) {
                Destroy(gameObject);
            }
            else {
                _instance = this;
            }
            
            _playerCamera = FindObjectOfType<PlayerCamera>();
        }

        public void LateUpdate() {
            if (!_hasControl) {
                return;
            }

            if (!_acquiredControlThisFrame && Input.GetKeyDown(KeyCode.Tab)) {
                ReleaseControl();
                PlayerShipController.Instance.AcquireControl();
            }

            Scroll();
            if (Input.GetMouseButtonDown(0)) {
                LeftClickSelection();
            } else if (Input.GetMouseButtonDown(1)) {
                RightClickOrder();
            }
            
            if (_acquiredControlThisFrame) {
                _acquiredControlThisFrame = false;
            }
        }

        public void AcquireControl() {
            _hasControl = true;
            _acquiredControlThisFrame = true;
            Cursor.lockState = CursorLockMode.None;
            GameManager.Instance.SetGameSpeed(GameManager.GameSpeed.Paused);
            _playerCamera.SetMode(PlayerCamera.CameraMode.Command);
            _playerCamera.FollowTransform(transform);
        }

        public void ReleaseControl() {
            _hasControl = false;
            _playerCamera.Release();
            GameManager.Instance.Resume();
        }
        
        /*
         * CAMERA MOVEMENT
         */

        private void Scroll() {
            float xScroll = Input.GetAxisRaw("Horizontal");
            float zScroll = Input.GetAxisRaw("Vertical");
            float yScroll = Input.GetAxisRaw("Mouse ScrollWheel");

            float horizontalScrollFactor = HorizontalScrollSpeed * Time.unscaledDeltaTime;
            
            transform.Translate(new Vector3(
                xScroll * horizontalScrollFactor,
                -yScroll * VerticalScrollSpeed,
                zScroll * horizontalScrollFactor
                ), Space.World);
        }
        
        /*
         * SELECTION
         */

        /// <summary>
        /// Selects a single unit with a left click.
        /// </summary>
        private void LeftClickSelection() {
            bool madeHit = UnitRay(out GameUnit hitUnit);
            
            if (!madeHit) {
                DeselectAll();
                return;
            }
            
            if (hitUnit.team != GameManager.PlayerTeam) {
                return;
            }
            
            DeselectAll();
            SelectUnit(hitUnit);
            
        }

        private void RightClickOrder() {
            if (_selectedUnits.Count == 0) {
                return;
            }
            bool unitHit = UnitRay(out GameUnit hitUnit);
            if (unitHit) {
                if (hitUnit.team == GameManager.PlayerTeam) {
                    foreach (GameUnit selectedUnit in _selectedUnits) {
                        Command c = new FollowCommand(selectedUnit.AiController, hitUnit);
                        SetOrEnqueueCommand(selectedUnit.AiController, c);
                    }
                    return;
                }
                else {
                    foreach (GameUnit selectedUnit in _selectedUnits) {
                        Command c = new AttackCommand(selectedUnit.AiController, hitUnit);
                        SetOrEnqueueCommand(selectedUnit.AiController, c);
                    }
                    return;
                }

            }

            bool terrainHit = TerrainRay(out Vector3 hitPosition);
            if (terrainHit) {
                foreach (GameUnit selectedUnit in _selectedUnits) {
                    Command c = new MoveToPointCommand(selectedUnit.AiController, hitPosition, 100f);
                    SetOrEnqueueCommand(selectedUnit.AiController, c);
                }
            }
        }

        private void SetOrEnqueueCommand(AiUnitController controller, Command command) {
            if (Input.GetKey(KeyCode.LeftShift)) {
                controller.EnqueueCommand(command);
            }
            else {
                controller.SetCommand(command);
            }
        }
        
        /// <summary>
        /// Makes a raycast to check for unit selection.
        /// </summary>
        /// <param name="hitUnit">Reference to the unit that was hit (returns null if no hit).</param>
        /// <returns>True if a unit was hit, else false.</returns>
        private bool UnitRay(out GameUnit hitUnit) {
            Ray mouseRay = _playerCamera.Camera.ScreenPointToRay(Input.mousePosition);
            bool madeHit = Physics.Raycast(
                mouseRay,
                out RaycastHit hit,
                _playerCamera.Camera.farClipPlane,
                (int) LayerMasks.Selectable
            );
            hitUnit = madeHit? hit.transform.GetComponent<GameUnit>() : null;
            return madeHit;
        }

        /// <summary>
        /// Makes a raycast to check for terrain hit.
        /// </summary>
        /// <param name="hitPosition">The position that was hit (returns Vector3.zero if no hit).</param>
        /// <returns>True if terrain was hit, else false.</returns>
        private bool TerrainRay(out Vector3 hitPosition) {
            Ray mouseRay = _playerCamera.Camera.ScreenPointToRay(Input.mousePosition);
            bool madeHit = Physics.Raycast(
                mouseRay,
                out RaycastHit hit,
                _playerCamera.Camera.farClipPlane,
                (int) LayerMasks.Terrain
            );
            hitPosition = madeHit? hit.point : Vector3.zero;
            return madeHit;
        }

        private void DeselectAll() {
            foreach (GameUnit unit in _selectedUnits) {
                ProcessDeselection(unit);
            }
            _selectedUnits.Clear();
        }

        private void SelectUnit(GameUnit u) {
            _selectedUnits.Add(u);
            u.Select();
            u.OnDeath += OnSelectedUnitDeath;
        }

        private void DeselectUnit(GameUnit u) {
            _selectedUnits.Remove(u);
            ProcessDeselection(u);
        }

        /// <summary>
        /// Processes deselection of a unit without removing it from _selectedUnits.
        /// Used when iterating over _selectedUnits without changing its contents.
        /// (Remember to clear the unit from _selectedUnits afterwards.)
        /// </summary>
        private void ProcessDeselection(GameUnit u) {
            u.Deselect();
            u.OnDeath -= OnSelectedUnitDeath;
        }

        private void OnSelectedUnitDeath(GameUnit u) {
            DeselectUnit(u);
        }

        private void OnDestroy() {
            foreach (GameUnit unit in _selectedUnits) {
                unit.OnDeath -= OnSelectedUnitDeath;
            }
        }
    }
}