using C_Flat_Interpreter.Execution;
using NUnit.Framework;

namespace C_Flat_Tests.Tests_Unit;

public class ExecutionUnit
{
    private Execution _execution;
    private List<Execution.Token> _tokens;
    [SetUp]
    public void Setup()
    {
        //generate token test data
        _tokens = new List<Execution.Token>
        {
            new (Execution.TokenType.NUMBER, 1),
            new (Execution.TokenType.PLUS),
            new (Execution.TokenType.NUMBER, 3)
        };
    }
    
    [Test]
    public void Execution_ShuntYard_Returns4()
    {
        _execution = new Execution
        {
            Tokens = _tokens.ToArray()
        };
        var result = _execution.ShuntYard();
        Assert.That(result, Is.EqualTo(4));
    }
}