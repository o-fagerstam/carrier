using System;
using System.Collections.Generic;
using Unit;
using UnityEngine;

namespace CommandMode {
    public class CommandProcessor {
        private Queue<Command> _commands = new Queue<Command>();
        private Command ActiveCommand => _commands.Peek();
        private bool Idle => ActiveCommand.GetType() == typeof(EmptyCommand);
        public event Action OnNewCommand;
        private AiUnitController _owner;

        public CommandProcessor(AiUnitController owner) {
            _owner = owner;
            SetCommand(new EmptyCommand(owner));
        }

        public void SetCommand(Command o) {
            ClearAllCommands();
            _commands.Enqueue(o);
            o.OnCommandFinished += OnCommandFinished;
            OnNewCommand?.Invoke();
        }

        public void EnqueueCommand(Command o) {
            if (Idle) {
                SetCommand(o);
            }
            _commands.Enqueue(o);
            o.OnCommandFinished += OnCommandFinished;
        }

        public void ExecuteCurrentCommand() {
            ActiveCommand.Execute();
        }

        public void OnCommandFinished() {
            ActiveCommand.OnCommandFinished -= OnCommandFinished;
            _commands.Dequeue();
            OnNewCommand?.Invoke();
        }
        
        public void ClearAllCommands() {
            foreach (Command command in _commands) {
                command.OnCommandFinished -= OnCommandFinished;
            }
            _commands.Clear();
        }

        ~CommandProcessor() {
            ClearAllCommands();
        }


    }
}