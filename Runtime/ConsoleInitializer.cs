using System;
using System.Collections.Generic;
using System.Linq;
using ModularConsole.Contracts;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace ModularConsole
{
    internal static class ConsoleInitializer
    {
        private const string DEFAULT_DOCUMENT_ROOT_FILE_NAME = "ModularConsoleDocument";
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Initialize()
        {
            if (Application.isPlaying == false)
                return;
            
            ConsoleSystem.Reset();
            InitializePersistentModules();

            var styleSheet = Resources.Load<StyleSheet>(ConsoleSystem.INTERNAL_STYLES_FILE_NAME);
            var themeStyleSheet = Resources.Load<ThemeStyleSheet>(ConsoleSystem.THEME_STYLESHEET_FILE_NAME);
            var defaultDocumentRoot = Resources.Load<VisualTreeAsset>(DEFAULT_DOCUMENT_ROOT_FILE_NAME);
            
            var consoleObj = new GameObject("ModularConsole - Don't destroy");
            Object.DontDestroyOnLoad(consoleObj);
            var consoleUI = consoleObj.AddComponent<ConsoleUI>();
            consoleUI.Initialize(defaultDocumentRoot, styleSheet, themeStyleSheet, 
                Resources.Load<PanelSettings>("ModularConsole/" + ConsoleSystem.CONSOLE_STYLE_PANEL_SETTINGS_OVERRIDE_NAME));

            ConsoleSystem.UI = consoleUI;
            
            var styleOverride = Resources.Load<StyleSheet>("ModularConsole/" + ConsoleSystem.CONSOLE_STYLE_OVERRIDE_NAME);
            if (styleOverride)
                consoleUI.AdditionalStyles.Add(styleOverride);

#if UNITY_ANDROID || UNITY_IOS
            styleOverride = Resources.Load<StyleSheet>("ModularConsole/" + ConsoleSystem.CONSOLE_STYLE_MOBILE_OVERRIDE_NAME);
            if (styleOverride)
                consoleUI.AdditionalStyles.Add(styleOverride);
#endif
        }

        private static void InitializePersistentModules()
        {
            var moduleTypes = AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(i => i.GetTypes())
                .Where(t => typeof(PersistentModule).IsAssignableFrom(t) && !t.IsAbstract)
                .ToArray();

            if (moduleTypes.Length <= 0)
                return;
            
            List<IConsoleModule> modules = new();
            foreach (var moduleType in moduleTypes)
            {
                var module = Activator.CreateInstance(moduleType) as IConsoleModule;
                if (module == null)
                    continue;
                
                modules.Add(module);
            }

            ConsoleSystem.Modules.AddRange(modules);
        }
    }
}