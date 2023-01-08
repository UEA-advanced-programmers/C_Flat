namespace C_Flat_Interpreter.Common;

public class FunctionTable
{
    private static Dictionary<string, ParseNode> _table = new();

    public static void Add(string identifier, ParseNode node)
    {
        if(!_table.ContainsKey(identifier))
            _table.Add(identifier, node);
    }

    public static bool Exists(string identifier)
    {
        return _table.ContainsKey(identifier);
    }

    public static void Clear()
    {
        _table.Clear();
    }
}