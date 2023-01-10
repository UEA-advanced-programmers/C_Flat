using C_Flat_Interpreter.Common;
using C_Flat_Interpreter.Common.Enums;
using C_Flat_Interpreter.Parser;
using C_Flat_Tests.Common;
using NUnit.Framework;

namespace C_Flat_Tests.Tests_Unit;

public class ParserUnit : TestLogger
{
    private readonly Parser _parser;

    public ParserUnit()
    {
        GetLogger("Parser Unit Tests");
        _parser = new();
    }

    [SetUp]
    public void SetupParserTests()
    {
        _parser.ClearLogs();
    }

    [Test]
    public void Parser_Parse_EmptyTokenList_ReturnsFail()
    {
        //Log test
        _logger.Information("Testing Parse() with empty token list");
        //Arrange
        List<Token> tokens = new();
        //Act
        var returnStatus = _parser.Parse(tokens);
        //Assert
        Assert.That(returnStatus is 1);
    }

    [Test]
    public void Parser_Parse_SyntaxError_ReturnsFail()
    {
        //Log test
        _logger.Information("Testing Parse() with Syntax error");
        //Arrange
        List<Token> tokens = new() {new Token(TokenType.Num, "-10")};
        //Act
        var returnStatus = _parser.Parse(tokens);
        //Assert
        Assert.That(returnStatus is 1);
    }

    // Variable Tests
 
        // Int tests

    [Test]
    public void Parser_Parse_VarAssignment_DeclareIntegerVariable_ReturnsSuccess()
    {
        //Log test
        _logger.Information("Testing Parse() with int declaration");
        //Arrange
        List<Token> tokens = new() { new Token(TokenType.Word, "var"), new Token(TokenType.Word, "x"), new Token(TokenType.Assignment, "="), new Token(TokenType.Num, "1"), new Token(TokenType.SemiColon, ";") };
        //Act
        var returnStatus = _parser.Parse(tokens);
        //Assert
        Assert.That(returnStatus is 0);
    }

    [Test]
    public void Parser_Parse_VarAssignment_DeclareEquationIntegerVariable_ReturnsSuccess()
    {
        //Log test
        _logger.Information("Testing Parse() with int equation declaration");
        //Arrange
        List<Token> tokens = new() { new Token(TokenType.Word, "var"), new Token(TokenType.Word, "x"), new Token(TokenType.Assignment, "="), new Token(TokenType.Num, "1"), new Token(TokenType.Add, "+"), new Token(TokenType.Num, "1"), new Token(TokenType.SemiColon, ";") };
        //Act
        var returnStatus = _parser.Parse(tokens);
        //Assert
        Assert.That(returnStatus is 0);
    }

    [Test]
    public void Parser_Parse_VarAssignment_DeclareIntegerVariableWithOtherVariable_ReturnsSuccess()
    {
        //Log test
        _logger.Information("Testing Parse() with int equation with other variable declaration");
        //Arrange
        List<Token> tokens = new() { new Token(TokenType.Word, "var"), new Token(TokenType.Word, "y"), new Token(TokenType.Assignment, "="), new Token(TokenType.Num, "1"), new Token(TokenType.SemiColon, ";"),
            new Token(TokenType.Word, "var"), new Token(TokenType.Word, "x"), new Token(TokenType.Assignment, "="), new Token(TokenType.Word, "y"), new Token(TokenType.Add, "+"), new Token(TokenType.Num, "1"), new Token(TokenType.SemiColon, ";") };
        //Act
        var returnStatus = _parser.Parse(tokens);
        //Assert
        Assert.That(returnStatus is 0);
    }

    //String tests

    [Test]
    public void Parser_Parse_VarAssignment_DeclareStringVariable_ReturnsSuccess()
    {
        //Log test
        _logger.Information("Testing Parse() with string declaration");
        //Arrange
        List<Token> tokens = new() { new Token(TokenType.Word, "var"), new Token(TokenType.Word, "x"), new Token(TokenType.Assignment, "="), new Token(TokenType.String, "Hello"), new Token(TokenType.SemiColon, ";") };
        //Act
        var returnStatus = _parser.Parse(tokens);
        //Assert
        Assert.That(returnStatus is 0);
    }

