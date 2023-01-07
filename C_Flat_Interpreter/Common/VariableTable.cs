using System.Diagnostics;
using C_Flat_Interpreter.Common.Enums;

namespace C_Flat_Interpreter.Common;

public class VariableTable
{
    private Dictionary<string, ParseNode> _table;
    public VariableTable()
    {
        _table = new Dictionary<string, ParseNode>();
    }

    public void Add(string word, ParseNode node)
    {
        _table.Add(word, node);
    }
    
    public void Add(string word)
    {
        _table.Add(word, new ParseNode(NodeType.Null));
    }

    public bool Exists(string identifier)
    {
        return _table.ContainsKey(identifier);
    }

    public NodeType GetType(string identifier)
    {
        while (true)
        {
            var node = _table[identifier];

            if (node.type != NodeType.VarIdentifier) return node.type;
            identifier = node.token?.Word ?? throw new Exception("Identifier node token is null");
        }
    }
}