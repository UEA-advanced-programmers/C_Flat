using System.Diagnostics;

namespace C_Flat_Interpreter.Common;

public static class FunctionTable
{
    private static Dictionary<string, List<string>> _table = new();

    public static void Add(string identifier, List<string> parameters)
    {
        if(!_table.ContainsKey(identifier))
            _table.Add(identifier, parameters);
    }

    public static bool Exists(string identifier)
    {
        return _table.ContainsKey(identifier);
    }

    public static List<string> GetParams(string identifier)
    {
        return _table[identifier];
    }

    public static void Clear()
    {
        _table.Clear();
    }
}