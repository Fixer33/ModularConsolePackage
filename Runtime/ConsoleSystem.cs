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
    }
}