using C_Flat_Interpreter.Common;
using C_Flat_Interpreter.Common.Enums;
using C_Flat_Interpreter.Transpiler;
using C_Flat_Tests.Common;
using NUnit.Framework;

namespace C_Flat_Tests.Tests_Unit;

public class TranspilerUnit : TestLogger
{
    private readonly Transpiler _transpiler = new();
    public TranspilerUnit()
    { 
        GetLogger("Execution Unit Tests");
    }

    [Test]
    public void Transpiler_Transpile_WritesStringToFile()
    {
        //Arrange
        string testInput = @"Console.WriteLine(""Hello World!"");";
        List<Line> input = new List<Line>
        {
            new Line
            {
                LineNumber = 1,
                Tokens = new List<Token>
                {
                    new(TokenType.Num, 10),
                    new(TokenType.Add)
                    {
                        Word = "+"
                    },
                    new(TokenType.Num, 20),
                }
            }
        };
        //Act
        _transpiler.Transpile(input);
        var testOutput = File.ReadAllText(_transpiler.GetProgramPath());

        //Assert
        Assert.That(testOutput, Contains.Substring("10+20"));
    }
    [Test]
    public void Transpiler_Transpile_MultipleLinesToFile()
    {
        //Arrange
        List<Line> input = new List<Line>
        {
            new Line
            {
                LineNumber = 1,
                Tokens = new List<Token>
                {
                    new(TokenType.Num, 10),
                    new(TokenType.Add)
                    {
                        Word = "+"
                    },
                    new(TokenType.Num, 20),
                }
            },
            new Line
            {
                LineNumber = 2,
                Tokens = new List<Token>
                {
                    new(TokenType.Num, 10),
                    new(TokenType.Less)
                    {
                        Word = "<"
                    },
                    new(TokenType.Num, 20),
                }
            }
        };
        //Act
        _transpiler.Transpile(input);
        var testOutput = File.ReadLines(_transpiler.GetProgramPath()).ToList();

        //Assert
        Assert.That(testOutput.Count(), Is.EqualTo(2));
        Assert.That(testOutput[0], Contains.Substring("10+20"));
        Assert.That(testOutput[1], Contains.Substring("10<20"));

    }

    [TearDown]
    public void TearDown()
    {
        //Recreate Program.cs with just a simple WriteLine to prevent build errors
        _transpiler.ResetOutput();
    }
}