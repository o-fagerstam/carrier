using System;
using System.Collections.Generic;
using CommandMode;
using UnityEngine;

namespace Unit {
    public abstract class AiUnitController : MonoBehaviour {
        private CommandProcessor _commandProcessor;
        public GameUnit ControlledUnit { get; private set; }
        public IReadOnlyCollection<Command> CurrentCommands => _commandProcessor.CurrentCommands;

        protected virtual void Awake() {
            _commandProcessor = new CommandProcessor(this);
            _commandProcessor.OnNewCommand += OnNewCommand;
        }

        protected virtual void Start() {
            ControlledUnit = GetComponentInParent<GameUnit>();
        }

        protected virtual void Update() {
            _commandProcessor.ExecuteCurrentCommand();
        }

        public void SetCommand(Command c) {
            _commandProcessor.SetCommand(c);
        }

        public void EnqueueCommand(Command c) {
            _commandProcessor.EnqueueCommand(c);
        }

        protected virtual void OnDestroy() {
            _commandProcessor.OnNewCommand -= OnNewCommand;
        }

        public abstract void OnNewCommand();

        public abstract void IdleCommand();
        public abstract void PointMoveCommand(Vector3 targetPoint);
        public abstract void FollowCommand(GameUnit unitToFollow);
        public abstract void AttackCommand(GameUnit target);
    }
}