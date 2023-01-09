using C_Flat_Interpreter.Common.Enums;
using C_Flat_Interpreter.Common.Exceptions;

namespace C_Flat_Interpreter.Common;

public static class VariableTable
{
    private static Dictionary<string, NodeType> _table = new();
    private static Stack<string> _currentFunction = new();

    public static void Add(string identifier, ParseNode? node = null)
    {
        if (_currentFunction.Count != 0)
        {
            identifier = $"{_currentFunction.Peek()}:{identifier}";
        }
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

        if (node.type == NodeType.VariableIdentifier)
        {
            _table.Add(identifier, GetType(node.GetChild().token.ToString()  ?? throw new SyntaxErrorException("Identifier node token is null")));
            return;
        }
        _table.Add(identifier, node.type);
    }

    public static bool Exists(string identifier)
    {
        if (_currentFunction.Count != 0)
        {
            identifier = $"{_currentFunction.Peek()}:{identifier}";
        }
        return _table.ContainsKey(identifier);
    }

    public static NodeType GetType(string identifier)
    {
        if (_currentFunction.Count != 0)
        {
            identifier = $"{_currentFunction.Peek()}:{identifier}";
        }
        return _table[identifier];
    }

    public static void EnterFunction(string functionIdentifier)
    {
        _currentFunction.Push(functionIdentifier);
    }
    public static void LeaveFunction()
    {
        _currentFunction.Pop();
    }

    public static void Clear()
    {
        _currentFunction.Clear();
        _table.Clear();
    }
}