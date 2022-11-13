using System.Data;
using C_Flat_Interpreter.Common;
using C_Flat_Interpreter.Common.Enums;
using C_Flat_Interpreter.Parser;
using NUnit.Framework;

namespace C_Flat_Tests.Tests_Unit;

public class ParserUnit
{
    [Test]
    public void Parser_IfStatementEmpty()
    {
        List<Token> tokens = new List<Token>
        {
            new Token(TokenType.String, "if"),
            new Token(TokenType.LeftParen),
            new Token(TokenType.String, "true"),
            new Token(TokenType.RightParen),
            new Token(TokenType.LeftCurlyBrace),
            new Token(TokenType.RightCurlyBrace),
        };
        Parser parser = new Parser();

        Assert.That(parser.Parse(tokens) == 0);
    }
    [Test]
    public void Parser_IfStatementWithExpression()
    {
        List<Token> tokens = new List<Token>
        {
            new Token(TokenType.String, "if"),
            new Token(TokenType.LeftParen),
            new Token(TokenType.String, "true"),
            new Token(TokenType.RightParen),
            new Token(TokenType.LeftCurlyBrace),
            new Token(TokenType.Num, 3),
            new Token(TokenType.RightCurlyBrace),
        };
        Parser parser = new Parser();

        Assert.That(parser.Parse(tokens)==0);
    }
    [Test]
    public void Parser_IfStatementWithLogic()
    {
        List<Token> tokens = new List<Token>
        {
            new Token(TokenType.String, "if"),
            new Token(TokenType.LeftParen),
            new Token(TokenType.String, "true"),
            new Token(TokenType.RightParen),
            new Token(TokenType.LeftCurlyBrace),
            new Token(TokenType.Num, 3),
            new Token(TokenType.Equals),
            new Token(TokenType.Equals),
            new Token(TokenType.Num, 3),
            new Token(TokenType.RightCurlyBrace),
        };
        Parser parser = new Parser();

        Assert.That(parser.Parse(tokens) == 0);
    }
    [Test]
    public void Parser_IfElseStatement()
    {
        List<Token> tokens = new List<Token>
        {
            //if
            new Token(TokenType.String, "if"),
            new Token(TokenType.LeftParen),
            new Token(TokenType.String, "true"),
            new Token(TokenType.RightParen),
            new Token(TokenType.LeftCurlyBrace),
            new Token(TokenType.Num, 3),
            new Token(TokenType.RightCurlyBrace),
            //else
            new Token(TokenType.String, "if"),
            new Token(TokenType.LeftCurlyBrace),
            new Token(TokenType.Num, 7),
            new Token(TokenType.RightCurlyBrace),
        };
        Parser parser = new Parser();

        Assert.That(parser.Parse(tokens) == 0);
    }
}