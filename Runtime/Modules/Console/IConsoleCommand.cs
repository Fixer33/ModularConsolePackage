namespace ModularConsole.Modules.Console
{
    public interface IConsoleCommand
    {
        public string CommandText { get; }
        public void Execute(string arguments = null);
    }

    public static class IConsoleCommandExtensions
    {
        public static void Register(this IConsoleCommand command)
        {
            ConsoleModule.Commands.Add(command);
        }
        
        public static void UnRegister(this IConsoleCommand command)
        {
            if (ConsoleModule.Commands.Contains(command) == false)
                ConsoleModule.Commands.Remove(command);
        }
    }
}