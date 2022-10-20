using Microsoft.Extensions.Logging;

namespace C_Flat_Interpreter.Common;

public abstract class InterpreterLogger
{
    public ILogger GetLogger(string category)
    {
        using ILoggerFactory loggerFactory =
            LoggerFactory.Create(builder =>
                builder.AddSimpleConsole());
        return loggerFactory.CreateLogger(category);
    }
}