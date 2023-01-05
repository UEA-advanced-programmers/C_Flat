using System.Diagnostics;
using C_Flat_Interpreter.Common.Enums;

namespace C_Flat_Interpreter.Common;

public class VariableLUT //todo - rename
{
    private Dictionary<string, ParseNode>? _dictionary; //todo - rename maybe?
    public VariableLUT()
    {
        _dictionary = new Dictionary<string, ParseNode>();
    }

    public void AddToLut(string word, ParseNode node)
    {
        _dictionary.Add(word, node);
    }
    
    public void AddToLut(string word)
    {
        _dictionary?.Add(word, new ParseNode(NodeType.Null, null));
    }

    public bool IsDeclared(string identifier)
    {
        return _dictionary != null && _dictionary.ContainsKey(identifier);
    }
    
    public ParseNode GetVarType(string identifier)
    {
        var node = _dictionary?[identifier];

        return node.type == NodeType.VarIdentifier ? GetVarType(node.token.Word) : node;
    }
}