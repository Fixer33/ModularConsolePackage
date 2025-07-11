using UnityEngine.UIElements;

namespace ModularConsole.Modules.Console
{
    internal class ConsoleLogMessage : VisualElement
    {
        private const string EXPANDED_CLASS_NAME = "logRecordExpanded";

        public LogRecordBase LogRecord => _logRecord;
        
        public bool IsLog { get; }
        public bool IsWarning { get; }
        public bool IsError { get; }

        private readonly LogRecordBase _logRecord;
        private readonly Label _amountText;
        private int _amount;
        private bool _isExpanded;
        
        internal ConsoleLogMessage(LogRecordBase logRecord)
        {
            _logRecord = logRecord;

            IsLog = logRecord is SimpleLog;
            IsWarning = logRecord is WarningLog;
            IsError = logRecord is ErrorLog;

            Label text = new Label()
            {
                name = "log-record__main-text",
                text = _logRecord.Message
            };
            Add(text);

            _amount = 1;
            _amountText = new Label()
            {
                name = "log-record__same-count-text",
                text = ""
            };
            Add(_amountText);
            _amountText.style.display = DisplayStyle.None;

            if (logRecord is ErrorLog errorLog)
            {
                Add(new Label()
                {
                    name = "log-record__stacktrace",
                    text = errorLog.StackTrace
                });
            }
            
            RegisterCallback<ClickEvent>(OnClick);
        }

        private void OnClick(ClickEvent evt)
        {
            _isExpanded = !_isExpanded;
            if (_isExpanded)
                AddToClassList(EXPANDED_CLASS_NAME);
            else
                RemoveFromClassList(EXPANDED_CLASS_NAME);
        }

        public void IncreaseAmount()
        {
            _amountText.style.display = DisplayStyle.Flex;
            _amount++;
            _amountText.text = _amount.ToString();
        }
    }
}