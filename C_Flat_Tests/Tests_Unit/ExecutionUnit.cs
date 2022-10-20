using C_Flat_Interpreter.Execution;
using C_Flat_Tests.Common;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace C_Flat_Tests.Tests_Unit;

public class ExecutionUnit : TestLogger
{
    private readonly Execution _execution = new();
    private readonly ILogger _logger;

    public ExecutionUnit()
    {
        _logger = GetLogger("Execution Unit Tests");
    }

    [Test]
    public void Execution_ShuntYard_SimpleAddition_ReturnsCorrectAnswer()
    {
        _logger.LogInformation("Testing simple addition: \n");

        //Arrange
        _execution.Tokens = new[]
        {
            //  1+3 = 4
            new Execution.Token(Execution.TokenType.NUMBER, 1),
            new Execution.Token(Execution.TokenType.PLUS),
            new Execution.Token(Execution.TokenType.NUMBER, 3),
        };

        //Act
        var result = _execution.ShuntYard();

        //Assert
        Assert.That(result, Is.EqualTo(4));
    }

    [Test]
    public void Execution_ShuntYard_Multiplication_ReturnsCorrectAnswer()
    {
        _logger.LogInformation("Testing simple multiplication: \n");

        //Arrange
        _execution.Tokens = new[]
        {
            //  9×3 = 27
            new Execution.Token(Execution.TokenType.NUMBER, 9),
            new Execution.Token(Execution.TokenType.STAR),
            new Execution.Token(Execution.TokenType.NUMBER, 3),
        };

        //Act
        var result = _execution.ShuntYard();

        //Assert
        Assert.That(result, Is.EqualTo(27));
    }

    [Test]
    public void Execution_ShuntYard_ParenthesesExpression_ReturnsCorrectAnswer()
    {
        _logger.LogInformation("Testing parenthesised expression: \n");

        //Arrange
        _execution.Tokens = new[]
        { 
            //  (9+3) × (4+2) = 72
            new Execution.Token(Execution.TokenType.L_PAR),
            new Execution.Token(Execution.TokenType.NUMBER, 9),
            new Execution.Token(Execution.TokenType.PLUS),
            new Execution.Token(Execution.TokenType.NUMBER, 3),
            new Execution.Token(Execution.TokenType.R_PAR),

            new Execution.Token(Execution.TokenType.STAR),

            new Execution.Token(Execution.TokenType.L_PAR),
            new Execution.Token(Execution.TokenType.NUMBER, 4),
            new Execution.Token(Execution.TokenType.PLUS),
            new Execution.Token(Execution.TokenType.NUMBER, 2),
            new Execution.Token(Execution.TokenType.R_PAR),
        };

        //Act
        var result = _execution.ShuntYard();

        //Assert
        Assert.That(result, Is.EqualTo(72));
    }

    [Test]
    public void Execution_ShuntYard_NegativeNumberExpressions_ReturnsCorrectAnswers()
    {
        _logger.LogInformation("Testing parenthesised expression: \n");

        //Arrange
        var negativeAddition = new Execution.Token[]
        { 
            //  8 + -3 = 5
            new Execution.Token(Execution.TokenType.NUMBER, 8),
            new Execution.Token(Execution.TokenType.PLUS),
            new Execution.Token(Execution.TokenType.NUMBER, -3),
        };

        var negativeMultiplication = new Execution.Token[]
        { 
            //  -5 × 3 = -15
            new Execution.Token(Execution.TokenType.NUMBER, -5),
            new Execution.Token(Execution.TokenType.STAR),
            new Execution.Token(Execution.TokenType.NUMBER, 3),
        };

        //Act

        //  Perform negative addition
        _execution.Tokens = negativeAddition;
        var additionResult = _execution.ShuntYard();

        //  Perform negative multiplication
        _execution.Tokens = negativeMultiplication;
        var multiplicationResult = _execution.ShuntYard();

        //Assert
        Assert.That(additionResult, Is.EqualTo(5));
        Assert.That(multiplicationResult, Is.EqualTo(-15));

    }
}