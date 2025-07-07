using UnityEngine;
using UnityEngine.UIElements;

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

            var styleSheet = Resources.Load<StyleSheet>(STYLES_FILE_NAME);
            var themeStyleSheet = Resources.Load<ThemeStyleSheet>(THEME_STYLESHEET_FILE_NAME);
            var defaultDocumentRoot = Resources.Load<VisualTreeAsset>(DEFAULT_DOCUMENT_ROOT_FILE_NAME);
            
            var consoleObj = new GameObject("ModularConsole - Don't destroy");
            Object.DontDestroyOnLoad(consoleObj);
            var consoleUI = consoleObj.AddComponent<ConsoleUI>();
            consoleUI.Initialize(defaultDocumentRoot, styleSheet, themeStyleSheet);

            ConsoleSystem.UI = consoleUI;
        }
    }
}