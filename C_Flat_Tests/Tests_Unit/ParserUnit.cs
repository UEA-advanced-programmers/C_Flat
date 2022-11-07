using System.Data;
using C_Flat_Interpreter.Common;
using C_Flat_Interpreter.Common.Enums;
using C_Flat_Interpreter.Parser;
using NUnit.Framework;

namespace C_Flat_Tests.Tests_Unit;

public class ParserUnit
{
/*
    [Test]
    public void Parser_Parse_DoubleMulti_ThrowsException()
    {
        List<Token> tokens = new List<Token>
        {
            new Token(TokenType.Num, 1),
            new Token(TokenType.Multi),
            new Token(TokenType.Multi),
            new Token(TokenType.Num, 2),
        };
        Parser parser = new Parser();

        Assert.Throws(Is.TypeOf<SyntaxErrorException>().And.Message.Contains("Number expected"),
        delegate { parser.Parse(tokens); });
    }
    
    [Test]
    public void Parser_Parse_NoRightParam_ThrowsException()
    {
        List<Token> tokens = new List<Token>
        {
            new Token(TokenType.Num, 1),
            new Token(TokenType.Multi),
            new Token(TokenType.LeftParen),
            new Token(TokenType.Num, 2),
        };
        Parser parser = new Parser();

        Assert.Throws(Is.TypeOf<SyntaxErrorException>().And.Message.Contains("Mismatched parentheses"),
            delegate { parser.Parse(tokens); });
    }
    
    [Test]
    public void Parser_Parse_NoLeftParam_ThrowsException()
    {
        List<Token> tokens = new List<Token>
        {
            new Token(TokenType.Num, 1),
            new Token(TokenType.Multi),
            new Token(TokenType.RightParen),
            new Token(TokenType.Num, 2),
        };
        Parser parser = new Parser();
        
        Assert.Throws(Is.TypeOf<SyntaxErrorException>().And.Message.Contains("Number expected"),
            delegate { parser.Parse(tokens); });
    }
    */
}