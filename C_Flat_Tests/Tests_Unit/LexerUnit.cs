using C_Flat_Interpreter.Common.Enums;
using C_Flat_Interpreter.Lexer;
using NUnit.Framework;
using Serilog.Events;

namespace C_Flat_Tests.Tests_Unit;

public class LexerUnit
{
    private readonly Lexer _lexer;
    public LexerUnit()
    {
        _lexer = new Lexer();
    }
    
    //Note - Magic numbers are used in these tests, however their disruption, if changed, is minimal - resulting in an
    //error in only the respective test. I had considered avoiding this by having one test and taking an iterative
    //approach. However, I decided that splitting this into multiple tests made it easier to identify what was failing
    //and why. Therefore, I believe that having multiple, clear tests with magic numbers is better than having one big
    //test without them.
    
    [Test]
    public void Lexer_Tokenise_Add_TokenIsAdd()
    {
        const string input = " +*()-/0123456789";
        _lexer.Lex(input);
        var token = _lexer.GetLine(0).Tokens[0];
        Assert.That(token.Type, Is.EqualTo(TokenType.Add));
        Assert.That(token.Word.ToString(), Is.EqualTo('+'.ToString()));
    }
    
    [Test]
    public void Lexer_Tokenise_Multi_TokenIsMulti()
    {
        const string input = " +*()-/0123456789";
        _lexer.Lex(input);
        Assert.That(_lexer.GetLine(0).Tokens[1].Type, Is.EqualTo(TokenType.Multi));
        Assert.That(_lexer.GetLine(0).Tokens[1].Word.ToString(), Is.EqualTo("*"));
    }
    
    [Test]
    public void Lexer_Tokenise_LeftParam_TokenIsLeftParam()
    {
        const string input = " +*()-/0123456789";
        //_lexer.Tokenise(input);
       // Assert.That(_lexer.GetFromTokenList(2).Type, Is.EqualTo(TokenType.LeftParen));
        //Assert.That(_lexer.GetFromTokenList(2).Word.ToString(), Is.EqualTo("("));
    }
    
 /*   [Test]
    public void Lexer_Tokenise_RightParam_TokenIsRightParam()
    {
        const string input = " +*()-/0123456789";
        _lexer.Tokenise(input);
        Assert.That(_lexer.GetFromTokenList(3).Type, Is.EqualTo(TokenType.RightParen));
        Assert.That(_lexer.GetFromTokenList(3).Word.ToString(), Is.EqualTo(")"));
    }
    
    [Test]
    public void Lexer_Tokenise_Sub_TokenIsSub()
    {
        const string input = " +*()-/0123456789";
        _lexer.Tokenise(input);
        Assert.That(_lexer.GetFromTokenList(4).Type, Is.EqualTo(TokenType.Sub));
        Assert.That(_lexer.GetFromTokenList(4).Word.ToString(), Is.EqualTo("-"));
    }
    
    [Test]
    public void Lexer_Tokenise_Divide_TokenIsDivide()
    {
        const string input = " +*()-/0123456789";
        _lexer.Tokenise(input);
        Assert.That(_lexer.GetFromTokenList(5).Word.ToString(), Is.EqualTo("/"));
        Assert.That(_lexer.GetFromTokenList(5).Type, Is.EqualTo(TokenType.Divide));
    }
    
    [Test]
    public void Lexer_Tokenise_MultiDigitNumber_TokenIsNumber()
    {
        const string input = "69";
        _lexer.Tokenise(input);
        Assert.That(_lexer.GetFromTokenList(0).Word.ToString(), Is.EqualTo("69"));
        Assert.That(_lexer.GetFromTokenList(0).Type, Is.EqualTo(TokenType.Num));
        Assert.That(_lexer.GetFromTokenList(0).Value, Is.EqualTo(69));
    }
    
    [Test]
    public void Lexer_Tokenise_RealNumber_IsDouble()
    {
        const string input = "69.420";
        _lexer.Tokenise(input);
        Assert.That(_lexer.GetFromTokenList(0).Word.ToString(), Is.EqualTo("69.420"));
        Assert.That(_lexer.GetFromTokenList(0).Type, Is.EqualTo(TokenType.Num));
        Assert.That(_lexer.GetFromTokenList(0).Value, Is.EqualTo(69.420));
    }
    
    [Test]
    public void Lexer_Tokenise_InvalidToken_IsHandled()
    {
        const string input = "5 ^ 3";
        _lexer.Tokenise(input);
        var errorLogs = _lexer.GetInMemoryLogs().Where(log => log.Level > LogEventLevel.Warning);
        Assert.That(_lexer.GetFromTokenList(0).Word.ToString(), Is.EqualTo("5"));
        Assert.That(errorLogs.Any(x => x.RenderMessage().Contains("Invalid lexeme encountered!")));
        Assert.That(_lexer.GetFromTokenList(1).Word.ToString(), Is.EqualTo("3"));
    }*/
}