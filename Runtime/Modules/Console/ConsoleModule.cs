using UnityEngine;
using UnityEngine.UIElements;

namespace ModularConsole.Modules.Console
{
    public class ConsoleModule : IConsoleModule
    {
        private static StyleSheet _styleSheet;

        public string ModuleName => "Console";
        public bool NeedsUpdate { get; private set; }

        private readonly ScrollView _logScrollView;
        private readonly VisualElement _controlPanel;
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
                // NeedsUpdate = true;
                _updateCycles = 0;
                
                return;
            }

            _lastLogMessage.IncreaseAmount();
        }

        public void Update()
        {
            // var scrollOffset = _logScrollView.scrollOffset;
            // scrollOffset.y = _logScrollView.verticalScroller.highValue;
            // _logScrollView.scrollOffset = scrollOffset;
            // _updateCycles++;
            //
            // if (_updateCycles > 5)
            // {
            //     _updateCycles = 0;
            //     NeedsUpdate = false;
            // }
            //
            // _root.Remove(_controlPanel);
            // _root.Add(_controlPanel);
            // _root.Remove(_logScrollView);
            // _root.Add(_logScrollView);
        }
        
        public void ConstructUI(VisualElement root)
        {
            _root = root;
            
            _styleSheet ??= Resources.Load<StyleSheet>("ConsoleModule");
            root.styleSheets.Add(_styleSheet);
            
            root.Add(_controlPanel);
            root.Add(_logScrollView);
        }
    }
}