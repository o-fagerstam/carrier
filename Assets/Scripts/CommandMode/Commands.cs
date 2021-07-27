using System;
using Unit;
using UnityEngine;

namespace CommandMode {
    public abstract class Command {
        protected AiUnitController _controller;
        public event Action OnCommandFinished;
        public abstract CommandDrawInfo CreateDrawInfo { get; }

        public Command(AiUnitController controller) {
            _controller = controller;
        }

        public abstract void Execute();

        public void FinishCommand() {
            OnCommandFinished?.Invoke();
        }
    }

    public struct CommandDrawInfo {
        public readonly CommandDrawInstruction Instruction;
        public readonly Vector3 Point;
        public CommandDrawInfo(CommandDrawInstruction instruction, Vector3 point) {
            Instruction = instruction;
            Point = point;
        }
    }

    public enum CommandDrawInstruction {
        DoNotDraw, ColorRed, ColorGreen
    }

    public class IdleCommand : Command {
        public override CommandDrawInfo CreateDrawInfo => new CommandDrawInfo(CommandDrawInstruction.DoNotDraw, Vector3.zero);
        public IdleCommand(AiUnitController controller) : base(controller) { }

        public override void Execute() {
            _controller.IdleCommand();
        }
    }

    public class MoveToPointCommand : Command {
        private Vector3 _targetPoint;
        private float _finishDistance;
        
        public override CommandDrawInfo CreateDrawInfo => new CommandDrawInfo(CommandDrawInstruction.ColorGreen, _targetPoint);
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
        public override CommandDrawInfo CreateDrawInfo => new CommandDrawInfo(CommandDrawInstruction.ColorGreen, _unitToFollow.transform.position);
        public FollowCommand(AiUnitController controller, GameUnit unitToFollow) : base(controller) {
            _unitToFollow = unitToFollow;
        }

        public override void Execute() {
            _controller.FollowCommand(_unitToFollow);
        }
    }

    public class AttackCommand : Command {
        private GameUnit _target;
        
        public override CommandDrawInfo CreateDrawInfo => new CommandDrawInfo(CommandDrawInstruction.ColorRed, _target.transform.position);

        public AttackCommand(AiUnitController controller, GameUnit target) : base(controller) {
            _target = target;
        }

        public override void Execute() {
            if (!_target.alive) {
                FinishCommand();
            }
            _controller.AttackCommand(_target);
        }
    }
}