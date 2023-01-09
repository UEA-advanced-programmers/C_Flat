using C_Flat_Interpreter.Common;
using C_Flat_Interpreter.Common.Enums;
using C_Flat_Interpreter.Common.Exceptions;


namespace C_Flat_Interpreter.Transpiler;

public class Transpiler : InterpreterLogger
{
    private readonly string[] _defaultProgramString =
    {
        "// See https://aka.ms/new-console-template for more information",
        @"Console.WriteLine(""Hello, World!"");",
    };

    private int _currentLine;
    public string Program { get; private set; }
    public Transpiler()
    {
        Program = string.Empty;
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
            Program += Environment.NewLine;
        }
        Program += tokenToPrint.Word;
    }


    public int Transpile(List<ParseNode> parseTree)
    {
        //Retrieve program.cs file
        Program = string.Empty;
        _currentLine = 0;
        ClearLogs();
        var writer = File.CreateText(GetProgramPath());
        foreach (var node in parseTree)
        {
            try
            {
                if (node.GetChild().type is NodeType.TopLevelStatement)
                {
                    TranspileTopLevelStatement(node.GetChild());                    
                }
                else
                {
                    TranspileControlStatement(node.GetChild());
                }
            }
            catch (Exception e)
            {
                //if we catch an error write to output and return a failed status code
                _logger.Error(e.Message);
                writer.Write(Program);
                writer.Close();
                return 1;
            }
        }
        writer.Write(Program);
        writer.Close();
        return 0;
    }

    private void TranspileStatement(ParseNode node)
    {
        var statement = node.GetChild();
        switch (statement.type)
        {
            //If and Whiles are functionally the same except while doesn't contain else
            case NodeType.ControlStatement:
                TranspileControlStatement(statement);
                break;
            case NodeType.TopLevelStatement:
                TranspileTopLevelStatement(statement);
                break;
            default:
                throw new Exception("Unhandled statement");
        }
    }

    private void TranspileControlStatement(ParseNode node)
    {
        var controlStatement = node.GetChild();
        switch (controlStatement.type)
        {
            //If and Whiles are functionally the same except while doesn't contain else
            case NodeType.ConditionalStatement:
            case NodeType.WhileStatement:
                TranspileConditionalStatement(controlStatement);
                break;
            case NodeType.VariableDeclaration:
                TranspileVariableDeclaration(controlStatement);
                break;
            case NodeType.VariableAssignment:
                TranspileVariableAssignment(controlStatement);
                break;
            case NodeType.FunctionCall:
                TranspileFunctionCall(controlStatement);
                break;
            default:
                throw new Exception("Unhandled statement");
        }
    }

    private void TranspileTopLevelStatement(ParseNode node)
    {
        var topLevelStatement = node.GetChild();
        TranspileFunctionDefinition(topLevelStatement);
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
        var firstExpression = node.GetChild();
        TranspileExpression(firstExpression);

        var comparisonOp = node.GetChild(1);
        PrintTerminal(comparisonOp);

        var secondExpression = node.GetLastChild();
        TranspileExpression(secondExpression);
    }

    private void TranspileBoolean(ParseNode node)
    {
        var children = node.GetChildren();
        switch (children.First().type)
        {
            //Check for !<LogicStatement>, bool literal, ( <Logic-Statement> )
            case NodeType.Terminal:
                {
                    PrintTerminal(children.First());
                    //Handle !LogicStatement, ( <Logic-Statement> )
                    if (children.Count > 1)
                    {
                        TranspileLogicStatement(children[1]);
                        if (children.Count > 2)
                        {
                            //print R Paren 
                            PrintTerminal(children.Last());
                        }
                    }
                    break;
                }
            case NodeType.ExpressionQuery:
                TranspileExpressionQuery(children.First());
                break;
            default:
                TranspileIdentifier(children.First());
                break;
        }
    }

    private void TranspileCondition(ParseNode node)
    {
        var conditionComparison = node.GetChild();
        PrintTerminal(conditionComparison);
        if (conditionComparison.token?.Type is TokenType.And or TokenType.Or)
        {
            //For and/or we add the token word twice because we don't do bitwise logic
            Program += conditionComparison.token?.Word;
        }
        var conditionBoolean = node.GetLastChild();
        TranspileBoolean(conditionBoolean);
    }

    private void TranspileLogicStatement(ParseNode node)
    {
        var boolean = node.GetChild();
        TranspileBoolean(boolean);

        //TODO: Fix parser so that empty conditionals aren't added!!!
        if (node.GetChildren().Count == 2 && node.GetLastChild().GetChildren().Count > 0)
            TranspileCondition(node.GetLastChild());
    }

    private void TranspileString(ParseNode node)
    {
        PrintTerminal(node.GetChild());
    }

    private void TranspileIdentifier(ParseNode node)
    {
        PrintTerminal(node.GetChild());
    }

    private void TranspileVariableAssignmentValue(ParseNode node)
    {
        var valueNode = node.GetChild();
        //TODO: Add handling for other assignment types
        switch (valueNode.type)
        {
            case NodeType.Expression:
                TranspileExpression(valueNode);
                break;
            case NodeType.String:
                TranspileString(valueNode);
                break;
            case NodeType.LogicStatement:
                TranspileLogicStatement(valueNode);
                break;
            case NodeType.VariableIdentifier:
                TranspileIdentifier(valueNode);
                break;
        }
    }

    private void TranspileVariableAssignment(ParseNode node)
    {
        var children = node.GetChildren();
        TranspileIdentifier(children.First());

        //Print assignment terminal
        PrintTerminal(children[1]);

        TranspileVariableAssignmentValue(children[2]);

        //Print semicolon terminal
        PrintTerminal(children.Last());
    }
    private void TranspileVariableDeclaration(ParseNode node)
    {
        var children = node.GetChildren();

        //  Determine variable type before transpile
        if (children[1].type is NodeType.VariableIdentifier)
        {
            try
            {
                TranspileType(children[1]);
            }
            catch (InvalidSyntaxException e)
            {
                _logger.Warning(e.Message);
                return;
            }
            TranspileIdentifier(children[1]);
            //Print semicolon terminal
            PrintTerminal(children.Last());
        }
        else
        {
            PrintTerminal(children.First());
            TranspileVariableAssignment(children.Last());
        }
    }

    private void TranspileType(ParseNode identifierNode)
    {
        var identifierToken = identifierNode.GetChild().token;
        //  Find first assignment of this variable
        var type = VariableTable.GetType(identifierToken?.ToString() ?? throw new Exception());
        var typeWord = type switch
        {
            NodeType.Expression => "float",
            NodeType.LogicStatement => "bool",
            NodeType.String => "string",
            NodeType.Null => throw new InvalidSyntaxException($"Variable '{identifierToken}' is declared but never used, omitting"),
            _ => throw new IncorrectTypeException("Variable has invalid assignment type")
        };
        if (identifierToken.Line > _currentLine)
        {
            _currentLine = identifierToken.Line;
            Program += Environment.NewLine;
        }
        Program += typeWord;
    }
    private void TranspileBlock(ParseNode node)
    {
        var children = node.GetChildren();
        //Print L Curly
        PrintTerminal(children.First());
        foreach (var statement in children.Where(child => child.type == NodeType.Statement))
        {
            TranspileControlStatement(statement);
        }
        //Print R Curly
        PrintTerminal(children.Last());
    }

    private void TranspileConditionalStatement(ParseNode node)
    {
        var children = node.GetChildren();
        //Print if/while keyword
        PrintTerminal(children.First());

        //Print L Paren
        PrintTerminal(children[1]);

        //Transpile condition (logic statement)
        TranspileLogicStatement(children[2]);

        //Print R Paren
        PrintTerminal(children[3]);

        //Transpile block
        TranspileBlock(children[4]);

        //Check for else
        if (children.Count <= 5) return;
        //Print else
        PrintTerminal(children[5]);
        //Transpile block
        TranspileBlock(children.Last());
    }
    
    private void TranspileFunctionDefinition(ParseNode node)
    {
        var children = node.GetChildren();
        
        //Print first child replacing the word
        if (children.First().token?.ToString() is "func")
        {
            var token = children.First().token ?? throw new Exception("func token empty");
            token.Word = token.Word.Replace("func", "void");
        }
        PrintTerminal(children.First());
        
        //Transpile identifier
        TranspileIdentifier(children[1]);
        
        //  Print left paren
        PrintTerminal(children[2]);
        
        //  count parameters
        var paramNodes = children.Where(child => child.type is NodeType.Parameter).ToList();
        
        //  Transpile all parameters adding commas between them
        for (int i = 0; i < paramNodes.Count; i++)
        {
            TranspileParameter(paramNodes[i]);
            if (i + 1 < paramNodes.Count)
                Program += ",";
        }

        // Print right paren
        PrintTerminal(children.Last(child => child.type is NodeType.Terminal));

        // Transpile block
        TranspileBlock(children.Last());
    }

    private void TranspileParameter(ParseNode node)
    {
        //  Determine type of parameter
        var identifierString = node.GetChild(1).GetChild().token?.ToString() ?? "";
        if (VariableTable.Exists(identifierString))
        {
            var paramType = VariableTable.GetType(identifierString);
            switch (paramType)
            {
                case NodeType.Expression:
                    Program += "float";
                    break;
                case NodeType.String:
                    Program += "string";
                    break;
                case NodeType.LogicStatement:
                    Program += "bool";
                    break;
                default:
                    throw new Exception("Invalid parameter type");
            }
        }
        
        //  Transpile identifier
        TranspileIdentifier(node.GetChild(1));
    }

    private void TranspileFunctionCall(ParseNode node)
    {
        var children = node.GetChildren();
        
        //  Transpile function identifier
        TranspileIdentifier(children.First());
        
        //  Print left paren
        PrintTerminal(children[1]);
        
        //  Transpile arguments
        var argumentNodes = children.Where(child => child.type is NodeType.AssignmentValue).ToList();
        
        //  Transpile all arguments adding commas between them
        for (int i = 0; i < argumentNodes.Count; i++)
        {
            TranspileVariableAssignmentValue(argumentNodes[i]);
            if (i + 1 < argumentNodes.Count)
                Program += ",";
        }

        //  Print right paren
        PrintTerminal(children.Last(x=> x.type is NodeType.Terminal && x.token!.Type is TokenType.RightParen));
        
        //  Print semi-colon
        PrintTerminal(children.Last());

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