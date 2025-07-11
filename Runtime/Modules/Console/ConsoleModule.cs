using System.Collections.Generic;
using ModularConsole.Contracts;
using UnityEngine;
using UnityEngine.UIElements;

namespace ModularConsole.Modules.Console
{
    public class ConsoleModule : PersistentModule
    {
        private const string LOG_TOGGLE_CLASS_NAME = "logFilterToggle";
        
        private static StyleSheet _styleSheet;

        internal static List<IConsoleCommand> Commands = new();

        public override string ModuleName => "Console";
        public bool NeedsUpdate { get; private set; }

        private readonly List<ConsoleLogMessage> _messageLogRecords = new();
        
        private readonly ScrollView _logScrollView = new() { name = "logs-container" };
        private readonly TextField _commandInput = new() { name = "command-input-field" };

        private readonly VisualElement _controlPanel = new() { name = "control-panel", };
        private readonly Toggle _showLogsTgl = new() { name = "control__show-logs-tgl", text = "Show Logs", value = true };
        private readonly Toggle _showWarningsTgl = new() { name = "control__show-warnings-tgl", text = "Show Warnings", value = true };
        private readonly Toggle _showErrorsTgl = new() { name = "control__show-errors-tgl", text = "Show Errors", value = true };
        private readonly Toggle _showAssertionsTgl = new() { name = "control__show-assertions-tgl", text = "Show Assertions", value = true };
        
        private ConsoleLogMessage _lastLogMessage;
        private VisualElement _root;
        private int _updateCycles;

        public ConsoleModule()
        {
            _controlPanel.Add(_showLogsTgl);
            _showLogsTgl.RegisterValueChangedCallback(OnLogFiltersChanged);
            _showLogsTgl.AddToClassList(LOG_TOGGLE_CLASS_NAME);
            _controlPanel.Add(_showWarningsTgl);
            _showWarningsTgl.RegisterValueChangedCallback(OnLogFiltersChanged);
            _showWarningsTgl.AddToClassList(LOG_TOGGLE_CLASS_NAME);
            _controlPanel.Add(_showErrorsTgl);
            _showErrorsTgl.RegisterValueChangedCallback(OnLogFiltersChanged);
            _showErrorsTgl.AddToClassList(LOG_TOGGLE_CLASS_NAME);
            _controlPanel.Add(_showAssertionsTgl);
            _showAssertionsTgl.RegisterValueChangedCallback(OnLogFiltersChanged);
            _showAssertionsTgl.AddToClassList(LOG_TOGGLE_CLASS_NAME);
            
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
                LogType.Error => new ErrorLog(condition, stacktrace),
                LogType.Exception => new ErrorLog(condition, stacktrace),
                LogType.Log => new SimpleLog(condition),
                _ => new WarningLog(condition),
            };

            if (_lastLogMessage == null || _lastLogMessage.LogRecord != logRecord)
            {
                _lastLogMessage = new ConsoleLogMessage(logRecord);
                _messageLogRecords.Add(_lastLogMessage);
                _lastLogMessage.AddToClassList("logRecord");
                _logScrollView.contentContainer.Add(_lastLogMessage);
                NeedsUpdate = true;
                _updateCycles = 0;
                
                return;
            }

            _lastLogMessage.IncreaseAmount();
        }

        /// <summary>
        /// Submit entered command to be executed
        /// </summary>
        /// <param name="keyDownEvent"></param>
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
        
        private void OnLogFiltersChanged(ChangeEvent<bool> _) // ignore the value, because different toggles invoke this callback
        {
            foreach (var consoleLogMessage in _messageLogRecords)
            {
                var isVisible = consoleLogMessage.IsLog && _showLogsTgl.value;
                isVisible |= consoleLogMessage.IsWarning && _showWarningsTgl.value;
                isVisible |= consoleLogMessage.IsError && _showErrorsTgl.value;
                consoleLogMessage.SetEnabled(isVisible);
            }
        }

        public void Update()
        {
            // Scroll to the last log message
            var scrollOffset = _logScrollView.scrollOffset;
            scrollOffset.y = _logScrollView.verticalScroller.highValue;
            _logScrollView.scrollOffset = scrollOffset;
            _updateCycles++;
            
            // Make sure scroll view has been recalculated, skip 5 frames before update stop
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