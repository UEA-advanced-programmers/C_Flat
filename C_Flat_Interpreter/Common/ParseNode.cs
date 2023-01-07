using C_Flat_Interpreter.Common.Enums;

namespace C_Flat_Interpreter.Common;

public class ParseNode
{
    private List<ParseNode> childNodes = new();
    public Token? token;
    public NodeType type;

    //todo - figure out what constructors are needed
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

    //Testing Function
    public List<ParseNode> getChildren()
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