using C_Flat_Interpreter.Common.Enums;
using C_Flat_Interpreter.Common.Exceptions;

namespace C_Flat_Interpreter.Common;

public static class VariableTable
{
    private static Dictionary<string, NodeType> _table = new();

    public static void Add(string identifier, ParseNode? node = null)
    {
        if (node == null)
        {
            if (_table.ContainsKey(identifier))
                _table[identifier] = NodeType.Null;
            else
                _table.Add(identifier, NodeType.Null);
            return;
        }
        
        if (_table.ContainsKey(identifier))
        {
            _table[identifier] = node.type;
            return;
        }
        
        //  By removing variable identifier from a valid assignment value we might not need this
        if (node.type == NodeType.VariableIdentifier)
        {
            _table.Add(identifier, GetType(node.GetChild().token.ToString()  ?? throw new SyntaxErrorException("Identifier node token is null")));
            return;
        }
        _table.Add(identifier, node.type);
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