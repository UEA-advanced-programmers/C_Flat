using C_Flat_Interpreter.Common.Enums;

namespace C_Flat_Interpreter.Common;

public class Token
{
    public TokenType Type;
    public string Word;
    public int Line;
    public Token(TokenType type = default, string word = "", int line = -1)
    {
        Type = type;
        Word = word;
        Line = line;
    }
}