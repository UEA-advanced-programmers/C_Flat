using C_Flat_Interpreter.Common;
using C_Flat_Interpreter.Common.Enums;
using Microsoft.Extensions.Logging;

namespace C_Flat_Interpreter.Lexer;
using System.Collections.Generic;

/*
Assumptions - only one line can be entered.

This will be changed but is assumed now in order to create a simple starting lexer
*/

/*
 * List of things to add in future
 * 
 * TODO - Other operators (power, root etc)
 * TODO - Think about what should happen if user inputs nothing - currently causes issue in parser, but maybe should be prevented in lexer
 * TODO - AddToken function instead of repeating in the switch statement
 */

public class Lexer : InterpreterLogger
{
    private readonly List<Token> _tokens = new(); //TODO - define max with the group
    private readonly ILogger _logger;

    //constructor
    public Lexer()
    {
        _logger = GetLogger("Lexer");
    }

    public List<Token> GetTokens()
    {
        return _tokens;
    }

    //TODO - Handle index out of bounds exceptions here
    public Token GetFromTokenList(int placeToSearch)
    {
        return _tokens[placeToSearch];
    }

    public List<Token> Tokenise(string input)
    {
        _tokens.Clear();
        //TODO - Set fail flag to prevent parser from continuing
        for(int i = 0; i < input.Length; i++)
        {
            char c = input[i];
            var newToken  = new Token();
            switch(c)
            {
                case ' ' :
                    //ignore whitespace
                    newToken = null;
                    break;
                case '+' :
                    newToken.Type = TokenType.Add;
                    newToken.Word = c.ToString();
                    break;
                case '*' :
                    newToken.Type = TokenType.Multi;
                    newToken.Word = c.ToString();
                    break;
                case '(' :
                    newToken.Type = TokenType.LeftParen;
                    newToken.Word = c.ToString();
                    break;
                case ')' :
                    newToken.Type = TokenType.RightParen;
                    newToken.Word = c.ToString();
                    break;
                case '-' :
                    newToken.Type = TokenType.Sub;
                    newToken.Word = c.ToString();
                    break;
                case '/' :
                    newToken.Type = TokenType.Divide;
                    newToken.Word = c.ToString();
                    break;
                default :
                    if (char.IsDigit(c))
                    {
                        var numberString = c.ToString();
                        newToken.Type = TokenType.Num;
                        bool isDecimal = false;
                        while (i+1 < input.Length)
                        {
                            if (Char.IsDigit(input[i+1]))
                            {
                                numberString += input[++i];    
                            }
                            else if (!isDecimal && input[i+1] == '.' && i+2 < input.Length && char.IsDigit(input[i+2]))
                            {
                                //if '.' ensure there's not already '.' and at least one digit afterwards
                                isDecimal = true;
                                numberString += input[++i];
                            }
                            else
                            {
                                break;
                            }
                        }
                        newToken.Word = numberString;
                        newToken.Value = double.Parse(numberString);
                    }
                    else
                    {
                        newToken = null;
                        var invalidToken = c.ToString();
                        while (i + 1 < input.Length && input[i + 1] != ' ')
                        {
                            invalidToken += input[++i];
                        }
                        _logger.LogWarning("Invalid token encountered, disregarding: {invalidToken}", invalidToken);
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