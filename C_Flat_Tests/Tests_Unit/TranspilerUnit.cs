using C_Flat_Interpreter.Common;
using C_Flat_Interpreter.Common.Enums;
using C_Flat_Interpreter.Transpiler;
using C_Flat_Tests.Common;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace C_Flat_Tests.Tests_Unit;

public class TranspilerUnit : TestLogger
{
    private readonly Transpiler _transpiler = new();
    private readonly ILogger _logger;
    public TranspilerUnit()
    {
        _logger = GetLogger("Execution Unit Tests");
    }

    [Test]
    public void Transpiler_Transpile_WritesStringToFile()
    {
        //Arrange
        string testInput = @"Console.WriteLine(""Hello World!"");";
        List<Token> input = new List<Token>
        {
            new(TokenType.Num, 10),
            new(TokenType.Add)
            {
                Word = "+"
            },
            new(TokenType.Num, 20),
        };
        //Act
        _transpiler.Transpile(input);
        var testOutput = File.ReadAllText(_transpiler.GetProgramPath());

        //Assert
        Assert.That(testOutput, Contains.Substring("10+20"));
    }

    [TearDown]
    public void TearDown()
    {
        //Recreate Program.cs with just a simple WriteLine to prevent build errors
        _transpiler.ResetOutput();
    }
}