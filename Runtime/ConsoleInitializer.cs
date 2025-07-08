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
        private const string STYLES_FILE_NAME = "ModularConsoleStyles";
        private const string THEME_STYLESHEET_FILE_NAME = "UnityDefaultRuntimeTheme";
        private const string DEFAULT_DOCUMENT_ROOT_FILE_NAME = "ModularConsoleDocument";
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Initialize()
        {
            ConsoleSystem.Reset();
            InitializePersistentModules();

            var styleSheet = Resources.Load<StyleSheet>(STYLES_FILE_NAME);
            var themeStyleSheet = Resources.Load<ThemeStyleSheet>(THEME_STYLESHEET_FILE_NAME);
            var defaultDocumentRoot = Resources.Load<VisualTreeAsset>(DEFAULT_DOCUMENT_ROOT_FILE_NAME);
            
            var consoleObj = new GameObject("ModularConsole - Don't destroy");
            Object.DontDestroyOnLoad(consoleObj);
            var consoleUI = consoleObj.AddComponent<ConsoleUI>();
            consoleUI.Initialize(defaultDocumentRoot, styleSheet, themeStyleSheet);

            ConsoleSystem.UI = consoleUI;
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