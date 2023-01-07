using System.Diagnostics;
using C_Flat_Interpreter.Common.Enums;

namespace C_Flat_Interpreter.Common;

public static class VariableTable
{
    private static Dictionary<string, ParseNode> _table = new();

    public static void Add(string word, ParseNode node)
    {
        if (_table.ContainsKey(word))
            _table[word] = node;
        else
            _table.Add(word, node);
    }
    
    public static void Add(string word)
    {
        if (_table.ContainsKey(word))
            _table[word] = new(NodeType.Null);
        else
            _table.Add(word, new(NodeType.Null));
    }

    public static bool Exists(string identifier)
    {
        return _table.ContainsKey(identifier);
    }

    public static NodeType GetType(string identifier)
    {
        while (true)
        {
            var node = _table[identifier];

            if (node.type != NodeType.VarIdentifier) return node.type;
            identifier = node.token?.Word ?? throw new Exception("Identifier node token is null");
        }
    }

    public static void Clear()
    {
        _table.Clear();
    }
}