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
}