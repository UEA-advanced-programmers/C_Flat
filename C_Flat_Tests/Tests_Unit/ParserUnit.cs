using System.Data;
using C_Flat_Interpreter.Common;
using C_Flat_Interpreter.Common.Enums;
using C_Flat_Interpreter.Parser;
using C_Flat_Interpreter.Lexer;
using NUnit.Framework;
namespace C_Flat_Tests.Tests_Unit;

public class ParserUnit
{
    [Test]
    public void Parser_Parse_Expression()
    {
        string statement = "3+4;";
        Lexer lexer = new Lexer();
        lexer.Tokenise(statement);
        List<Token> tokens = lexer.GetTokens();
        Parser parser = new Parser();

        Assert.That(parser.Parse(tokens) == 0);
    }

    [Test]
    public void Parser_Parse_LogicStatement()
    {
        string statement = "true;";
        Lexer lexer = new Lexer();
        lexer.Tokenise(statement);
        List<Token> tokens = lexer.GetTokens();
        Parser parser = new Parser();
        parser.Parse(tokens);
        Assert.That(parser.Parse(tokens) == 0);
    }

    [Test]
    public void Parser_Parse_IfStatementEmpty()
    {
        string statement = "if(true){}";
        Lexer lexer = new Lexer();
        lexer.Tokenise(statement);
        List<Token> tokens = lexer.GetTokens();
        Parser parser = new Parser();

        Assert.That(parser.Parse(tokens) == 0);
    }

    [Test]
    public void Parser_Parse_IfStatementWithExpression()
    {
        string statement = "if(true){3+4;}";
        Lexer lexer = new Lexer();
        lexer.Tokenise(statement);
        List<Token> tokens = lexer.GetTokens();
        Parser parser = new Parser();

        Assert.That(parser.Parse(tokens) == 0);
    }

    [Test]
    public void Parser_Parse_IfStatementWithLogic()
    {
        string statement = "if(true){true;}";
        Lexer lexer = new Lexer();
        lexer.Tokenise(statement);
        List<Token> tokens = lexer.GetTokens();
        Parser parser = new Parser();

        Assert.That(parser.Parse(tokens) == 0);
    }

    [Test]
    public void Parser_Parse_IfElseStatement()
    {
        string statement = "if(true){}else{}";
        Lexer lexer = new Lexer();
        lexer.Tokenise(statement);
        List<Token> tokens = lexer.GetTokens();
        Parser parser = new Parser();

        Assert.That(parser.Parse(tokens) == 0);
    }
}