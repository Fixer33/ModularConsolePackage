namespace ModularConsole.Modules.Console
{
    internal abstract record LogRecordBase(string Message)
    {
        public string Message { get; } = Message;
    }
    
    internal record SimpleLog(string Message) : LogRecordBase(Message) { }
    internal record WarningLog(string Message) : LogRecordBase(Message) {}

    internal record ErrorLog(string Message, string StackTrace) : LogRecordBase(Message)
    {
        public string StackTrace { get; } = StackTrace;
    }
}