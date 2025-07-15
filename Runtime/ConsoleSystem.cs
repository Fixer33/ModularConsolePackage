using System.Collections.Generic;
using ModularConsole.Contracts;

namespace ModularConsole
{
    public static class ConsoleSystem
    {
        public const string THEME_STYLESHEET_FILE_NAME = "UnityDefaultRuntimeTheme";
        public const string INTERNAL_STYLES_FILE_NAME = "ModularConsoleStyles";
        public const string CONSOLE_STYLE_OVERRIDE_NAME = "ModularConsoleStyleBaseOverride";
        public const string CONSOLE_STYLE_MOBILE_OVERRIDE_NAME = "ModularConsoleStyleBaseMobileOverride";
        public const string CONSOLE_STYLE_PANEL_SETTINGS_OVERRIDE_NAME = "ConsolePanelSettingsOverride";
        
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