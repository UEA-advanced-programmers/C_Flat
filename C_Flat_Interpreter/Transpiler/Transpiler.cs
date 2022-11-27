using System.Diagnostics.SymbolStore;
using C_Flat_Interpreter.Common;
using C_Flat_Interpreter.Common.Enums;
using Serilog;

namespace C_Flat_Interpreter.Transpiler;

public class Transpiler : InterpreterLogger
{
    private readonly string[] _defaultProgramString =
    {
        "// See https://aka.ms/new-console-template for more information",
        @"Console.WriteLine(""Hello, World!"");",
    };

    private int _currentLine;
    private string _program;
    public Transpiler()
    {
        GetLogger("Transpiler");
    }
    
    //Helper functions
    private void PrintTerminal(ParseNode node)
    {
        var tokenToPrint = node.token ?? 
                           throw new NullReferenceException("Node is marked as terminal but has no token");
        if (tokenToPrint.Line > _currentLine)
        {
            _currentLine = tokenToPrint.Line;
            _program += Environment.NewLine;
        }
        _program += tokenToPrint.Word;
    }


    public string Transpile(List<ParseNode> parseTree)
    {
        //Retrieve program.cs file
        _program = string.Empty;
        var writer = File.CreateText(GetProgramPath());
        foreach (var node in parseTree)
        {
            foreach (var statement in node.getChildren())
            {
                switch (statement.type)
                {
                    case NodeType.LogicStatement:
                        _program += @"Console.WriteLine(";
                        TranspileLogicStatement(statement);
                        _program += @");";
                        break;
                    case NodeType.Expression:
                        _program += @"Console.WriteLine(";
                        TranspileExpression(statement);
                        _program += @");";
                        break;
                    case NodeType.Conditional:
                        break;
                    default:
                        _logger.Error("Unhandled statement");
                        break;
                }
            }
        }
        writer.Write(_program);
        writer.Close();
        return _program;
    }

    private void TranspileExpression(ParseNode node)
    {
        List<ParseNode> terminals = new List<ParseNode>();
        node.GetTerminals(terminals);
        foreach (var terminal in terminals)
        {
            PrintTerminal(terminal);
        }
    }
    private void TranspileExpressionQuery(ParseNode node)
    {
        var firstExpression = node.getChildren().First();
        TranspileExpression(firstExpression);
        
        var comparisonOp = node.getChildren()[1];
        PrintTerminal(comparisonOp);
        
        var secondExpression = node.getChildren().Last();
        TranspileExpression(secondExpression);
    }

    private void TranspileBoolean(ParseNode node)
    {
        var booleanChild = node.getChildren().First();
        switch (booleanChild.type)
        {
            case NodeType.Terminal:
                if(booleanChild.getChildren().Count == 1)
                    //Boolean is true or false literal
                    PrintTerminal(booleanChild);

                else if(booleanChild.getChildren().First().token?.Type is TokenType.Not)
                {
                    //It's a !<logic-statement>
                    PrintTerminal(booleanChild.getChildren().First());
                    TranspileLogicStatement(booleanChild.getChildren()[1]);
                }
                else
                {
                    //It's a (Logic-Statement>)
                    PrintTerminal(booleanChild.getChildren().First());
                    TranspileLogicStatement(booleanChild.getChildren()[1]);
                    PrintTerminal(booleanChild.getChildren().Last());
                }
                break;
            
            case NodeType.ExpressionQuery:
                TranspileExpressionQuery(booleanChild);
                break;
        }
    }

    private void TranspileCondition(ParseNode node)
    {
        var conditionComparison = node.getChildren().First();
        PrintTerminal(conditionComparison);
        if(conditionComparison.token?.Type is TokenType.And or TokenType.Or)
        {
            //For and/or we add the token word twice because we don't do bitwise logic
            _program += conditionComparison.token?.Word;
        }
        var conditionBoolean = node.getChildren().Last();
        TranspileBoolean(conditionBoolean);
    }

    private void TranspileLogicStatement(ParseNode node)
    {
        var boolean = node.getChildren().First();
        TranspileBoolean(boolean);
        
        //TODO: Fix parser so that empty conditionals aren't added!!!
        if(node.getChildren().Count == 2 && node.getChildren().Last().getChildren().Count > 0)
            TranspileCondition(node.getChildren().Last());
    }
     
    
    public string GetProgramPath()
    {
        return Path.GetFullPath("../../../../C_Flat_Output/Program.cs");
    }

    public void ResetOutput()
    {
        //Writes the microsoft console application template to program.cs to prevent build errors.
        var writer = File.CreateText(GetProgramPath());
        foreach (var line in _defaultProgramString)
        {
            writer.WriteLine(line);
        }
        writer.Close();
    }
}