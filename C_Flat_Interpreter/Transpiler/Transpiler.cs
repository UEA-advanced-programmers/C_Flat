using C_Flat_Interpreter.Common;
using Microsoft.Extensions.Logging;

namespace C_Flat_Interpreter.Transpiler;

public class Transpiler : InterpreterLogger
{
    private readonly ILogger _logger;

    public Transpiler()
    {
        _logger = GetLogger("Transpiler");
    }

    public void Transpile(string input)
    {
        var writer = File.CreateText("../../../../C_Flat_Output/Program.cs");
        writer.Write(input);
        writer.Close();
    }
}