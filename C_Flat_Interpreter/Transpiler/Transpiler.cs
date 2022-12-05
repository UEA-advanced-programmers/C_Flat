using C_Flat_Interpreter.Common;
using C_Flat_Interpreter.Common.Enums;


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
        var writer = File.CreateText(GetProgramPath());
        foreach (var node in parseTree)
        {
            try
            {
                TranspileStatement(node);
            }
            catch(Exception e)
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
        var statement = node.getChildren().First();
        switch (statement.type)
        {
            //If and Whiles are functionally the same except while doesn't contain else
            case NodeType.ConditionalStatement:
            case NodeType.WhileStatement:
                TranspileConditionalStatement(statement);
                break;
            case NodeType.DeclareVariable:
                TranspileDeclaration(statement);
                break;
            case NodeType.VarAssignment:
                TranspileAssignment(statement);
                break;
            default:
                throw new Exception("Unhandled statement");
        }
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
            Program += conditionComparison.token?.Word;
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

    private void TranspileIdentifier(ParseNode node)
    {
        PrintTerminal(node.getChildren().First());
    }

    private void TranspileAssignment(ParseNode node)
    {
        var children = node.getChildren();
        TranspileIdentifier(children.First());
        
        //Print assignment terminal
        PrintTerminal(children[1]);
        
        //TODO: Add handling for other assignment types
        if (children[2].type is NodeType.Expression)
        {
            TranspileExpression(children[2]);
        }
        
        //Print semicolon terminal
        PrintTerminal(children.Last());
    }
    private void TranspileDeclaration(ParseNode node)
    {
        var children = node.getChildren();
        
        //print var keyword
        PrintTerminal(children.First());
        if (children[1].type is NodeType.VarIdentifier)
        {
            TranspileIdentifier(children[1]);
            
            //Print semicolon terminal
            PrintTerminal(children.Last());
        }
        else
        {
            TranspileAssignment(children.Last());
        }
    }
    private void TranspileBlock(ParseNode node)
    {
        var children = node.getChildren();
        //Print L Curly
        PrintTerminal(children.First());
        foreach (var statement in children.Where(child => child.type == NodeType.Statement))
        {
            TranspileStatement(statement);
        }
        //Print R Curly
        PrintTerminal(children.Last());
    }

    private void TranspileConditionalStatement(ParseNode node)
    {
        var children = node.getChildren();
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