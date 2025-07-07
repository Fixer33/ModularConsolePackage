using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace ModularConsole
{
    public class ConsoleModule : IConsoleModule
    {
        private static StyleSheet _styleSheet;

        public string ModuleName => "Console";

        private readonly List<string> _logs = new List<string>();

        public ConsoleModule()
        {
            Application.logMessageReceived -= ApplicationOnLogMessageReceived;
            Application.logMessageReceived += ApplicationOnLogMessageReceived;
        }

        ~ConsoleModule()
        {
            Application.logMessageReceived -= ApplicationOnLogMessageReceived;
        }

        private void ApplicationOnLogMessageReceived(string condition, string stacktrace, LogType type)
        {
            _logs.Add(condition);
        }
        
        public void ConstructUI(VisualElement root)
        {
            _styleSheet ??= Resources.Load<StyleSheet>("ConsoleModule");
            root.styleSheets.Add(_styleSheet);

            var logListView = new ScrollView()
            {
                name = "logs-container"
            };
            foreach (var log in _logs)
            {
                logListView.contentContainer.Add(new Label()
                {
                    text = log
                });
            }
            root.Add(logListView);
        }
    }
}