using C_Flat_Interpreter.Common.Enums;
using C_Flat_Interpreter.Common.Exceptions;

namespace C_Flat_Interpreter.Common;

public class ParseNode
{
    private List<ParseNode> childNodes = new();
    public Token? token;
    public NodeType type;
    
    public ParseNode(NodeType type, Token token)
    {
        this.type = type;
        this.token = token;
    }

    public ParseNode(NodeType type)
    {
        this.type = type;
    }

    public void AddChild(ParseNode child)
    {
        childNodes.Add(child);
    }

    public ParseNode GetChild(int index = 0)
    {
        return childNodes[index] ?? throw new Exception("Index out of bounds"); //todo - exception
    }
    
    public ParseNode GetLastChild()
    {
        return childNodes.Last();
    }

    //Testing Function
    public List<ParseNode> GetChildren()
    {
        return childNodes;
    }

    public void GetTerminals(List<ParseNode> terminalList)
    {
        if (this.type is NodeType.Terminal)
        {
            terminalList.Add(this);
            return;
        }
        foreach (var childNode in this.childNodes)
        {
            childNode.GetTerminals(terminalList);
        }
    }

    public override string ToString()
    {
        return token != null ? $"{type.ToString()}: {token.Word.Trim()}" : type.ToString();
    }
}