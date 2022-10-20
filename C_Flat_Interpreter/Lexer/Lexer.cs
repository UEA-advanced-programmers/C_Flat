using C_Flat_Interpreter.Common;
using C_Flat_Interpreter.Common.Enums;

namespace C_Flat_Interpreter.Lexer;
using System.Collections.Generic;

/*
Assumptions - only one line can be entered. Each character is a new token

This will be changed but is assumed now in order to create a simple starting lexer
*/

/*
 * List of things to add in future
 * TODO - Create Token class and move over token stuff
 * TODO - Find out if I need to count the number of tokens (maybe just for testing)
 * TODO - Finalise enum names
 * TODO - Use our own language
 * TODO - Consider multi-digit numbers
 * TODO - Other operators (divide, power, root etc)
 */

public class Lexer
{
    private readonly string _input;
    private readonly List<Token> _tokens = new List<Token>(); //TODO - define max with the group
    private int _tokenCount = 0;

    //constructor
    public Lexer(string input)
    {
        _input = input;
    }

    //TODO - might be needed for tests, otherwise get rid of this
    public string GetInput()
    {
        return _input;
    }

    public Token GetFromTokenList(int placeToSearch)
    {
        return _tokens[placeToSearch];
    }

    public int Tokenise() //TODO - think about if this needs to return an int
    {
        foreach(char c in _input)
        {
            var newToken  = new Token();
            switch(c)
            {
                case ' ' :
                    //ignore
                    return 0;
                case '+' :
                    _tokenCount++; //todo - do I need this?
                    newToken.Type = TokenType.Add;
                    newToken.Value = c;
                    break;
                case '*' :
                    _tokenCount++; //todo - do I need this?
                    newToken.Type = TokenType.Multi;
                    newToken.Value = c;
                    break;
                case '(' :
                    _tokenCount++; //todo - do I need this?
                    newToken.Type = TokenType.LeftParam;
                    newToken.Value = c;
                    break;
                case ')' :
                    _tokenCount++; //todo - do I need this?
                    newToken.Type = TokenType.RightParam;
                    newToken.Value = c;
                    break;
                default :
                    if (char.IsDigit(c))
                    {
                        _tokenCount++;
                        newToken.Type = TokenType.Num;
                        newToken.Value = c;
                    }
                    else
                    {
                        //TODO - need an error message here!
                    }
                    break;
            }
            _tokens.Add(newToken);
        }
        return 0;
    }
}