﻿using C_Flat_Interpreter.Common.Enums;

namespace C_Flat_Interpreter.Common;

public class Token
{
    public TokenType Type;
    public object? Value;
    public string Word;
    public int Line;
    public Token(TokenType type = default, object? value = null, int line = -1)
    {
        Type = type;
        Value = value;
        Line = line;
    }
}