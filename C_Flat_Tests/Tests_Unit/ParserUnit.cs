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
    public void Parser_Parse_Expression_RunsCorrectly()
    {
        List<Token> tokens =
            new List<Token>() { 
                new Token(TokenType.Num, 3),
                new Token(TokenType.Add),
                new Token(TokenType.Num, 4),
            };
        Parser parser = new Parser();

        Assert.That(parser.Parse(tokens) == 0);
    }

    [Test]
    public void Parser_Parse_LogicStatement_RunsCorrectly()
    {
        string statement = "true";
        Lexer lexer = new Lexer();
        lexer.Tokenise(statement);
        List<Token> tokens =
            new List<Token>() {
                new Token(TokenType.Num, 3),
                new Token(TokenType.Less),
                new Token(TokenType.Num, 4),
            };
        Parser parser = new Parser();
        parser.Parse(tokens);
        Assert.That(parser.Parse(tokens) == 0);
    }

    [Test]
    public void Parser_Parse_IfStatementEmpty_RunsCorrectly()
    {
        string statement = "if(true){}";
        Lexer lexer = new Lexer();
        lexer.Tokenise(statement);
        List<Token> tokens = lexer.GetTokens();
        Parser parser = new Parser();

        Assert.That(parser.Parse(tokens) == 0);
    }

    [Test]
    public void Parser_Parse_IfStatementWithExpression_RunsCorrectly()
    {
        string statement = "if(true){3+4}";
        Lexer lexer = new Lexer();
        lexer.Tokenise(statement);
        List<Token> tokens = lexer.GetTokens();
        Parser parser = new Parser();

        Assert.That(parser.Parse(tokens) == 0);
    }

    [Test]
    public void Parser_Parse_IfStatementWithLogic_RunsCorrectly()
    {
        string statement = "if(true){true}";
        Lexer lexer = new Lexer();
        lexer.Tokenise(statement);
        List<Token> tokens = lexer.GetTokens();
        Parser parser = new Parser();

        Assert.That(parser.Parse(tokens) == 0);
    }

    [Test]
    public void Parser_Parse_IfElseStatement_RunsCorrectly()
    {
        string statement = "if(true){}else{}";
        Lexer lexer = new Lexer();
        lexer.Tokenise(statement);
        List<Token> tokens = lexer.GetTokens();
        Parser parser = new Parser();

        Assert.That(parser.Parse(tokens) == 0);
    }
}