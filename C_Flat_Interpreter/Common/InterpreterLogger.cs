using Serilog;
using Serilog.Events;
using Serilog.Sinks.InMemory;

namespace C_Flat_Interpreter.Common;

public abstract class InterpreterLogger
{
    private InMemorySink memorySink;
    protected ILogger _logger;
    private string _category;
    protected void GetLogger(string category)
    {
        _category = category;
        _logger = new LoggerConfiguration().Destructure.ByTransforming<Token>(t => new {TokenType = t.Type, Word = t.Word, Value = t.Value}).WriteTo.Console(outputTemplate:
            $"[{{Timestamp:HH:mm:ss}} {{Level:u3}}] {category}: {{Message:lj}}{{NewLine}}{{Exception}}").WriteTo.InMemory().CreateLogger();
        memorySink = InMemorySink.Instance;
    }

    public IEnumerable<LogEvent> GetInMemoryLogs()
    {
        return memorySink.LogEvents;
    }

    public void ClearLogs()
    {
        memorySink.Dispose();
        GetLogger(_category);
    }
}