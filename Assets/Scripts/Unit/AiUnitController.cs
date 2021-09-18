using System;
using System.Collections.Generic;
using CommandMode;
using UnityEngine;

namespace Unit {
    public abstract class AiUnitController : MonoBehaviour {
        protected CommandProcessor commandProcessor;
        public GameUnit ControlledUnit { get; private set; }
        public IReadOnlyCollection<Command> CurrentCommands => commandProcessor.CurrentCommands;

        protected virtual void Awake() {
            commandProcessor = new CommandProcessor(this);
            commandProcessor.OnNewCommand += OnNewCommand;
        }

        protected virtual void Start() {
            ControlledUnit = GetComponentInParent<GameUnit>();
        }

        protected virtual void Update() {
            commandProcessor.ExecuteCurrentCommand();
        }

        public void SetCommand(Command c) {
            commandProcessor.SetCommand(c);
        }

        public void EnqueueCommand(Command c) {
            commandProcessor.EnqueueCommand(c);
        }

        protected virtual void OnDestroy() {
            commandProcessor.OnNewCommand -= OnNewCommand;
        }

        public abstract void OnNewCommand();

        public abstract void IdleCommand();
        public abstract void PointMoveCommand(Vector3 targetPoint);
        public abstract void FollowCommand(GameUnit unitToFollow);
        public abstract void AttackCommand(GameUnit target);
    }
}