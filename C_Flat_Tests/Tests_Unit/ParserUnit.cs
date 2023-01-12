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
    public void Parser_Parse_VarAssignment_DeclareInvalidIntegerVariable_ReservedName_ReturnsFail()
    {
        //Log test
        _logger.Information("Testing Parse() with reserved name int declaration");
        //Arrange
        List<Token> tokens = new() { new Token(TokenType.Word, "var"), new Token(TokenType.Word, "Print"), new Token(TokenType.Assignment, "="), new Token(TokenType.Num, "1"), new Token(TokenType.SemiColon, ";") };
        //Act
        var returnStatus = _parser.Parse(tokens);
        //Assert
        Assert.That(returnStatus is 1);
    }

    [Test]
    public void Parser_Parse_VarAssignment_DeclareInvalidIntegerVariable_EmptyAssignment_ReturnsFail()
    {
        //Log test
        _logger.Information("Testing Parse() with blank assignment int declaration");
        //Arrange
        List<Token> tokens = new() { new Token(TokenType.Word, "var"), new Token(TokenType.Word, "x"), new Token(TokenType.Assignment, "="), new Token(TokenType.SemiColon, ";") };
        //Act
        var returnStatus = _parser.Parse(tokens);
        //Assert
        Assert.That(returnStatus is 1);
    }

    [Test]
    public void Parser_Parse_VarAssignment_DeclareInvalidIntegerVariable_NoVarDeclaration_ReturnsFail()
    {
        //Log test
        _logger.Information("Testing Parse() with no var declaration int declaration");
        //Arrange
        List<Token> tokens = new() { new Token(TokenType.Word, "x"), new Token(TokenType.Assignment, "="), new Token(TokenType.Num, "1"), new Token(TokenType.SemiColon, ";") };
        //Act
        var returnStatus = _parser.Parse(tokens);
        //Assert
        Assert.That(returnStatus is 1);
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
    public void Parser_Parse_ConditionalStatement_DeclareInvalidIfStatement_MissingParenthesis_ReturnsFail()
    {
        //Log test
        _logger.Information("Testing Parse() with invalid if statement missing parenthesis");
        //Arrange
        List<Token> tokens = new() { new Token(TokenType.Word, "if"), new Token(TokenType.Word, "true"), new Token(TokenType.LeftCurlyBrace, "{"), new Token(TokenType.RightCurlyBrace, "}") };
        //Act
        var returnStatus = _parser.Parse(tokens);
        //Assert
        Assert.That(returnStatus is 1);
    }

    [Test]
    public void Parser_Parse_ConditionalStatement_DeclareInvalidIfStatement_MissingCurlyBrace_ReturnsFail()
    {
        //Log test
        _logger.Information("Testing Parse() with invalid if statement missing curly braces");
        //Arrange
        List<Token> tokens = new() { new Token(TokenType.Word, "if"), new Token(TokenType.LeftParen, "("), new Token(TokenType.Word, "true"), new Token(TokenType.RightParen, ")") };
        //Act
        var returnStatus = _parser.Parse(tokens);
        //Assert
        Assert.That(returnStatus is 1);
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
    public void Parser_Parse_WhileStatement_DeclareInvalidWhileStatement_MissingCurlyBraces_ReturnsFail()
    {
        //Log test
        _logger.Information("Testing Parse() with invalid while statement missing curly braces");
        //Arrange
        List<Token> tokens = new() { new Token(TokenType.Word, "while"), new Token(TokenType.LeftParen, "("), new Token(TokenType.Word, "true"), new Token(TokenType.RightParen, ")") };
        //Act
        var returnStatus = _parser.Parse(tokens);
        //Assert
        Assert.That(returnStatus is 1);
    }

    [Test]
    public void Parser_Parse_WhileStatement_DeclareInvalidWhileStatement_MissingParenthesis_ReturnsFail()
    {
        //Log test
        _logger.Information("Testing Parse() with invalid while statement missing parenthesis");
        //Arrange
        List<Token> tokens = new() { new Token(TokenType.Word, "while"), new Token(TokenType.Word, "true"), new Token(TokenType.LeftCurlyBrace, "{"), new Token(TokenType.RightCurlyBrace, "}") };
        //Act
        var returnStatus = _parser.Parse(tokens);
        //Assert
        Assert.That(returnStatus is 1);
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

    // Functions

        //Print
       
    [Test]
    public void Parser_Parse_FunctionCall_DeclarePrintStatementWithString_ReturnsSuccess()
    {
        //Log test
        _logger.Information("Testing Parse() with print function using string");
        //Arrange
        List<Token> tokens = new() { new Token(TokenType.Word, "Print"), new Token(TokenType.LeftParen, "("), new Token(TokenType.String, "Hello World"), new Token(TokenType.RightParen, ")"), new Token(TokenType.SemiColon, ";") };
        //Act
        var returnStatus = _parser.Parse(tokens);
        //Assert
        Assert.That(returnStatus is 0);
    }

    [Test]
    public void Parser_Parse_FunctionCall_DeclarePrintStatementWithInt_ReturnsSuccess()
    {
        //Log test
        _logger.Information("Testing Parse() with print function using int");
        //Arrange
        List<Token> tokens = new() { new Token(TokenType.Word, "Print"), new Token(TokenType.LeftParen, "("), new Token(TokenType.Num, "7"), new Token(TokenType.RightParen, ")"), new Token(TokenType.SemiColon, ";") };
        //Act
        var returnStatus = _parser.Parse(tokens);
        //Assert
        Assert.That(returnStatus is 0);
    }

    [Test]
    public void Parser_Parse_FunctionCall_DeclarePrintStatementWithBool_ReturnsSuccess()
    {
        //Log test
        _logger.Information("Testing Parse() with print function using bool");
        //Arrange
        List<Token> tokens = new() { new Token(TokenType.Word, "Print"), new Token(TokenType.LeftParen, "("), new Token(TokenType.Word, "true"), new Token(TokenType.RightParen, ")"), new Token(TokenType.SemiColon, ";") };
        //Act
        var returnStatus = _parser.Parse(tokens);
        //Assert
        Assert.That(returnStatus is 0);
    }

    [Test]
    public void Parser_Parse_FunctionCall_DeclarePrintStatementWithVariableInt_ReturnsSuccess()
    {
        //Log test
        _logger.Information("Testing Parse() with print function using int variable");
        //Arrange
        List<Token> tokens = new() { new Token(TokenType.Word, "var"), new Token(TokenType.Word, "x"), new Token(TokenType.Assignment, "="), new Token(TokenType.Num, "1"), new Token(TokenType.SemiColon, ";"),
            new Token(TokenType.Word, "Print"), new Token(TokenType.LeftParen, "("), new Token(TokenType.Word, "x"), new Token(TokenType.RightParen, ")"), new Token(TokenType.SemiColon, ";") };
        //Act
        var returnStatus = _parser.Parse(tokens);
        //Assert
        Assert.That(returnStatus is 0);
    }

    [Test]
    public void Parser_Parse_FunctionCall_DeclarePrintStatementWithVariableString_ReturnsSuccess()
    {
        //Log test
        _logger.Information("Testing Parse() with print function using string variable");
        //Arrange
        List<Token> tokens = new() { new Token(TokenType.Word, "var"), new Token(TokenType.Word, "x"), new Token(TokenType.Assignment, "="), new Token(TokenType.String, "Hello World"), new Token(TokenType.SemiColon, ";"),
            new Token(TokenType.Word, "Print"), new Token(TokenType.LeftParen, "("), new Token(TokenType.Word, "x"), new Token(TokenType.RightParen, ")"), new Token(TokenType.SemiColon, ";") };
        //Act
        var returnStatus = _parser.Parse(tokens);
        //Assert
        Assert.That(returnStatus is 0);
    }

    [Test]
    public void Parser_Parse_FunctionCall_DeclarePrintStatementWithVariableBool_ReturnsSuccess()
    {
        //Log test
        _logger.Information("Testing Parse() with print function using bool variable");
        //Arrange
        List<Token> tokens = new() { new Token(TokenType.Word, "var"), new Token(TokenType.Word, "x"), new Token(TokenType.Assignment, "="), new Token(TokenType.Word, "true"), new Token(TokenType.SemiColon, ";"),
            new Token(TokenType.Word, "Print"), new Token(TokenType.LeftParen, "("), new Token(TokenType.Word, "x"), new Token(TokenType.RightParen, ")"), new Token(TokenType.SemiColon, ";") };
        //Act
        var returnStatus = _parser.Parse(tokens);
        //Assert
        Assert.That(returnStatus is 0);
    }

    [Test]
    public void Parser_Parse_FunctionCall_DeclarePrintStatementWithNullVariable_ReturnsFail()
    {
        //Log test
        _logger.Information("Testing Parse() with print function using null variable");
        //Arrange
        List<Token> tokens = new() { new Token(TokenType.Word, "var"), new Token(TokenType.Word, "x"), new Token(TokenType.SemiColon, ";"),
            new Token(TokenType.Word, "Print"), new Token(TokenType.LeftParen, "("), new Token(TokenType.Word, "x"), new Token(TokenType.RightParen, ")"), new Token(TokenType.SemiColon, ";") };
        //Act
        var returnStatus = _parser.Parse(tokens);
        //Assert
        Assert.That(returnStatus is 1);
    }

    [Test]
    public void Parser_Parse_FunctionCall_DeclareInvalidPrintStatementWithNullValue_ReturnsFail()
    {
        //Log test
        _logger.Information("Testing Parse() with invalid print function missing semicolon");
        //Arrange
        List<Token> tokens = new() { new Token(TokenType.Word, "Print"), new Token(TokenType.LeftParen, "("), new Token(TokenType.Null, ""), new Token(TokenType.RightParen, ")"), new Token(TokenType.SemiColon, ";") };
        //Act
        var returnStatus = _parser.Parse(tokens);
        //Assert
        Assert.That(returnStatus is 1);
    }

    [Test]
    public void Parser_Parse_FunctionCall_DeclareInvalidPrintStatementWithNoSemicolon_ReturnsFail()
    {
        //Log test
        _logger.Information("Testing Parse() with invalid print function missing semicolon");
        //Arrange
        List<Token> tokens = new() { new Token(TokenType.Word, "Print"), new Token(TokenType.LeftParen, "("), new Token(TokenType.String, "Hello World"), new Token(TokenType.RightParen, ")")};
        //Act
        var returnStatus = _parser.Parse(tokens);
        //Assert
        Assert.That(returnStatus is 1);
    }

    [Test]
    public void Parser_Parse_FunctionCall_DeclareInvalidPrintStatementWithNoParenthesis_ReturnsFail()
    {
        //Log test
        _logger.Information("Testing Parse() with invalid print function missing parenthesis");
        //Arrange
        List<Token> tokens = new() { new Token(TokenType.Word, "Print"), new Token(TokenType.String, "Hello World"), new Token(TokenType.SemiColon, ";") };
        //Act
        var returnStatus = _parser.Parse(tokens);
        //Assert
        Assert.That(returnStatus is 1);
    }

        //Concatenate

    [Test]
    public void Parser_Parse_FunctionCall_DeclareConcatenateStatementWithTwoStrings_ReturnsSuccess()
    {
        //Log test
        _logger.Information("Testing Parse() with concatenate function using 2 strings");
        //Arrange
        List<Token> tokens = new() { new Token(TokenType.Word, "Concatenate"), new Token(TokenType.LeftParen, "("), new Token(TokenType.String, "Hello"), new Token(TokenType.Comma, ","), new Token(TokenType.String, "World"), new Token(TokenType.RightParen, ")"), new Token(TokenType.SemiColon, ";") };
        //Act
        var returnStatus = _parser.Parse(tokens);
        //Assert
        Assert.That(returnStatus is 0);
    }

    [Test]
    public void Parser_Parse_FunctionCall_DeclareConcatenateStatementWithOneStringOneVariable_ReturnsSuccess()
    {
        //Log test
        _logger.Information("Testing Parse() with  concatenate function using 1 string 1 variable");
        //Arrange
        List<Token> tokens = new() { new Token(TokenType.Word, "var"), new Token(TokenType.Word, "x"), new Token(TokenType.Assignment, "="), new Token(TokenType.String, "Hello"), new Token(TokenType.SemiColon, ";"), 
            new Token(TokenType.Word, "Concatenate"), new Token(TokenType.LeftParen, "("), new Token(TokenType.Word, "x"), new Token(TokenType.Comma, ","), new Token(TokenType.String, "World"), new Token(TokenType.RightParen, ")"), new Token(TokenType.SemiColon, ";") };
        //Act
        var returnStatus = _parser.Parse(tokens);
        //Assert
        Assert.That(returnStatus is 0);
    }

    [Test]
    public void Parser_Parse_FunctionCall_DeclareConcatenateStatementWithTwoVariables_ReturnsSuccess()
    {
        //Log test
        _logger.Information("Testing Parse() with concatenate function using 2 variables");
        //Arrange
        List<Token> tokens = new() { new Token(TokenType.Word, "var"), new Token(TokenType.Word, "x"), new Token(TokenType.Assignment, "="), new Token(TokenType.String, "Hello"), new Token(TokenType.SemiColon, ";"),
            new Token(TokenType.Word, "var"), new Token(TokenType.Word, "y"), new Token(TokenType.Assignment, "="), new Token(TokenType.String, "World"), new Token(TokenType.SemiColon, ";"),
            new Token(TokenType.Word, "Concatenate"), new Token(TokenType.LeftParen, "("), new Token(TokenType.Word, "x"), new Token(TokenType.Comma, ","), new Token(TokenType.Word, "y"), new Token(TokenType.RightParen, ")"), new Token(TokenType.SemiColon, ";") };
        //Act
        var returnStatus = _parser.Parse(tokens);
        //Assert
        Assert.That(returnStatus is 0);
    }

    [Test]
    public void Parser_Parse_FunctionCall_DeclareConcatenateStatementWithTwoInts_ReturnsFail()
    {
        //Log test
        _logger.Information("Testing Parse() with concatenate function using 2 ints");
        //Arrange
        List<Token> tokens = new() { new Token(TokenType.Word, "Concatenate"), new Token(TokenType.LeftParen, "("), new Token(TokenType.Num, "1"), new Token(TokenType.Comma, ","), new Token(TokenType.Num, "2"), new Token(TokenType.RightParen, ")"), new Token(TokenType.SemiColon, ";") };
        //Act
        var returnStatus = _parser.Parse(tokens);
        //Assert
        Assert.That(returnStatus is 1);
    }

    //Stringify


    [Test]
    public void Parser_Parse_FunctionCall_DeclareStringifyStatementWithInt_ReturnsSuccess()
    {
        //Log test
        _logger.Information("Testing Parse() with stringify function using int");
        //Arrange
        List<Token> tokens = new() { new Token(TokenType.Word, "Stringify"), new Token(TokenType.LeftParen, "("), new Token(TokenType.Num, "1"), new Token(TokenType.RightParen, ")"), new Token(TokenType.SemiColon, ";") };
        //Act
        var returnStatus = _parser.Parse(tokens);
        //Assert
        Assert.That(returnStatus is 0);
    }

    [Test]
    public void Parser_Parse_FunctionCall_DeclareStringifyStatementWithBool_ReturnsSuccess()
    {
        //Log test
        _logger.Information("Testing Parse() with stringify function using Bool");
        //Arrange
        List<Token> tokens = new() { new Token(TokenType.Word, "Stringify"), new Token(TokenType.LeftParen, "("), new Token(TokenType.Word, "true"), new Token(TokenType.RightParen, ")"), new Token(TokenType.SemiColon, ";") };
        //Act
        var returnStatus = _parser.Parse(tokens);
        //Assert
        Assert.That(returnStatus is 0);
    }

    [Test]
    public void Parser_Parse_FunctionCall_DeclareStringifyStatementWithVariableInt_ReturnsSuccess()
    {
        //Log test
        _logger.Information("Testing Parse() with stringify function using int variable");
        //Arrange
        List<Token> tokens = new() { new Token(TokenType.Word, "var"), new Token(TokenType.Word, "x"), new Token(TokenType.Assignment, "="), new Token(TokenType.Num, "1"), new Token(TokenType.SemiColon, ";"),
           new Token(TokenType.Word, "Stringify"), new Token(TokenType.LeftParen, "("), new Token(TokenType.Word, "x"), new Token(TokenType.RightParen, ")"), new Token(TokenType.SemiColon, ";") };
        //Act
        var returnStatus = _parser.Parse(tokens);
        //Assert
        Assert.That(returnStatus is 0);
    }

    [Test]
    public void Parser_Parse_FunctionCall_DeclareStringifyStatementWithVariableIntAndEquation_ReturnsSuccess()
    {
        //Log test
        _logger.Information("Testing Parse() with stringify function using int variable and equation");
        //Arrange
        List<Token> tokens = new() { new Token(TokenType.Word, "var"), new Token(TokenType.Word, "x"), new Token(TokenType.Assignment, "="), new Token(TokenType.Num, "1"), new Token(TokenType.SemiColon, ";"),
           new Token(TokenType.Word, "Stringify"), new Token(TokenType.LeftParen, "("), new Token(TokenType.Word, "x"), new Token(TokenType.Add, "+"), new Token(TokenType.Num, "1"), new Token(TokenType.RightParen, ")"), new Token(TokenType.SemiColon, ";") };
        //Act
        var returnStatus = _parser.Parse(tokens);
        //Assert
        Assert.That(returnStatus is 0);
    }

    [Test]
    public void Parser_Parse_FunctionCall_DeclareStringifyStatementWithNullVariable_ReturnsFail()
    {
        //Log test
        _logger.Information("Testing Parse() with stringify function using null variable");
        //Arrange
        List<Token> tokens = new() { new Token(TokenType.Word, "var"), new Token(TokenType.Word, "x"), new Token(TokenType.SemiColon, ";"),
           new Token(TokenType.Word, "Stringify"), new Token(TokenType.LeftParen, "("), new Token(TokenType.Word, "x"), new Token(TokenType.RightParen, ")"), new Token(TokenType.SemiColon, ";") };
        //Act
        var returnStatus = _parser.Parse(tokens);
        //Assert
        Assert.That(returnStatus is 1);
    }

    [Test]
    public void Parser_Parse_FunctionCall_DeclareStringifyStatementWithNull_ReturnsFail()
    {
        //Log test
        _logger.Information("Testing Parse() with stringify function using null");
        //Arrange
        List<Token> tokens = new() {new Token(TokenType.Word, "Stringify"), new Token(TokenType.LeftParen, "("), new Token(TokenType.Null, ""), new Token(TokenType.RightParen, ")"), new Token(TokenType.SemiColon, ";") };
        //Act
        var returnStatus = _parser.Parse(tokens);
        //Assert
        Assert.That(returnStatus is 1);
    }


    // Root

    [Test]
    public void Parser_Parse_FunctionCall_DeclareRootStatementWithInt_ReturnsSuccess()
    {
        //Log test
        _logger.Information("Testing Parse() with root function using int");
        //Arrange
        List<Token> tokens = new() { new Token(TokenType.Word, "Root"), new Token(TokenType.LeftParen, "("), new Token(TokenType.Num, "1"), new Token(TokenType.RightParen, ")"), new Token(TokenType.SemiColon, ";") };
        //Act
        var returnStatus = _parser.Parse(tokens);
        //Assert
        Assert.That(returnStatus is 0);
    }

    [Test]
    public void Parser_Parse_FunctionCall_DeclareRootStatementWithIntVariable_ReturnsSuccess()
    {
        //Log test
        _logger.Information("Testing Parse() with root function using int variable");
        //Arrange
        List<Token> tokens = new() { new Token(TokenType.Word, "var"), new Token(TokenType.Word, "x"), new Token(TokenType.Assignment, "="), new Token(TokenType.Num, "1"), new Token(TokenType.SemiColon, ";"),
           new Token(TokenType.Word, "Root"), new Token(TokenType.LeftParen, "("), new Token(TokenType.Word, "x"), new Token(TokenType.RightParen, ")"), new Token(TokenType.SemiColon, ";") };
        //Act
        var returnStatus = _parser.Parse(tokens);
        //Assert
        Assert.That(returnStatus is 0);
    }

    public void Parser_Parse_FunctionCall_DeclareRootStatementWithIntVariableInEquation_ReturnsSuccess()
    {
        //Log test
        _logger.Information("Testing Parse() with root function using int variable");
        //Arrange
        List<Token> tokens = new() { new Token(TokenType.Word, "var"), new Token(TokenType.Word, "x"), new Token(TokenType.Assignment, "="), new Token(TokenType.Num, "1"), new Token(TokenType.SemiColon, ";"),
           new Token(TokenType.Word, "Root"), new Token(TokenType.LeftParen, "("), new Token(TokenType.Word, "x"), new Token(TokenType.Add, "+"), new Token(TokenType.Num, "2"), new Token(TokenType.RightParen, ")"), new Token(TokenType.SemiColon, ";") };
        //Act
        var returnStatus = _parser.Parse(tokens);
        //Assert
        Assert.That(returnStatus is 0);
    }

    [Test]
    public void Parser_Parse_FunctionCall_DeclareRootStatementWithNullVariable_ReturnsFail()
    {
        //Log test
        _logger.Information("Testing Parse() with root function using null variable");
        //Arrange
        List<Token> tokens = new() { new Token(TokenType.Word, "var"), new Token(TokenType.Word, "x"),new Token(TokenType.SemiColon, ";"),
           new Token(TokenType.Word, "Root"), new Token(TokenType.LeftParen, "("), new Token(TokenType.Word, "x"), new Token(TokenType.RightParen, ")"), new Token(TokenType.SemiColon, ";") };
        //Act
        var returnStatus = _parser.Parse(tokens);
        //Assert
        Assert.That(returnStatus is 1);
    }

    [Test]
    public void Parser_Parse_FunctionCall_DeclareRootStatementWithNull_ReturnsFail()
    {
        //Log test
        _logger.Information("Testing Parse() with root function using null");
        //Arrange
        List<Token> tokens = new() { new Token(TokenType.Word, "Root"), new Token(TokenType.LeftParen, "("), new Token(TokenType.Null,""), new Token(TokenType.RightParen, ")"), new Token(TokenType.SemiColon, ";") };
        //Act
        var returnStatus = _parser.Parse(tokens);
        //Assert
        Assert.That(returnStatus is 1);
    }

    // Power

    [Test]
    public void Parser_Parse_FunctionCall_DeclarePowerStatementWithTwoInts_ReturnsSuccess()
    {
        //Log test
        _logger.Information("Testing Parse() with power function using 2 ints");
        //Arrange
        List<Token> tokens = new() { new Token(TokenType.Word, "Power"), new Token(TokenType.LeftParen, "("), new Token(TokenType.Num, "1"), new Token(TokenType.Comma, ","), new Token(TokenType.Num, "1"), new Token(TokenType.RightParen, ")"), new Token(TokenType.SemiColon, ";") };
        //Act
        var returnStatus = _parser.Parse(tokens);
        //Assert
        Assert.That(returnStatus is 0);
    }

    [Test]
    public void Parser_Parse_FunctionCall_DeclarePowerStatementWithOneIntOneVariable_ReturnsSuccess()
    {
        //Log test
        _logger.Information("Testing Parse() with power function using 1 int 1 variable");
        //Arrange
        List<Token> tokens = new() { new Token(TokenType.Word, "var"), new Token(TokenType.Word, "x"), new Token(TokenType.Assignment, "="), new Token(TokenType.Num, "1"), new Token(TokenType.SemiColon, ";"),
           new Token(TokenType.Word, "Power"), new Token(TokenType.LeftParen, "("), new Token(TokenType.Word, "x"), new Token(TokenType.Comma, ","), new Token(TokenType.Num, "1"), new Token(TokenType.RightParen, ")"), new Token(TokenType.SemiColon, ";") };
        //Act
        var returnStatus = _parser.Parse(tokens);
        //Assert
        Assert.That(returnStatus is 0);
    }

    [Test]
    public void Parser_Parse_FunctionCall_DeclarePowerStatementWithTwoVariables_ReturnsSuccess()
    {
        //Log test
        _logger.Information("Testing Parse() with power function using 2 variables");
        //Arrange
        List<Token> tokens = new() { new Token(TokenType.Word, "var"), new Token(TokenType.Word, "y"), new Token(TokenType.Assignment, "="), new Token(TokenType.Num, "1"), new Token(TokenType.SemiColon, ";"),
           new Token(TokenType.Word, "var"), new Token(TokenType.Word, "x"), new Token(TokenType.Assignment, "="), new Token(TokenType.Num, "1"), new Token(TokenType.SemiColon, ";"),
           new Token(TokenType.Word, "Power"), new Token(TokenType.LeftParen, "("), new Token(TokenType.Word, "x"), new Token(TokenType.Comma, ","), new Token(TokenType.Word, "y"), new Token(TokenType.RightParen, ")"), new Token(TokenType.SemiColon, ";") };
        //Act
        var returnStatus = _parser.Parse(tokens);
        //Assert
        Assert.That(returnStatus is 0);
    }

    [Test]
    public void Parser_Parse_FunctionCall_DeclarePowerStatementWithOneIntOneNullVariable_ReturnsFail()
    {
        //Log test
        _logger.Information("Testing Parse() with power function using 1 int 1 null variable");
        //Arrange
        List<Token> tokens = new() { new Token(TokenType.Word, "var"), new Token(TokenType.Word, "x"), new Token(TokenType.SemiColon, ";"),
           new Token(TokenType.Word, "Power"), new Token(TokenType.LeftParen, "("), new Token(TokenType.Word, "x"), new Token(TokenType.Comma, ","), new Token(TokenType.Num, "1"), new Token(TokenType.RightParen, ")"), new Token(TokenType.SemiColon, ";") };
        //Act
        var returnStatus = _parser.Parse(tokens);
        //Assert
        Assert.That(returnStatus is 1);
    }

    [Test]
    public void Parser_Parse_FunctionCall_DeclarePowerStatementWithTwoNulls_ReturnsFail()
    {
        //Log test
        _logger.Information("Testing Parse() with power function using 2 nulls");
        //Arrange
        List<Token> tokens = new() { new Token(TokenType.Word, "Power"), new Token(TokenType.LeftParen, "("), new Token(TokenType.Null, ""), new Token(TokenType.Comma, ","), new Token(TokenType.Null, ""), new Token(TokenType.RightParen, ")"), new Token(TokenType.SemiColon, ";") };
        //Act
        var returnStatus = _parser.Parse(tokens);
        //Assert
        Assert.That(returnStatus is 1);
    }

    // nested functions

    [Test]
    public void Parser_Parse_FunctionCall_DeclarePowerInRootInStringifyInConcatenateInPrint_ReturnsSuccess()
    {
        //Log test
        _logger.Information("Testing Parse() with power function using 2 nulls");
        //Arrange
        List<Token> tokens = new() {
            new Token(TokenType.Word, "Print"), new Token(TokenType.LeftParen, "("),
            new Token(TokenType.Word, "Concatenate"), new Token(TokenType.LeftParen, "("),new Token(TokenType.String, "Number: "), new Token(TokenType.Comma, ","),
            new Token(TokenType.Word, "Stringify"), new Token(TokenType.LeftParen, "("),
            new Token(TokenType.Word, "Root"), new Token(TokenType.LeftParen, "("),
            new Token(TokenType.Word, "Power"), new Token(TokenType.LeftParen, "("), new Token(TokenType.Num, "11"), new Token(TokenType.Comma, ","), new Token(TokenType.Num, "3"),
            new Token(TokenType.RightParen, ")"), new Token(TokenType.RightParen, ")"), new Token(TokenType.RightParen, ")"), new Token(TokenType.RightParen, ")"), new Token(TokenType.RightParen, ")"), new Token(TokenType.SemiColon, ";")};
        //Act
        var returnStatus = _parser.Parse(tokens);
        //Assert
        Assert.That(returnStatus is 0);
    }

}