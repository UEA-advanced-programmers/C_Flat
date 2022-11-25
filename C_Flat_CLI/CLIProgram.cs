using C_Flat_Interpreter.Lexer;
using C_Flat_Interpreter.Parser;
using C_Flat_Interpreter.Transpiler;

namespace C_Flat_CLI; // Note: actual namespace depends on the project name.

internal static class CLIProgram
{
    static void Main(string[] args)
    {
        Console.WriteLine("Welcome to the CLI for the C_Flat Transpiler!");
        Console.WriteLine("Please enter your C_Flat code followed by enter to transpile it");
        var input = Console.ReadLine();
        Lexer _lexer = new Lexer();
        if (_lexer.Tokenise(input) != 0)
        {
            Console.WriteLine("Lexing failed! See above logs!");
            return;
        }
        Parser _parser = new Parser();
        if (_parser.Parse(_lexer.GetTokens()) != 0)
        {
            Console.WriteLine("Parsing failed! See above logs!");
            return;
        }
        Transpiler _transpiler = new Transpiler();
        _transpiler.Transpile(_parser.GetParseTree());
        Console.WriteLine($"Successfully transpiled output to {_transpiler.GetProgramPath()}");
    }
}
