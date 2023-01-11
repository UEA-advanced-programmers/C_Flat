using C_Flat_Interpreter.Common.Enums;

namespace C_Flat_Interpreter.Common;

public static class FunctionTable
{
    //  We need to statically declare the function dictionary with our standard library of functions

    //  Key = Function identifier, Value = Tuple<Parameters : Return type>
    private static readonly Dictionary<string, Tuple<List<NodeType>, NodeType>> FunctionLibrary = new()
    {
        { "Print", new Tuple<List<NodeType>, NodeType>(new List<NodeType>() {NodeType.Null}, NodeType.Null)},
        { "Concatenate", new Tuple<List<NodeType>, NodeType>(new List<NodeType>() {NodeType.String, NodeType.String}, NodeType.String)},
        { "Stringify", new Tuple<List<NodeType>, NodeType>(new List<NodeType>() {NodeType.Null}, NodeType.String)},
        { "Root", new Tuple<List<NodeType>, NodeType>(new List<NodeType>() {NodeType.Expression}, NodeType.Expression)},
        { "Power", new Tuple<List<NodeType>, NodeType>(new List<NodeType>() {NodeType.Expression, NodeType.Expression}, NodeType.Expression)},
    };

    public static bool Exists(string identifier)
    {
        return FunctionLibrary.ContainsKey(identifier);
    }

    public static List<NodeType> GetParams(string identifier)
    {
        return FunctionLibrary[identifier].Item1;
    }
    public static NodeType GetReturnType(string identifier)
    {
        return FunctionLibrary[identifier].Item2;
    }
}