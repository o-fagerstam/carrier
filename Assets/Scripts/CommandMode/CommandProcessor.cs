using System;
using System.Collections.Generic;
using Unit;

namespace CommandMode {
    public class CommandProcessor {
        private AiUnitController _controller;
        
        private Queue<Command> _commands = new Queue<Command>();
        private Command ActiveCommand => _commands.Peek();
        public IReadOnlyCollection<Command> CurrentCommands => _commands;
        private bool Idle => ActiveCommand.GetType() == typeof(IdleCommand);
        public event Action OnNewCommand;


        public CommandProcessor(AiUnitController controller) {
            _controller = controller;
            SetCommand(new IdleCommand(controller));
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
            if (_commands.Count == 0) {
                _commands.Enqueue(new IdleCommand(_controller));
            }
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