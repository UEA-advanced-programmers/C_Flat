namespace C_Flat_Interpreter.Common.Exceptions;

public class InvalidSyntaxException : Exception
{
    public InvalidSyntaxException(string? message) : base($"Invalid Syntax encountered! {message}") { }
}
public class SyntaxErrorException : Exception
{
    public SyntaxErrorException(string? message) : base($"Syntax Error encountered! {message}") { }
}