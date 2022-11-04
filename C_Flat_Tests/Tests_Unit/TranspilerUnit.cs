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
        
        //Act
        _transpiler.Transpile(testInput);
        var testOutput = File.ReadAllLines(_transpiler.GetProgramPath());

        //Assert
        Assert.That(testInput, Is.EqualTo(testOutput[0]));
    }

    [TearDown]
    public void TearDown()
    {
        //Recreate Program.cs with just a simple WriteLine to prevent build errors
        _transpiler.ResetOutput();
    }
}