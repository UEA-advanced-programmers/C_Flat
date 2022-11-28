using C_Flat_Interpreter.Common;

namespace C_Flat_Interpreter.Parser;

public class ParseNode
{
    private List<ParseNode> childNodes = new();
    private Token? token;
    private NodeType type;
    
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

    public override string ToString()
    {
        return type.ToString();
    }
}