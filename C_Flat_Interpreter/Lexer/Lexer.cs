using C_Flat_Interpreter.Common;
using C_Flat_Interpreter.Common.Enums;

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
    private string _input;
    private bool failed;

    //constructor
    public Lexer()
    {
        GetLogger("Lexer");
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

    public int Tokenise(string input)
    {
        failed = false;
        _tokens.Clear();
        _input = input;
        for(int i = 0; i < _input.Length; i++)
        {
            char c = _input[i];
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
                case '!':
                    newToken.Type = TokenType.Not;
                    newToken.Word = c.ToString();
                    break;
                case '=':
                    newToken.Type = TokenType.Equals;
                    newToken.Word = c.ToString();
                    break;
                case '&':
                    newToken.Type = TokenType.And;
                    newToken.Word = c.ToString();
                    break;
                case '|':
                    newToken.Type = TokenType.Or;
                    newToken.Word = c.ToString();
                    break;
                case '<':
                    newToken.Type = TokenType.Less;
                    newToken.Word = c.ToString();
                    break;
                case '>':
                    newToken.Type = TokenType.More;
                    newToken.Word = c.ToString();
                    break;
                default :
                    if (char.IsDigit(c))
                    {
                        newToken.Type = TokenType.Num;
                        newToken.Word = ParseNumber(i);
                        i += newToken.Word.Length-1;
                        newToken.Value = double.Parse(newToken.Word);
                    }
                    else if(char.IsLetter(c))
                    {
                        newToken.Type = TokenType.String;
                        newToken.Word = ParseWord(i);
                        i += newToken.Word.Length-1;
                        newToken.Value = newToken.Word;
                    }
                    else
                    {
                        newToken = null;
                        _logger.Error("Invalid lexeme encountered! Disregarding: {invalidToken}", c.ToString());
                        failed = true;
                    }
                    break;
            }

            if (newToken != null)
            {
                _tokens.Add(newToken);
            }
        }
        return failed ? 1 : 0;
    }

    private string ParseNumber(int index)
    {
        var numberString = _input[index].ToString();
        bool isDecimal = false;
        while (index+1 < _input.Length)
        {
            if (Char.IsDigit(_input[index+1]))
            {
                numberString += _input[++index];    
            }
            else if (!isDecimal && _input[index+1] == '.' && index+2 < _input.Length && char.IsDigit(_input[index+2]))
            {
                //if '.' ensure there's not already '.' and at least one digit afterwards
                isDecimal = true;
                numberString += _input[++index];
            }
            else
            {
                break;
            }
        }
        return numberString;
    }
    private string ParseWord(int index)
    {
        var wordString = _input[index].ToString();
        while (index+1 < _input.Length)
        {
            if (Char.IsLetter(_input[index+1]))
            {
                wordString += _input[++index];    
            }
            else
            {
                break;
            }
        }
        return wordString.ToLower();
    }

}