        //Bool tests

    [Test]
    public void Parser_Parse_VarAssignment_DeclareTrueBooleanVariable_ReturnsSuccess()
    {
        //Log test
        _logger.Information("Testing Parse() with true boolean declaration");
        //Arrange
        List<Token> tokens = new() { new Token(TokenType.Word, "var"), new Token(TokenType.Word, "x"), new Token(TokenType.Assignment, "="), new Token(TokenType.Word, "true"), new Token(TokenType.SemiColon, ";") };
        //Act
        var returnStatus = _parser.Parse(tokens);
        //Assert
        Assert.That(returnStatus is 0);
    }

    [Test]
    public void Parser_Parse_VarAssignment_DeclareFalseBooleanVariable_ReturnsSuccess()
    {
        //Log test
        _logger.Information("Testing Parse() with false boolean declaration");
        //Arrange
        List<Token> tokens = new() { new Token(TokenType.Word, "var"), new Token(TokenType.Word, "x"), new Token(TokenType.Assignment, "="), new Token(TokenType.Word, "false"), new Token(TokenType.SemiColon, ";") };
        //Act
        var returnStatus = _parser.Parse(tokens);
        //Assert
        Assert.That(returnStatus is 0);
    }

    [Test]
    public void Parser_Parse_VarAssignment_DeclareBooleanVariableWithEqualsEquation_ReturnsSuccess()
    {
        //Log test
        _logger.Information("Testing Parse() with equals boolean equation");
        //Arrange
        List<Token> tokens = new() { new Token(TokenType.Word, "var"), new Token(TokenType.Word, "x"), new Token(TokenType.Assignment, "="), new Token(TokenType.Num, "1"), new Token(TokenType.Equals, "=="), new Token(TokenType.Num, "1"), new Token(TokenType.SemiColon, ";") };
        //Act
        var returnStatus = _parser.Parse(tokens);
        //Assert
        Assert.That(returnStatus is 0);
    }

    [Test]
    public void Parser_Parse_VarAssignment_DeclareBooleanVariableWithLessThanEquation_ReturnsSuccess()
    {
        //Log test
        _logger.Information("Testing Parse() with less than boolean equation");
        //Arrange
        List<Token> tokens = new() { new Token(TokenType.Word, "var"), new Token(TokenType.Word, "x"), new Token(TokenType.Assignment, "="), new Token(TokenType.Num, "1"), new Token(TokenType.Less, "<"), new Token(TokenType.Num, "1"), new Token(TokenType.SemiColon, ";") };
        //Act
        var returnStatus = _parser.Parse(tokens);
        //Assert
        Assert.That(returnStatus is 0);
    }

    [Test]
    public void Parser_Parse_VarAssignment_DeclareBooleanVariableWitMoreThanEquation_ReturnsSuccess()
    {
        //Log test
        _logger.Information("Testing Parse() with more than boolean equation");
        //Arrange
        List<Token> tokens = new() { new Token(TokenType.Word, "var"), new Token(TokenType.Word, "x"), new Token(TokenType.Assignment, "="), new Token(TokenType.Num, "1"), new Token(TokenType.Less, ">"), new Token(TokenType.Num, "1"), new Token(TokenType.SemiColon, ";") };
        //Act
        var returnStatus = _parser.Parse(tokens);
        //Assert
        Assert.That(returnStatus is 0);
    }

    public void Parser_Parse_VarAssignment_DeclareBooleanVariableWitNotEqualEquation_ReturnsSuccess()
    {
        //Log test
        _logger.Information("Testing Parse() with not equal boolean equation");
        //Arrange
        List<Token> tokens = new() { new Token(TokenType.Word, "var"), new Token(TokenType.Word, "x"), new Token(TokenType.Assignment, "="), new Token(TokenType.Num, "1"), new Token(TokenType.NotEqual, "!="), new Token(TokenType.Num, "1"), new Token(TokenType.SemiColon, ";") };
        //Act
        var returnStatus = _parser.Parse(tokens);
        //Assert
        Assert.That(returnStatus is 0);
    }

