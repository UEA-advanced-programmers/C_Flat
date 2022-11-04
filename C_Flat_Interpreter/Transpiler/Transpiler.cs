using C_Flat_Interpreter.Common;
using Microsoft.Extensions.Logging;

namespace C_Flat_Interpreter.Transpiler;

public class Transpiler : InterpreterLogger
{
    private readonly ILogger _logger;
    private readonly string[] _defaultProgramString =
    {
        "// See https://aka.ms/new-console-template for more information",
        @"Console.WriteLine(""Hello, World!"");",
    };

    public Transpiler()
    {
        _logger = GetLogger("Transpiler");
    }

    public void Transpile(List<Token> tokens)
    {
        //Retrieve program.cs file
        var writer = File.CreateText(GetProgramPath());
        string prog = $@"Console.Out.WriteLine(";
        foreach (var tok in tokens)
        {
            prog += (tok.Value ?? tok.Word);
        }
        prog += @");";
        writer.Write(prog);
        writer.Close();
    }

    public string GetProgramPath()
    {
        return Path.GetFullPath("../../../../C_Flat_Output/Program.cs");
    }

    public void ResetOutput()
    {
        //Writes the microsoft console application template to program.cs to prevent build errors.
        var writer = File.CreateText(GetProgramPath());
        foreach (var line in _defaultProgramString)
        {
            writer.WriteLine(line);
        }
        writer.Close();
    }
}