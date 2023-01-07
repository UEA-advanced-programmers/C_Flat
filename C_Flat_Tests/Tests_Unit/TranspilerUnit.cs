using C_Flat_Interpreter.Common;
using C_Flat_Interpreter.Common.Enums;
using C_Flat_Interpreter.Transpiler;
using C_Flat_Tests.Common;
using NUnit.Framework;

namespace C_Flat_Tests.Tests_Unit;

public class TranspilerUnit : TestLogger
{
    private readonly Transpiler _transpiler = new();
    private readonly List<ParseNode> _parseTree;
    public TranspilerUnit()
    {
        _parseTree = new();
        GetLogger("Execution Unit Tests");
    }

    [SetUp]
    public void Setup()
    {
        _parseTree.Clear();
    }

    [Test]
    public void Transpiler_Transpile_WritesStringToFile()
    {
        //Arrange
        var statement = new ParseNode(NodeType.Statement);
        var declaration = new ParseNode(NodeType.DeclareVariable);
        
        declaration.AddChild(new (NodeType.Terminal, new Token(TokenType.Word){Word = "var"}));
        
        var assignment = new ParseNode(NodeType.VarAssignment);
        var identifier = new ParseNode(NodeType.VarIdentifier);
        identifier.AddChild(new(NodeType.Terminal, new Token(TokenType.Word) {Word = " test"}));
        assignment.AddChild(identifier);
        
        assignment.AddChild(new (NodeType.Terminal, new Token(TokenType.Assignment){Word = "="}));
        
        var value = new ParseNode(NodeType.AssignmentValue);
        var valueString = new ParseNode(NodeType.String);
        valueString.AddChild(new(NodeType.Terminal, new Token(TokenType.String) {Word = "\"test\""}));
        value.AddChild(valueString);
        assignment.AddChild(value);
        assignment.AddChild(new (NodeType.Terminal, new Token(TokenType.SemiColon){Word = ";"}));
        declaration.AddChild(assignment);
        statement.AddChild(declaration);
        _parseTree.Add(statement);
        
        //Act
        var success = _transpiler.Transpile(_parseTree);
        var testOutput = File.ReadAllText(_transpiler.GetProgramPath());
        
        //Assert
        Assert.That(success, Is.EqualTo(0));
        Assert.That(testOutput, Contains.Substring("var test"));
    }

    [TearDown]
    public void TearDown()
    {
        //Recreate Program.cs with just a simple WriteLine to prevent build errors
        _transpiler.ResetOutput();
    }
}