    public void Parser_Parse_VarAssignment_DeclareBooleanVariableWitOtherVariableAndEquation_ReturnsSuccess()
    {
        //Log test
        _logger.Information("Testing Parse() with variable in boolean equation");
        //Arrange
        List<Token> tokens = new() { new Token(TokenType.Word, "var"), new Token(TokenType.Word, "y"), new Token(TokenType.Assignment, "="), new Token(TokenType.Num, "1"), new Token(TokenType.SemiColon, ";"), 
            new Token(TokenType.Word, "var"), new Token(TokenType.Word, "x"), new Token(TokenType.Assignment, "="), new Token(TokenType.Num, "1"), new Token(TokenType.NotEqual, "!="), new Token(TokenType.Word, "y"), new Token(TokenType.SemiColon, ";") };
        //Act
        var returnStatus = _parser.Parse(tokens);
        //Assert
        Assert.That(returnStatus is 0);
    }

    [Test]
    public void Parser_Parse_VarAssignment_DeclareBooleanVariableWithOtherBooleanVariable_ReturnsSuccess()
    {
        //Log test
        _logger.Information("Testing Parse() with other variable");
        //Arrange
        List<Token> tokens = new() { new Token(TokenType.Word, "var"), new Token(TokenType.Word, "y"), new Token(TokenType.Assignment, "="), new Token(TokenType.Word, "false"),new Token(TokenType.SemiColon, ";"),
            new Token(TokenType.Word, "var"), new Token(TokenType.Word, "x"), new Token(TokenType.Assignment, "="), new Token(TokenType.Word, "y"), new Token(TokenType.SemiColon, ";") };
        //Act
        var returnStatus = _parser.Parse(tokens);
        //Assert
        Assert.That(returnStatus is 0);
    }

    // Blank Variable tests

    [Test]
    public void Parser_Parse_VarAssignment_DeclareEmptyVariable_ReturnsSuccess()
    {
        //Log test
        _logger.Information("Testing Parse() with blank declaration");
        //Arrange
        List<Token> tokens = new() { new Token(TokenType.Word, "var"), new Token(TokenType.Word, "x"), new Token(TokenType.SemiColon, ";") };
        //Act
        var returnStatus = _parser.Parse(tokens);
        //Assert
        Assert.That(returnStatus is 0);
    }

    // If statements

    [Test]
    public void Parser_Parse_ConditionalStatement_DeclareIfStatement_ReturnsSuccess()
    {
        //Log test
        _logger.Information("Testing Parse() with if statement");
        //Arrange
        List<Token> tokens = new() { new Token(TokenType.Word, "if"), new Token(TokenType.LeftParen, "("), new Token(TokenType.Word, "true"), new Token(TokenType.RightParen, ")"), new Token(TokenType.LeftCurlyBrace, "{"), new Token(TokenType.RightCurlyBrace, "}") };
        //Act
        var returnStatus = _parser.Parse(tokens);
        //Assert
        Assert.That(returnStatus is 0);
    }

    [Test]
    public void Parser_Parse_ConditionalStatement_DeclareIfStatementWithBooleanEquation_ReturnsSuccess()
    {
        //Log test
        _logger.Information("Testing Parse() with if statement with equation ");
        //Arrange
        List<Token> tokens = new() { new Token(TokenType.Word, "if"), new Token(TokenType.LeftParen, "("), new Token(TokenType.Num, "1"), new Token(TokenType.Equals, "=="), new Token(TokenType.Num, "1"), new Token(TokenType.RightParen, ")"), new Token(TokenType.LeftCurlyBrace, "{"), new Token(TokenType.RightCurlyBrace, "}") };
        //Act
        var returnStatus = _parser.Parse(tokens);
        //Assert
        Assert.That(returnStatus is 0);
    }

