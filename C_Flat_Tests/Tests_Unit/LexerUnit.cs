using C_Flat_Interpreter.Common.Enums;
using C_Flat_Interpreter.Lexer;
using NUnit.Framework;

namespace C_Flat_Tests.Tests_Unit;

public class LexerUnit
{
    [Test]
    public void Lexer_Tokenise_AllTokens_16Tokens()
    {
        const string input = " +*()-/0123456789";
        Lexer lexer = new Lexer(input);
        lexer.Tokenise();
        Assert.That(lexer.GetTokens().Count, Is.EqualTo(input.Length - 1));
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
        Lexer lexer = new Lexer(input);
        lexer.Tokenise();
        Assert.That(lexer.GetFromTokenList(0).Type, Is.EqualTo(TokenType.Add));
        Assert.That(lexer.GetFromTokenList(0).Value, Is.EqualTo('+'));
    }
    
    [Test]
    public void Lexer_Tokenise_Multi_TokenIsMulti()
    {
        const string input = " +*()-/0123456789";
        Lexer lexer = new Lexer(input);
        lexer.Tokenise();
        Assert.That(lexer.GetFromTokenList(1).Type, Is.EqualTo(TokenType.Multi));
        Assert.That(lexer.GetFromTokenList(1).Value, Is.EqualTo('*'));
    }
    
    [Test]
    public void Lexer_Tokenise_LeftParam_TokenIsLeftParam()
    {
        const string input = " +*()-/0123456789";
        Lexer lexer = new Lexer(input);
        lexer.Tokenise();
        Assert.That(lexer.GetFromTokenList(2).Type, Is.EqualTo(TokenType.LeftParen));
        Assert.That(lexer.GetFromTokenList(2).Value, Is.EqualTo('('));
    }
    
    [Test]
    public void Lexer_Tokenise_RightParam_TokenIsRightParam()
    {
        const string input = " +*()-/0123456789";
        Lexer lexer = new Lexer(input);
        lexer.Tokenise();
        Assert.That(lexer.GetFromTokenList(3).Type, Is.EqualTo(TokenType.RightParen));
        Assert.That(lexer.GetFromTokenList(3).Value, Is.EqualTo(')'));
    }
    
    [Test]
    public void Lexer_Tokenise_Sub_TokenIsSub()
    {
        const string input = " +*()-/0123456789";
        Lexer lexer = new Lexer(input);
        lexer.Tokenise();
        Assert.That(lexer.GetFromTokenList(4).Type, Is.EqualTo(TokenType.Sub));
        Assert.That(lexer.GetFromTokenList(4).Value, Is.EqualTo('-'));
    }
    
    [Test]
    public void Lexer_Tokenise_Divide_TokenIsDivide()
    {
        const string input = " +*()-/0123456789";
        Lexer lexer = new Lexer(input);
        lexer.Tokenise();
        Assert.That(lexer.GetFromTokenList(5).Value, Is.EqualTo('/'));
        Assert.That(lexer.GetFromTokenList(5).Type, Is.EqualTo(TokenType.Divide));
    }
    
    [Test]
    public void Lexer_Tokenise_Number_TokenIsANumber()
    {
        const string input = " +*()-/0123456789";
        Lexer lexer = new Lexer(input);
        lexer.Tokenise();
        for (int i = 6; i < lexer.GetTokens().Count; i++)
        {
            Assert.That(lexer.GetFromTokenList(i).Value, Is.EqualTo(input[i + 1]));
            Assert.That(lexer.GetFromTokenList(i).Type, Is.EqualTo(TokenType.Num));
        }
    }
}