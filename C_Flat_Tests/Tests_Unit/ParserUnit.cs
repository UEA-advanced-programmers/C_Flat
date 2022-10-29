using System.Data;
using C_Flat_Interpreter.Lexer;
using C_Flat_Interpreter.Parser;
using NUnit.Framework;

namespace C_Flat_Tests.Tests_Unit;

public class ParserUnit
{
    [Test]
    public void Parser_Parse_DoubleMulti_ThrowsException()
    {
        Lexer lexer = new Lexer("1**2");
        Parser parser = new Parser(lexer.Tokenise());

        Assert.Throws(Is.TypeOf<SyntaxErrorException>().And.Message.Contains("Number expected"),
        delegate { parser.Parse(); });
    }
    
    [Test]
    public void Parser_Parse_NoRightParam_ThrowsException()
    {
        Lexer lexer = new Lexer("1*(2");
        Parser parser = new Parser(lexer.Tokenise());

        Assert.Throws(Is.TypeOf<SyntaxErrorException>().And.Message.Contains("Mismatched parentheses"),
            delegate { parser.Parse(); });
    }
    
    [Test]
    public void Parser_Parse_NoLeftParam_ThrowsException()
    {
        Lexer lexer = new Lexer("1*)2");
        Parser parser = new Parser(lexer.Tokenise());
        
        Assert.Throws(Is.TypeOf<SyntaxErrorException>().And.Message.Contains("Number expected"),
            delegate { parser.Parse(); });
    }
}