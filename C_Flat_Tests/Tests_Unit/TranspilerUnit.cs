using C_Flat_Interpreter.Common;
using C_Flat_Interpreter.Common.Enums;
using C_Flat_Interpreter.Transpiler;
using C_Flat_Tests.Common;
using NUnit.Framework;
using Serilog.Events;

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
    public void Transpiler_Transpile_EmptyInputTree_WritesNothingToFile()
    {
        //  Log
        _logger.Information("Testing Transpile() with empty input tree");
        
        //Arrange
        
        //Act
        var success = _transpiler.Transpile(_parseTree);
        var logs = _transpiler.GetInMemoryLogs();
        var testOutput = File.ReadAllText(_transpiler.GetProgramPath());
        //Assert
        Assert.Multiple(() =>
        {
            Assert.That(success, Is.EqualTo(0));
            Assert.That(testOutput, Is.EqualTo(string.Empty));
            Assert.That(logs.Any(log => log.Level is LogEventLevel.Warning), Is.True);
        });
    }
    
    [Test]
    public void Transpiler_Transpile_InvalidTree_ReturnsFail()
    {
        //  Log
        _logger.Information("Testing Transpile() with invalid input tree");
        
        //Arrange
        _parseTree.Add(new ParseNode(NodeType.Expression));
        
        //Act
        var success = _transpiler.Transpile(_parseTree);
        var logs = _transpiler.GetInMemoryLogs();
        //Assert
        Assert.Multiple(() =>
        {
            Assert.That(success, Is.EqualTo(1));
            Assert.That(logs.Any(log => log.Level is LogEventLevel.Error), Is.True);
        });
    }
    
    [Test]
    public void Transpiler_Transpile_UnusedDeclaration_SucceedsWithWarning()
    {
        //  Log
        _logger.Information("Testing Transpile() with a variable declaration which is never used");
        
        //Arrange
        var statement = new ParseNode(NodeType.Statement);
        var variableIdentifier = "testVariable";
        var declarationNode = new ParseNode(NodeType.VariableDeclaration);
        declarationNode.AddChild(new ParseNode(NodeType.Terminal, new Token(TokenType.Word, "var")));
        
        var identifierNode = new ParseNode(NodeType.VariableIdentifier);
        identifierNode.AddChild(new ParseNode(NodeType.Terminal, new Token(TokenType.Word, variableIdentifier)));
        
        declarationNode.AddChild(identifierNode);
        declarationNode.AddChild(new ParseNode(NodeType.Terminal, new Token(TokenType.SemiColon, ";")));
        
        statement.AddChild(declarationNode);
        _parseTree.Add(statement);
        
        //  We need to add the new variable to the look up table
        VariableTable.Add(variableIdentifier);
        
        //Act
        var success = _transpiler.Transpile(_parseTree);
        var logs = _transpiler.GetInMemoryLogs();
        var testOutput = File.ReadAllText(_transpiler.GetProgramPath());
        //Assert
        Assert.Multiple(() =>
        {
            Assert.That(success, Is.EqualTo(0));
            Assert.That(logs.Any(log => log.Level is LogEventLevel.Warning), Is.True);
            Assert.That(testOutput, Is.EqualTo(string.Empty));
        });
    }
    
    [Test]
    public void Transpiler_Transpile_VariableDeclarationWithNoTableEntry_Fails()
    {
        //  Log
        _logger.Information("Testing Transpile() with a variable declaration which has no table entry");
        
        //Arrange
        var statement = new ParseNode(NodeType.Statement);
        var variableIdentifier = "testVariable";
        var declarationNode = new ParseNode(NodeType.VariableDeclaration);
        declarationNode.AddChild(new ParseNode(NodeType.Terminal, new Token(TokenType.Word, "var")));
        
        var identifierNode = new ParseNode(NodeType.VariableIdentifier);
        identifierNode.AddChild(new ParseNode(NodeType.Terminal, new Token(TokenType.Word, variableIdentifier)));
        
        declarationNode.AddChild(identifierNode);
        declarationNode.AddChild(new ParseNode(NodeType.Terminal, new Token(TokenType.SemiColon, ";")));
        
        statement.AddChild(declarationNode);
        _parseTree.Add(statement);

        //Act
        var success = _transpiler.Transpile(_parseTree);
        var logs = _transpiler.GetInMemoryLogs();
        var testOutput = File.ReadAllText(_transpiler.GetProgramPath());
        //Assert
        Assert.Multiple(() =>
        {
            Assert.That(success, Is.EqualTo(1));
            Assert.That(logs.Any(log => log.Level is LogEventLevel.Error), Is.True);
            Assert.That(testOutput, Is.EqualTo(string.Empty));
        });
    }
    
    [Test]
    public void Transpiler_Transpile_VariableDeclarationWithInvalidAssignmentType_Fails()
    {
        //  Log
        _logger.Information("Testing Transpile() with a variable declaration with an invalid assignment type");
        
        //Arrange
        var statement = new ParseNode(NodeType.Statement);
        var variableIdentifier = "testVariable";
        var declarationNode = new ParseNode(NodeType.VariableDeclaration);
        declarationNode.AddChild(new ParseNode(NodeType.Terminal, new Token(TokenType.Word, "var")));
        
        
        var identifierNode = new ParseNode(NodeType.VariableIdentifier);
        identifierNode.AddChild(new ParseNode(NodeType.Terminal, new Token(TokenType.Word, variableIdentifier)));
        declarationNode.AddChild(identifierNode);
        
        declarationNode.AddChild(new ParseNode(NodeType.Terminal, new Token(TokenType.SemiColon, ";")));

        statement.AddChild(declarationNode);
        _parseTree.Add(statement);
        
        //  Add a false variable type of terminal
        VariableTable.Add(variableIdentifier,  NodeType.Terminal);


        //Act
        var success = _transpiler.Transpile(_parseTree);
        var logs = _transpiler.GetInMemoryLogs();
        var testOutput = File.ReadAllText(_transpiler.GetProgramPath());
        //Assert
        Assert.Multiple(() =>
        {
            Assert.That(success, Is.EqualTo(1));
            Assert.That(logs.Any(log => log.Level is LogEventLevel.Error), Is.True);
            Assert.That(testOutput, Is.EqualTo(string.Empty));
        });
    }
    
    [Test]
    public void Transpiler_Transpile_VariableWithAmbiguousType_IsDetermined()
    {
        //  Log
        _logger.Information("Testing Transpile() with a variable declaration with an ambiguous type");
        
        //Arrange
        var statementOne = new ParseNode(NodeType.Statement);
        var variableIdentifier = "testVariable";
        var declarationNode = new ParseNode(NodeType.VariableDeclaration);
        declarationNode.AddChild(new ParseNode(NodeType.Terminal, new Token(TokenType.Word, "var")));

        var identifierNode = new ParseNode(NodeType.VariableIdentifier);
        identifierNode.AddChild(new ParseNode(NodeType.Terminal, new Token(TokenType.Word, $" {variableIdentifier}")));
        declarationNode.AddChild(identifierNode);
        
        declarationNode.AddChild(new ParseNode(NodeType.Terminal, new Token(TokenType.SemiColon, ";")));
        
        statementOne.AddChild(declarationNode);
        _parseTree.Add(statementOne);

        var statementTwo = new ParseNode(NodeType.Statement);

        var assignmentNode = new ParseNode(NodeType.VariableAssignment);
        
        assignmentNode.AddChild(identifierNode);
        assignmentNode.AddChild(new ParseNode(NodeType.Terminal, new Token(TokenType.Assignment, " =")));
        
        var assignmentValueNode = new ParseNode(NodeType.AssignmentValue);
        
        var stringNode = new ParseNode(NodeType.String);
        stringNode.AddChild(new ParseNode(NodeType.Terminal, new Token(TokenType.String, " \"Hello World!\"")));
        
        assignmentValueNode.AddChild(stringNode);

        assignmentNode.AddChild(assignmentValueNode);
        
        assignmentNode.AddChild(new ParseNode(NodeType.Terminal, new Token(TokenType.SemiColon, " ;")));
        
        statementTwo.AddChild(assignmentNode);
        _parseTree.Add(statementTwo);
        
        VariableTable.Add(variableIdentifier, NodeType.String);
        
        
        //Act
        var success = _transpiler.Transpile(_parseTree);
        var logs = _transpiler.GetInMemoryLogs();
        var testOutput = File.ReadAllText(_transpiler.GetProgramPath());
        
        //Assert
        Assert.Multiple(() =>
        {
            Assert.That(success, Is.EqualTo(0));
            Assert.That(logs.Any(log => log.Level > LogEventLevel.Information), Is.False);
            Assert.That(testOutput, Contains.Substring($"string {variableIdentifier};"));
        });
    }
    
    [TearDown]
    public void TearDown()
    {
        //Recreate Program.cs with just a simple WriteLine to prevent build errors
        _transpiler.ResetOutput();
        VariableTable.Clear();
    }
}