    [Test]
    public void Parser_Parse_ConditionalStatement_DeclareIfElseStatement_ReturnsSuccess()
    {
        //Log test
        _logger.Information("Testing Parse() with if else statement");
        //Arrange
        List<Token> tokens = new() { new Token(TokenType.Word, "if"), new Token(TokenType.LeftParen, "("), new Token(TokenType.Word, "true"), new Token(TokenType.RightParen, ")"), new Token(TokenType.LeftCurlyBrace, "{"), new Token(TokenType.RightCurlyBrace, "}"), new Token(TokenType.Word, "else"), new Token(TokenType.LeftCurlyBrace, "{"), new Token(TokenType.RightCurlyBrace, "}") };
        //Act
        var returnStatus = _parser.Parse(tokens);
        //Assert
        Assert.That(returnStatus is 0);
    }

    [Test]
    public void Parser_Parse_ConditionalStatement_DeclareIfStatementWithVariables_ReturnsSuccess()
    {
        //Log test
        _logger.Information("Testing Parse() with if statement using variables");
        //Arrange
        List<Token> tokens = new() { new Token(TokenType.Word, "var"), new Token(TokenType.Word, "x"), new Token(TokenType.Assignment, "="), new Token(TokenType.Word, "false"), new Token(TokenType.SemiColon, ";"), new Token(TokenType.Word, "if"), new Token(TokenType.LeftParen, "("), new Token(TokenType.Word, "x"), new Token(TokenType.RightParen, ")"), new Token(TokenType.LeftCurlyBrace, "{"), new Token(TokenType.RightCurlyBrace, "}") };
        //Act
        var returnStatus = _parser.Parse(tokens);
        //Assert
        Assert.That(returnStatus is 0);
    }

    // While statements

    [Test]
    public void Parser_Parse_WhileStatement_DeclareWhileStatement_ReturnsSuccess()
    {
        //Log test
        _logger.Information("Testing Parse() with while statement");
        //Arrange
        List<Token> tokens = new() { new Token(TokenType.Word, "while"), new Token(TokenType.LeftParen, "("), new Token(TokenType.Word, "true"), new Token(TokenType.RightParen, ")"), new Token(TokenType.LeftCurlyBrace, "{"), new Token(TokenType.RightCurlyBrace, "}") };
        //Act
        var returnStatus = _parser.Parse(tokens);
        //Assert
        Assert.That(returnStatus is 0);
    }

    [Test]
    public void Parser_Parse_WhileStatement_DeclareWhileStatementWithVariables_ReturnsSuccess()
    {
        //Log test
        _logger.Information("Testing Parse() with while statement using variables");
        //Arrange
        List<Token> tokens = new() { new Token(TokenType.Word, "var"), new Token(TokenType.Word, "x"), new Token(TokenType.Assignment, "="), new Token(TokenType.Word, "false"), new Token(TokenType.SemiColon, ";"), new Token(TokenType.Word, "while"), new Token(TokenType.LeftParen, "("), new Token(TokenType.Word, "x"), new Token(TokenType.RightParen, ")"), new Token(TokenType.LeftCurlyBrace, "{"), new Token(TokenType.RightCurlyBrace, "}") };
        //Act
        var returnStatus = _parser.Parse(tokens);
        //Assert
        Assert.That(returnStatus is 0);
    }

    [Test]
    public void Parser_Parse_WhileStatement_DeclareWhileStatementWithBooleanEquation_ReturnsSuccess()
    {
        //Log test
        _logger.Information("Testing Parse() with while statement using boolean equation");
        //Arrange
        List<Token> tokens = new() { new Token(TokenType.Word, "while"), new Token(TokenType.LeftParen, "("), new Token(TokenType.Num, "1"), new Token(TokenType.Equals, "=="), new Token(TokenType.Num, "1"), new Token(TokenType.RightParen, ")"), new Token(TokenType.LeftCurlyBrace, "{"), new Token(TokenType.RightCurlyBrace, "}") };
        //Act
        var returnStatus = _parser.Parse(tokens);
        //Assert
        Assert.That(returnStatus is 0);
    }
}