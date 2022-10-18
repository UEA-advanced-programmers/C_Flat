using C_Flat_Interpreter.Lexer;
using NUnit.Framework;

namespace C_Flat_Tests.Tests_Unit;

public class Tests
{
    private bool _testShouldPass;
    private Lexer Lexer;
    [SetUp]
    public void Setup()
    {
        _testShouldPass = true;
        Lexer = new Lexer(true);
    }

    [Test]
    public void Test1()
    {
        Assert.That(_testShouldPass);
    }

    [Test]
    public void Lexer_Lexed_ReturnsTrue()
    {
        Assert.That(Lexer.Lexed);
    }
}