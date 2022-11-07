using C_Flat_Interpreter.Common;
using C_Flat_Interpreter.Common.Enums;
using C_Flat_Interpreter.Execution;
using C_Flat_Tests.Common;
using NUnit.Framework;

namespace C_Flat_Tests.Tests_Unit;

public class ExecutionUnit : TestLogger
{
    private readonly Execution _execution = new();

    public ExecutionUnit()
    {
        GetLogger("Execution Unit Tests");
    }

    [Test]
    public void Execution_ShuntYard_SimpleAddition_ReturnsCorrectAnswer()
    {
        _logger.Information("Testing simple addition: 1 + 3 = 4\n");

        //Arrange
        _execution.Tokens = new()
        {
            //  1 + 3 = 4
            new Token(TokenType.Num, 1),
            new Token(TokenType.Add),
            new Token(TokenType.Num, 3),
        };

        //Act
        var result = _execution.ShuntYard();

        //Assert
        Assert.That(result, Is.EqualTo(4));
    }

    [Test]
    public void Execution_ShuntYard_Multiplication_ReturnsCorrectAnswer()
    {
        _logger.Information("Testing simple multiplication: 9 × 3 = 27\n");

        //Arrange
        _execution.Tokens = new()
        {
            //  9 × 3 = 27
            new Token(TokenType.Num, 9),
            new Token(TokenType.Multi),
            new Token(TokenType.Num, 3),
        };

        //Act
        var result = _execution.ShuntYard();

        //Assert
        Assert.That(result, Is.EqualTo(27));
    }

    [Test]
    public void Execution_ShuntYard_ParenthesesExpression_ReturnsCorrectAnswer()
    {
        _logger.Information("Testing parenthesised expression: (9 + 3) × (4 + 2) = 72\n");

        //Arrange
        _execution.Tokens = new()
        { 
            //  (9 + 3) × (4 + 2) = 72
            new Token(TokenType.LeftParen),
            new Token(TokenType.Num, 9),
            new Token(TokenType.Add),
            new Token(TokenType.Num, 3),
            new Token(TokenType.RightParen),

            new Token(TokenType.Multi),

            new Token(TokenType.LeftParen),
            new Token(TokenType.Num, 4),
            new Token(TokenType.Add),
            new Token(TokenType.Num, 2),
            new Token(TokenType.RightParen),
        };

        //Act
        var result = _execution.ShuntYard();

        //Assert
        Assert.That(result, Is.EqualTo(72));
    }

    [Test]
    public void Execution_ShuntYard_NegativeNumberExpressions_ReturnsCorrectAnswers()
    {
        _logger.Information("Testing negative expression: 8 + -3 = 5\n");

        //Arrange
        var negativeAddition = new List<Token>()
        { 
            //  8 + -3 = 5
            new Token(TokenType.Num, 8),
            new Token(TokenType.Add),
            new Token(TokenType.Num, -3),
        };

        var negativeMultiplication = new List<Token>()
        { 
            //  -5 × 3 = -15
            new Token(TokenType.Num, -5),
            new Token(TokenType.Multi),
            new Token(TokenType.Num, 3),
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
        _logger.Information("Testing simple subtraction: 3 - 1 = 2\n");

        //Arrange
        _execution.Tokens = new()
        {
            //  3 - 1 = 2
            new Token(TokenType.Num, 3),
            new Token(TokenType.Sub),
            new Token(TokenType.Num, 1),
        };

        //Act
        var result = _execution.ShuntYard();

        //Assert
        Assert.That(result, Is.EqualTo(2));
    }
    
    [Test] 
    public void Execution_ShuntYard_SimpleDivision_ReturnsCorrectAnswer()
    {
        _logger.Information("Testing simple division: 6 ÷ 2 = 3\n");

        //Arrange
        _execution.Tokens = new()
        {
            //  6 ÷ 2 = 3
            new Token(TokenType.Num, 6),
            new Token(TokenType.Divide),
            new Token(TokenType.Num, 2),
        };

        //Act
        var result = _execution.ShuntYard();

        //Assert
        Assert.That(result, Is.EqualTo(3));
    }
    
    [Test]
    public void Execution_ShuntYard_ComplexDivision_ReturnsWholeNumber()
    {
        _logger.Information("Testing complex division: 5 ÷ 2 = 2\n");

        //Arrange
        _execution.Tokens = new()
        {
            //  5 ÷ 2 = 2 (Integer division rounds down)
            new Token(TokenType.Num, 5),
            new Token(TokenType.Divide),
            new Token(TokenType.Num, 2),
        };

        //Act
        var result = _execution.ShuntYard();

        //Assert
        Assert.That(result, Is.EqualTo(2));
    }
    
    [Test]
    public void Execution_ShuntYard_ComplexExpression_ReturnsCorrectAnswer()
    {
        _logger.Information("Testing complex expression: (3 × 2) + 4 - 2 = 8 \n");

        //Arrange
        _execution.Tokens = new()
        {
            //  (3 × 2) + 4 - 2 = 8
            new Token(TokenType.LeftParen),
            new Token(TokenType.Num, 3),
            new Token(TokenType.Multi),
            new Token(TokenType.Num, 2),
            new Token(TokenType.RightParen),

            new Token(TokenType.Add),
            
            new Token(TokenType.Num, 4),
            new Token(TokenType.Sub),
            new Token(TokenType.Num, 2),
        };

        //Act
        var result = _execution.ShuntYard();

        //Assert
        Assert.That(result, Is.EqualTo(8));
    }
    
    [Test]
    public void Execution_ShuntYard_IncorrectExpression_ThrowsException()
    {
        _logger.Information("Testing exception handling:\n");
        
        
        //Arrange
        _execution.Tokens = new()
        {
            //  The below is an expression designed to trick the evaluate function into evaluating an operator which it can't
            //  (23 
            new Token(TokenType.LeftParen, 1),
            new Token(TokenType.Num, 2),
            new Token(TokenType.Num, 3),

        };

        //Act

        //Assert
        //The below ensures that the ShuntYard() method throws an ArgumentException. 
        //It is useful to explicitly define the type of exception expected to ensure the test fails if another exception is thrown
        Assert.Throws<ArgumentException>(() =>
        {
            _execution.ShuntYard();
        });
    }
}