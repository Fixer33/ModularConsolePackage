using System.Collections.Generic;
using ModularConsole.Contracts;
using UnityEngine;
using UnityEngine.UIElements;

namespace ModularConsole.Modules.Console
{
    public class ConsoleModule : PersistentModule
    {
        private const string COMMAND_INPUT_FOCUSED_CLASS_NAME = "command-input--focused";
        private const string COMMAND_INPUT_PLACEHOLDER = "Enter command…  (Enter to run)";

        private static StyleSheet _styleSheet;

        internal static List<IConsoleCommand> Commands = new();

        public override int ConsoleTabOrder => -1;
        public override string ModuleName => "Console";
        public bool NeedsUpdate { get; private set; }

        private readonly List<ConsoleLogMessage> _messageLogRecords = new();
        
        private readonly ScrollView _logScrollView = new() { name = "logs-container" };
        private readonly VisualElement _commandInputContainer = new() { name = "command-input" };
        private readonly TextField _commandInput = new() { name = "command-input-field" };

        private readonly VisualElement _controlPanel = new() { name = "control-panel", };
        private readonly Toggle _showLogsTgl = new() { name = "control__show-logs-tgl", text = "Logs", value = true };
        private readonly Toggle _showWarningsTgl = new() { name = "control__show-warnings-tgl", text = "Warnings", value = true };
        private readonly Toggle _showErrorsTgl = new() { name = "control__show-errors-tgl", text = "Errors", value = true };
        private readonly Toggle _showAssertionsTgl = new() { name = "control__show-assertions-tgl", text = "Assertions", value = true };
        private readonly Button _clearBtn = new() { name = "control__clear-btn", text = "Clear" };

        private ConsoleLogMessage _lastLogMessage;
        private VisualElement _root;
        private int _updateCycles;

        public ConsoleModule()
        {
            foreach (var toggle in new[] { _showLogsTgl, _showWarningsTgl, _showErrorsTgl, _showAssertionsTgl })
            {
                _controlPanel.Add(toggle);
                ConsoleToggleStyle.Apply(toggle);
                toggle.RegisterValueChangedCallback(OnLogFiltersChanged);
            }

            _clearBtn.AddToClassList("console-button");
            _clearBtn.clicked += ClearLogs;
            _controlPanel.Add(_clearBtn);

            var prompt = new Label("›") { name = "command-input__prompt", pickingMode = PickingMode.Ignore };
            _commandInputContainer.Add(prompt);
            _commandInputContainer.Add(_commandInput);
            _commandInput.textEdition.placeholder = COMMAND_INPUT_PLACEHOLDER;
            _commandInput.textEdition.hidePlaceholderOnFocus = true;
            _commandInput.RegisterCallback<FocusInEvent>(_ => _commandInputContainer.AddToClassList(COMMAND_INPUT_FOCUSED_CLASS_NAME));
            _commandInput.RegisterCallback<FocusOutEvent>(_ => _commandInputContainer.RemoveFromClassList(COMMAND_INPUT_FOCUSED_CLASS_NAME));

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
                LogType.Assert => new AssertionLog(condition),
                _ => new WarningLog(condition),
            };

            if (_lastLogMessage == null || _lastLogMessage.LogRecord != logRecord)
            {
                _lastLogMessage = new ConsoleLogMessage(logRecord);
                _messageLogRecords.Add(_lastLogMessage);
                _lastLogMessage.SetEnabled(IsVisibleByFilters(_lastLogMessage));
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
                consoleLogMessage.SetEnabled(IsVisibleByFilters(consoleLogMessage));
        }

        private bool IsVisibleByFilters(ConsoleLogMessage message)
        {
            var isVisible = message.IsLog && _showLogsTgl.value;
            isVisible |= message.IsWarning && _showWarningsTgl.value;
            isVisible |= message.IsError && _showErrorsTgl.value;
            isVisible |= message.IsAssertion && _showAssertionsTgl.value;
            return isVisible;
        }

        /// <summary>
        /// Removes all log records (and with them their repeat counters) from this console. Unity's own console is not affected
        /// </summary>
        private void ClearLogs()
        {
            _messageLogRecords.Clear();
            _logScrollView.contentContainer.Clear();
            _lastLogMessage = null;
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
            _root.Remove(_commandInputContainer);
            _root.Add(_commandInputContainer);
        }
        
        public override void ConstructUI(VisualElement root)
        {
            _root = root;
            
            _styleSheet ??= Resources.Load<StyleSheet>("ConsoleModule");
            if (_styleSheet != null)
                root.styleSheets.Add(_styleSheet);
            else
                Debug.LogError("[ModularConsole] Failed to load ConsoleModule stylesheet from Resources!");
            
            root.Add(_controlPanel);
            root.Add(_logScrollView);
            root.Add(_commandInputContainer);
        }
    }
}