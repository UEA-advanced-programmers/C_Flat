using Microsoft.Extensions.Logging;

namespace C_Flat_Tests.Common;

public class TestLogger
{
    public ILogger GetLogger(string category)
    {
        using ILoggerFactory loggerFactory =
            LoggerFactory.Create(builder =>
                builder.AddSimpleConsole());
        return loggerFactory.CreateLogger(category);
    }
}