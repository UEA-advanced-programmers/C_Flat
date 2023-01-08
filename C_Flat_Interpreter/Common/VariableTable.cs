using System.Diagnostics;
using C_Flat_Interpreter.Common.Enums;

namespace C_Flat_Interpreter.Common;

public static class VariableTable
{
    private static Dictionary<string, NodeType> _table = new();

    public static void Add(string word, ParseNode? node = null)
    {
        if (node == null)
        {
            _table.Add(word, NodeType.Null);
            return;
        }
        
        if (_table.ContainsKey(word))
        {
            _table[word] = node.type;
            return;
        }

        if (node.type == NodeType.Identifier)
        {
            _table.Add(word, GetType(node.token?.Word  ?? throw new Exception("Identifier node token is null")));
        }
        _table.Add(word, node.type);
    }

    public static bool Exists(string identifier)
    {
        return _table.ContainsKey(identifier);
    }

    public static NodeType GetType(string identifier)
    {
        return _table[identifier];
    }

    public static void Clear()
    {
        _table.Clear();
    }
}