using C_Flat_Interpreter.Common.Enums;
using C_Flat_Interpreter.Common.Exceptions;

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

        if (node.type == NodeType.VariableIdentifier)
        {
            _table.Add(word, GetType(node.token?.Word  ?? throw new SyntaxErrorException("Identifier node token is null")));
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