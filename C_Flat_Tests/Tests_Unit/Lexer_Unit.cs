using C_Flat_Interpreter.Common;
using C_Flat_Interpreter.Common.Enums;
using C_Flat_Interpreter.Lexer;
using NUnit.Framework;

namespace C_Flat_Tests.Tests_Unit;

public class LexerUnit
{
    private string _input;
    private Lexer _lexer;
    private string _tokens;
    private List<Token> _tokenList;
        
    [SetUp]
    public void Setup()
    {
        _input = "1+2";
        _lexer = new Lexer(_input);
        _lexer.Tokenise();
        _tokens = "";
        _tokenList = new List<Token>();
            
        for (int i = 0; i < _input.Length; i++)
        {
            var testToken = _lexer.GetFromTokenList(i);
                
            _tokens += testToken.Value;
            _tokenList.Add(testToken);
        }
    }

    [Test]
    public void LexerUnitTestOne()
    {
        Assert.AreEqual(_tokens, _input);
        //Assert.That(_tokens, Is.EqualTo(_input));
    }
        
    [Test]
    public void LexerUnitTestTwo()
    {
        Assert.That(_tokenList[0].Type, Is.EqualTo(TokenType.Num));
        Assert.That(_tokenList[1].Type, Is.EqualTo(TokenType.Add));
        Assert.That(_tokenList[2].Type, Is.EqualTo(TokenType.Num));
    }
}