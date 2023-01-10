using C_Flat_Interpreter.Common.Enums;
using C_Flat_Interpreter.Common.Exceptions;

namespace C_Flat_Interpreter.Common;

public static class VariableTable
{
    private static Dictionary<string, NodeType> _table = new();

    public static void Add(string identifier, NodeType type = NodeType.Null)
    {
        if (ReservedWords.Reserved(identifier))
            throw new Exception($"Cannot create variable with reserved word {identifier}");
        if (_table.ContainsKey(identifier))
        {
            _table[identifier] = type;
            return;
        }
        
        _table.Add(identifier, type);
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