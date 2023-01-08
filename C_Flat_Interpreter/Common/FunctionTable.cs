using System.Diagnostics;

namespace C_Flat_Interpreter.Common;

public class FunctionTable
{
    private static Dictionary<string, List<string>> _table = new();

    public static void Add(string identifier, List<string> parameters)
    {
        if(!_table.ContainsKey(identifier))
            _table.Add(identifier, parameters);
    }
    
    public static void Add(string identifier)
    {
        if(!_table.ContainsKey(identifier))
            _table.Add(identifier, new List<string>());
    }

    public static void AddParameter(string identifier, string parameter)
    {
        _table[identifier]?.Add(parameter);
    }

    public static bool Exists(string identifier)
    {
        return _table.ContainsKey(identifier);
    }

    public static List<string>? GetParams(string identifier)
    {
        return _table[identifier];
    }

    public static void Clear()
    {
        _table.Clear();
    }
}