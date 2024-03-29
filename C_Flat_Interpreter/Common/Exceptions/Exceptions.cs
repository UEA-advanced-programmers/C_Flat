﻿namespace C_Flat_Interpreter.Common.Exceptions;

public class ParserException : Exception
{
    protected ParserException(string prefix, string? message) : base($"{prefix} {message}") { }
    protected ParserException(string prefix, int line, string? message) : base($"[Line: {line}] {prefix} {message}") { }

}

public class InvalidSyntaxException : ParserException
{
    public InvalidSyntaxException(string? message) : base("Invalid Syntax!", message) { }
    public InvalidSyntaxException(string? message, int line) : base("Invalid Syntax!", line, message) { }

}
public class SyntaxErrorException : ParserException
{
    public SyntaxErrorException(string? message) : base("Syntax Error!", message) { }
    public SyntaxErrorException(string? message, int line) : base("Syntax Error!", line, message) { }
}
public class IncorrectTypeException : ParserException
{
    public IncorrectTypeException(string? message) : base("Incorrect Type!", message) { }

    public IncorrectTypeException(string? message, int line) : base("Incorrect Type!", line, message) { }
}

public class OutOfScopeException : ParserException
{
    public OutOfScopeException(string? message) : base("Scoping error!", message) { }

    public OutOfScopeException(string? message, int line) : base("Scoping Error!", line, message) { }
}