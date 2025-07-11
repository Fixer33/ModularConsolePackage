using System.Collections.Generic;
using ModularConsole.Contracts;

namespace ModularConsole
{
    public static class ConsoleSystem
    {
        public static ConsoleUI UI { get; internal set; }

        internal static List<IConsoleModule> Modules { get; private set; } = new();

        public static void Reset()
        {
            Modules.Clear();
        }

        public static void AddModule(IConsoleModule runtimeModule)
        {
            Modules.Add(runtimeModule);
        }

        public static void RemoveModule(IConsoleModule runtimeModule)
        {
            if (Modules.Contains(runtimeModule) == false)
                Modules.Remove(runtimeModule);
        }
    }
}