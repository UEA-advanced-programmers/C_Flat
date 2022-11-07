using Serilog;

namespace C_Flat_Tests.Common;

public class TestLogger
{
    protected ILogger _logger;
    private string _category;
    protected void GetLogger(string category)
    {
        _category = category;
        _logger = new LoggerConfiguration().WriteTo.Console(outputTemplate:
            $"[{{Timestamp:HH:mm:ss}} {{Level:u3}}] {category}: {{Message:lj}}{{NewLine}}{{Exception}}").CreateLogger();

    }
}