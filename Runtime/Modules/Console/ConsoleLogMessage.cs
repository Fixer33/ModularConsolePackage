using System;
using UnityEngine.UIElements;

namespace ModularConsole.Modules.Console
{
    internal class ConsoleLogMessage : VisualElement
    {
        private const string EXPANDED_CLASS_NAME = "logRecordExpanded";
        private const string TIME_FORMAT = "HH:mm:ss";

        public LogRecordBase LogRecord => _logRecord;

        public bool IsLog { get; }
        public bool IsWarning { get; }
        public bool IsError { get; }
        public bool IsAssertion { get; }

        private readonly LogRecordBase _logRecord;
        private readonly Label _amountText;
        private readonly Label _timeText;
        private int _amount;
        private bool _isExpanded;

        internal ConsoleLogMessage(LogRecordBase logRecord)
        {
            _logRecord = logRecord;

            IsLog = logRecord is SimpleLog;
            IsWarning = logRecord is WarningLog;
            IsError = logRecord is ErrorLog;
            IsAssertion = logRecord is AssertionLog;

            AddToClassList("logRecord");
            if (IsWarning)
                AddToClassList("logRecord--warning");
            else if (IsError)
                AddToClassList("logRecord--error");
            else if (IsAssertion)
                AddToClassList("logRecord--assertion");

            var row = new VisualElement() { name = "log-record__row" };
            Add(row);

            row.Add(new VisualElement() { name = "log-record__dot", pickingMode = PickingMode.Ignore });

            Label text = new Label()
            {
                name = "log-record__main-text",
                text = _logRecord.Message
            };
            row.Add(text);

            _timeText = new Label()
            {
                name = "log-record__time",
                text = DateTime.Now.ToString(TIME_FORMAT)
            };
            row.Add(_timeText);

            _amount = 1;
            _amountText = new Label()
            {
                name = "log-record__same-count-text",
                text = ""
            };
            row.Add(_amountText);
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
            _timeText.text = DateTime.Now.ToString(TIME_FORMAT);
        }
    }
}
