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
        _logger.LogInformation("Testing simple addition: 1 + 3 = 4\n");

        //Arrange
        _execution.Tokens = new[]
        {
            //  1 + 3 = 4
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
        _logger.LogInformation("Testing simple multiplication: 9 × 3 = 27\n");

        //Arrange
        _execution.Tokens = new[]
        {
            //  9 × 3 = 27
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
        _logger.LogInformation("Testing parenthesised expression: (9 + 3) × (4 + 2) = 72\n");

        //Arrange
        _execution.Tokens = new[]
        { 
            //  (9 + 3) × (4 + 2) = 72
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
        _logger.LogInformation("Testing negative expression: 8 + -3 = 5\n");

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
    
    [Test]
    public void Execution_ShuntYard_SimpleSubtraction_ReturnsCorrectAnswer()
    {
        _logger.LogInformation("Testing simple subtraction: 3 - 1 = 2\n");

        //Arrange
        _execution.Tokens = new[]
        {
            //  3 - 1 = 2
            new Execution.Token(Execution.TokenType.NUMBER, 3),
            new Execution.Token(Execution.TokenType.MINUS),
            new Execution.Token(Execution.TokenType.NUMBER, 1),
        };

        //Act
        var result = _execution.ShuntYard();

        //Assert
        Assert.That(result, Is.EqualTo(2));
    }
    
    [Test] 
    public void Execution_ShuntYard_SimpleDivision_ReturnsCorrectAnswer()
    {
        _logger.LogInformation("Testing simple division: 6 ÷ 2 = 3\n");

        //Arrange
        _execution.Tokens = new[]
        {
            //  6 ÷ 2 = 3
            new Execution.Token(Execution.TokenType.NUMBER, 6),
            new Execution.Token(Execution.TokenType.DIVIDE),
            new Execution.Token(Execution.TokenType.NUMBER, 2),
        };

        //Act
        var result = _execution.ShuntYard();

        //Assert
        Assert.That(result, Is.EqualTo(3));
    }
    
    [Test]
    public void Execution_ShuntYard_ComplexDivision_ReturnsWholeNumber()
    {
        _logger.LogInformation("Testing complex division: 5 ÷ 2 = 2\n");

        //Arrange
        _execution.Tokens = new[]
        {
            //  5 ÷ 2 = 2 (Integer division rounds down)
            new Execution.Token(Execution.TokenType.NUMBER, 5),
            new Execution.Token(Execution.TokenType.DIVIDE),
            new Execution.Token(Execution.TokenType.NUMBER, 2),
        };

        //Act
        var result = _execution.ShuntYard();

        //Assert
        Assert.That(result, Is.EqualTo(2));
    }
    
    [Test]
    public void Execution_ShuntYard_ComplexExpression_ReturnsCorrectAnswer()
    {
        _logger.LogInformation("Testing complex expression: (3 × 2) + 4 - 2 = 8 \n");

        //Arrange
        _execution.Tokens = new[]
        {
            //  (3 × 2) + 4 - 2 = 8
            new Execution.Token(Execution.TokenType.L_PAR),
            new Execution.Token(Execution.TokenType.NUMBER, 3),
            new Execution.Token(Execution.TokenType.STAR),
            new Execution.Token(Execution.TokenType.NUMBER, 2),
            new Execution.Token(Execution.TokenType.R_PAR),

            new Execution.Token(Execution.TokenType.PLUS),
            
            new Execution.Token(Execution.TokenType.NUMBER, 4),
            new Execution.Token(Execution.TokenType.MINUS),
            new Execution.Token(Execution.TokenType.NUMBER, 2),
        };

        //Act
        var result = _execution.ShuntYard();

        //Assert
        Assert.That(result, Is.EqualTo(8));
    }
}