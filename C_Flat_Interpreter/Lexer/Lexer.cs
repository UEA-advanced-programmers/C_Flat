using C_Flat_Interpreter.Common;
using C_Flat_Interpreter.Common.Enums;
using Microsoft.Extensions.Logging;

namespace C_Flat_Interpreter.Lexer;
using System.Collections.Generic;

/*
Assumptions - only one line can be entered. Each character is a new token.

This will be changed but is assumed now in order to create a simple starting lexer
*/

/*
 * List of things to add in future
 * 
 * TODO - Consider multi-digit numbers
 * TODO - Other operators (power, root etc)
 */

public class Lexer : InterpreterLogger
{
    private readonly string _input;
    private readonly List<Token> _tokens = new List<Token>(); //TODO - define max with the group
    private readonly ILogger _logger;

    //constructor
    public Lexer(string input)
    {
        _input = input;
        _logger = GetLogger("Lexer");
    }

    //TODO - might be needed for tests, otherwise get rid of this
    public string GetInput()
    {
        return _input;
    }

    public List<Token> GetTokens()
    {
        return _tokens;
    }

    public Token GetFromTokenList(int placeToSearch)
    {
        return _tokens[placeToSearch];
    }

    public List<Token> Tokenise() //TODO - think about if this needs to return an int
    {
        foreach(char c in _input)
        {
            var newToken  = new Token();
            switch(c)
            {
                case ' ' :
                    //ignore whitespace
                    newToken = null;
                    break;
                case '+' :
                    newToken.Type = TokenType.Add;
                    newToken.Word = c;
                    break;
                case '*' :
                    newToken.Type = TokenType.Multi;
                    newToken.Word = c;
                    break;
                case '(' :
                    newToken.Type = TokenType.LeftParen;
                    newToken.Word = c;
                    break;
                case ')' :
                    newToken.Type = TokenType.RightParen;
                    newToken.Word = c;
                    break;
                case '-' :
                    newToken.Type = TokenType.Sub;
                    newToken.Word = c;
                    break;
                case '/' :
                    newToken.Type = TokenType.Divide;
                    newToken.Word = c;
                    break;
                default :
                    if (char.IsDigit(c))
                    {
                        newToken.Type = TokenType.Num;
                        newToken.Word = c;
                    }
                    else
                    {
                        //TODO - need an error message here!
                        
                        //Due to an error, this method returns early with the list of tokens it was able to create,
                        //meaning it may not have been able to make it through the whole input, however en error will
                        //logged if this is the case
                        return _tokens;
                    }
                    break;
            }

            if (newToken != null)
            {
                _tokens.Add(newToken);
            }
        }
        return _tokens;
    }
}