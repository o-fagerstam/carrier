using System;
using PhysicsUtilities;
using Ship;
using Unit;
using UnityEngine;

namespace CommandMode {
    public abstract class Command {
        protected AiUnitController _controller;
        public event Action OnCommandFinished;

        public Command(AiUnitController controller) {
            _controller = controller;
        }

        public abstract void Execute();

        public void FinishCommand() {
            OnCommandFinished?.Invoke();
        }
    }

    public class EmptyCommand : Command {
        public EmptyCommand(AiUnitController controller) : base(controller) { }
        public override void Execute() { }
    }

    public class MoveToPointCommand : Command {
        private Vector3 _targetPoint;
        private float _finishDistance;
        public MoveToPointCommand(AiUnitController controller, Vector3 targetPoint, float finishDistance) : base(controller) {
            _targetPoint = targetPoint;
            _finishDistance = finishDistance;
        }

        public override void Execute() {
            if ((_controller.ControlledUnit.transform.position - _targetPoint).magnitude < _finishDistance) {
                FinishCommand();
                return;
            }
            _controller.PointMoveCommand(_targetPoint);
        }
    }

    public class FollowCommand : Command {
        private GameUnit _unitToFollow;
        public FollowCommand(AiUnitController controller, GameUnit unitToFollow) : base(controller) {
            _unitToFollow = unitToFollow;
        }

        public override void Execute() {
            _controller.FollowCommand(_unitToFollow);
        }
    }
}