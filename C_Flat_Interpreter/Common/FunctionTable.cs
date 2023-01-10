using C_Flat_Interpreter.Common.Enums;

namespace C_Flat_Interpreter.Common;

public static class FunctionTable
{
    //  We need to statically declare the function dictionary with our standard library of functions

    private static readonly Dictionary<string, List<NodeType>> FunctionLibrary = new()
    {
        { "print", new List<NodeType>() {NodeType.Null}},
    };

    public static bool Exists(string identifier)
    {
        return FunctionLibrary.ContainsKey(identifier);
    }

    public static List<NodeType> GetParams(string identifier)
    {
        return FunctionLibrary[identifier];
    }
}