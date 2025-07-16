using System;

namespace ModularConsole.Modules.Console.Commands
{
    public readonly struct ActionCommand : IConsoleCommand
    {
        public string CommandText { get; }
        private Action Action { get; }
        
        public ActionCommand(string commandText, Action action)
        {
            CommandText = commandText;
            Action = action;
        }
        
        public void Execute(string arguments = null)
        {
            Action?.Invoke();
        }
    }
}