using System.Collections.Generic;
using ModularConsole.Contracts;
using UnityEngine;
using UnityEngine.UIElements;

namespace ModularConsole.Modules.Console
{
    public class ConsoleModule : PersistentModule
    {
        private static StyleSheet _styleSheet;

        internal static List<IConsoleCommand> Commands = new();

        public override string ModuleName => "Console";
        public bool NeedsUpdate { get; private set; }

        private readonly ScrollView _logScrollView;
        private readonly VisualElement _controlPanel;
        private readonly TextField _commandInput;
        private ConsoleLogMessage _lastLogMessage;
        private VisualElement _root;
        private int _updateCycles;

        public ConsoleModule()
        {
            _controlPanel = new VisualElement()
            {
                name = "control-panel",
            };
            _logScrollView = new ScrollView()
            {
                name = "logs-container"
            };
            _commandInput = new TextField()
            {
                name = "command-input-field"
            };
            _commandInput.Q<TextElement>().RegisterCallback<KeyDownEvent>(OnCommandSubmit);
            
            Application.logMessageReceived -= ApplicationOnLogMessageReceived;
            Application.logMessageReceived += ApplicationOnLogMessageReceived;
        }

        ~ConsoleModule()
        {
            Application.logMessageReceived -= ApplicationOnLogMessageReceived;
        }

        private void ApplicationOnLogMessageReceived(string condition, string stacktrace, LogType type)
        {
            LogRecordBase logRecord = type switch
            {
                LogType.Assert => new AssertLog(condition),
                LogType.Warning => new WarningLog(condition),
                LogType.Log => new SimpleLog(condition),
                _ => new ErrorLog(condition, stacktrace)
            };

            if (_lastLogMessage == null || _lastLogMessage.LogRecord != logRecord)
            {
                _lastLogMessage = new ConsoleLogMessage(logRecord);
                _lastLogMessage.AddToClassList("logRecord");
                _logScrollView.contentContainer.Add(_lastLogMessage);
                NeedsUpdate = true;
                _updateCycles = 0;
                
                return;
            }

            _lastLogMessage.IncreaseAmount();
        }

        private void OnCommandSubmit(KeyDownEvent keyDownEvent)
        {
            if (keyDownEvent.keyCode is KeyCode.KeypadEnter or KeyCode.Return == false)
                return;

            string text = _commandInput.value;
            _commandInput.value = "";
            
            var commandText = text.Contains(' ') ? text.Substring(0, text.IndexOf(' ')) : text;
            IConsoleCommand command = null;
            foreach (var consoleCommand in Commands)
            {
                if (consoleCommand.CommandText != commandText)
                    continue;

                command = consoleCommand;
                break;
            }

            if (command is null)
            {
                Debug.LogWarning($"Failed to execute command {commandText}: No such command found");
                return;
            }

            string argsString = text.Replace(commandText, "");
            if (argsString.Length > 0 && argsString[0] == ' ')
                argsString = argsString[1..];
            
            command.Execute(argsString);
        }

        public void Update()
        {
            var scrollOffset = _logScrollView.scrollOffset;
            scrollOffset.y = _logScrollView.verticalScroller.highValue;
            _logScrollView.scrollOffset = scrollOffset;
            _updateCycles++;
            
            if (_updateCycles > 5)
            {
                _updateCycles = 0;
                NeedsUpdate = false;
            }
            
            _root.Remove(_controlPanel);
            _root.Add(_controlPanel);
            _root.Remove(_logScrollView);
            _root.Add(_logScrollView);
        }
        
        public override void ConstructUI(VisualElement root)
        {
            _root = root;
            
            _styleSheet ??= Resources.Load<StyleSheet>("ConsoleModule");
            root.styleSheets.Add(_styleSheet);
            
            root.Add(_controlPanel);
            root.Add(_logScrollView);
            root.Add(_commandInput);
        }
    }
}