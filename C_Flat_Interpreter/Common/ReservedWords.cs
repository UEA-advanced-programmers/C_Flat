namespace C_Flat_Interpreter.Common;

public static class ReservedWords
{
    private static readonly List<string> Words = new(){
        "if",
        "else",
        "while",
        "var",
        "func",
        "true",
        "false",
        "Print",
        "Concatenate",
        "Stringify",
    };
    
    public static bool Reserved(string word)
    {
        return Words.Contains(word);
    }
}