using System.Data;
using C_Flat_Interpreter.Common;
using C_Flat_Interpreter.Common.Enums;
using C_Flat_Interpreter.Parser;
using C_Flat_Interpreter.Lexer;
using NUnit.Framework;

namespace C_Flat_Tests.Tests_Unit;

public class ParserUnit
{
    //utility
    bool ValidStatementNode(StatementNode statement, String phrase)
    {
        Lexer lexer = new Lexer();
        lexer.Tokenise(phrase);
        List<Token> tokens = lexer.GetTokens();
        for (int i = 0; i < tokens.Count; i++)
        {
            Token token = tokens[i];
            Token statementToken = statement.statement[i];
            if ((token.Equals(statementToken))) 
                return false;
        }
        return true;
    }
    //tests
    [Test]
    public void Parser_BlockTreeSingleStatement()
    {
        //declarations
        string statement = "4+3;";
        Lexer lexer = new Lexer();
        lexer.Tokenise(statement);
        List<Token> tokens = lexer.GetTokens();
        BlockNode block = new BlockNode();
        block.Load(tokens, 0);
        Parser parser = new Parser();
        //tests
        Assert.That(ValidStatementNode(block.statements[0], statement));
        parser.Parse(tokens);
        Assert.That(parser.Parse(tokens) == 0);
    }
    [Test]
    public void Parser_BlockTreeMultipleStatements()
    {
        //declarations
        string statement = "4+3;true==false;";
        Lexer lexer = new Lexer();
        lexer.Tokenise(statement);
        List<Token> tokens = lexer.GetTokens();
        BlockNode block = new BlockNode();
        block.Load(tokens, 0);
        Parser parser = new Parser();
        //tests
        Assert.That(ValidStatementNode(block.statements[0], "4+3;"));
        Assert.That(ValidStatementNode(block.statements[1], "true==false;"));
        Assert.That(parser.Parse(tokens) == 0);
    }
    [Test]
    public void Parser_IfStatementEmpty()
    {
        //declarations
        string statement = "if(true){}";
        Lexer lexer = new Lexer();
        lexer.Tokenise(statement);
        List<Token> tokens = lexer.GetTokens();
        BlockNode block = new BlockNode();
        block.Load(tokens, 0);
        Parser parser = new Parser();
        //tests
        Assert.That(ValidStatementNode(block.statements[0], "if(true){}"));
        Assert.That(parser.Parse(tokens) == 0);
    }
    [Test]
    public void Parser_IfStatementWithExpression()
    {
        //declarations
        string statement = "if(true){4;}";
        Lexer lexer = new Lexer();
        lexer.Tokenise(statement);
        List<Token> tokens = lexer.GetTokens();
        BlockNode block = new BlockNode();
        block.Load(tokens, 0);
        Parser parser = new Parser();
        //tests
        Assert.That(ValidStatementNode(block.statements[0], "if(true){}"));
        Assert.That(ValidStatementNode(block.statements[0].subBlock.statements[0], "4;"));
        Assert.That(parser.Parse(tokens) == 0);
    }
    [Test]
    public void Parser_IfStatementWithLogic()
    {
        //declarations
        string statement = "if(true){true;}";
        Lexer lexer = new Lexer();
        lexer.Tokenise(statement);
        List<Token> tokens = lexer.GetTokens();
        BlockNode block = new BlockNode();
        block.Load(tokens, 0);
        Parser parser = new Parser();
        //tests
        Assert.That(ValidStatementNode(block.statements[0], "if(true){}"));
        Assert.That(ValidStatementNode(block.statements[0].subBlock.statements[0], "true;"));
        Assert.That(parser.Parse(tokens) == 0);
    }
    [Test]
    public void Parser_IfElseStatement()
    {
        //declarations
        string statement = "if(true){7;}else{true==false;}";
        Lexer lexer = new Lexer();
        lexer.Tokenise(statement);
        List<Token> tokens = lexer.GetTokens();
        BlockNode block = new BlockNode();
        block.Load(tokens, 0);
        Parser parser = new Parser();
        //tests
        Assert.That(ValidStatementNode(block.statements[0], "if(true){}"));
        Assert.That(ValidStatementNode(block.statements[0].subBlock.statements[0], "7;"));
        Assert.That(ValidStatementNode(block.statements[1], "else{}"));
        Assert.That(ValidStatementNode(block.statements[1].subBlock.statements[0], "true==false;"));
        Assert.That(parser.Parse(tokens) == 0);
    }
}