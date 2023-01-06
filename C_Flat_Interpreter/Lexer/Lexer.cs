using C_Flat_Interpreter.Common;
using C_Flat_Interpreter.Common.Enums;

namespace C_Flat_Interpreter.Lexer;
using System.Collections.Generic;

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
    private bool _failed;
    private string[] _lines;

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
        _failed = false;
        _tokens.Clear();

        var whitespace = "";

        input = input.Replace("\r", "");
        _lines = input.Split("\n");

        for (int j = 0; j < _lines.Length; j++) //line
        {
            for(int i = 0; i < _lines[j].Length; i++) //character
            {
                char c = _lines[j][i];
                var newToken = new Token
                {
                    Line = j
                };

                switch (c)
                {
                    case ' ':
                        whitespace += c;
                        newToken = null;
                        break;
                    case ';':
                        newToken.Type = TokenType.SemiColon;
                        newToken.Word = whitespace + c;
                        break;
                    case '+':
                        newToken.Type = TokenType.Add;
                        newToken.Word = whitespace + c;
                        break;
                    case '*':
                        newToken.Type = TokenType.Multi;
                        newToken.Word = whitespace + c;
                        break;
                    case '(':
                        newToken.Type = TokenType.LeftParen;
                        newToken.Word = whitespace + c;
                        break;
                    case ')':
                        newToken.Type = TokenType.RightParen;
                        newToken.Word = whitespace + c;
                        break;
                    case '{':
                        newToken.Type = TokenType.LeftCurlyBrace;
                        newToken.Word = whitespace + c;
                        break;
                    case '}':
                        newToken.Type = TokenType.RightCurlyBrace;
                        newToken.Word = whitespace + c;
                        break;
                    case '-':
                        newToken.Type = TokenType.Sub;
                        newToken.Word = whitespace + c;
                        break;
                    case '/':
                        newToken.Type = TokenType.Divide;
                        newToken.Word = whitespace + c;
                        break;
                    case '!':
                        if (_lines[j][i + 1] == '=')
                        {
                            i++;
                            newToken.Type = TokenType.NotEqual;
                            newToken.Word = whitespace + c + _lines[j][i];
                        }
                        else
                        {
                            newToken.Type = TokenType.Not;
                            newToken.Word = whitespace + c;
                        }
                        break;
                    case '=':
                        if (_lines[j][i + 1] == '=')
                        {
                            i++;
                            newToken.Type = TokenType.Equals;
                            newToken.Word = whitespace + c + _lines[j][i];
                        }
                        else
                        {
                            newToken.Type = TokenType.Assignment;
                            newToken.Word = whitespace + c;
                        }
                        break;
                    case '&':
                        newToken.Type = TokenType.And;
                        newToken.Word = whitespace + c;
                        break;
                    case '|':
                        newToken.Type = TokenType.Or;
                        newToken.Word = whitespace + c;
                        break;
                    case '<':
                        newToken.Type = TokenType.Less;
                        newToken.Word = whitespace + c;
                        break;
                    case '>':
                        newToken.Type = TokenType.More;
                        newToken.Word = whitespace + c;
                        break;
                    case '"':
                        newToken.Type = TokenType.String;
                        newToken.Word = whitespace + c;
                        var substring = ParseString(j, ++i);
                        newToken.Word += substring;
                        i += substring.Length - 1;
                        if (_lines[j].ElementAtOrDefault(i+1) != '"')
                        {
                            _logger.Error("Invalid lexeme encountered! Disregarding: {invalidToken}", newToken);
                            newToken = null;
                            _failed = true;
                        }
                        else
                            newToken.Word += _lines[j][++i];
                        
                        break;
                    default:
                        if (char.IsDigit(c))
                        {
                            newToken.Type = TokenType.Num;
                            var num = ParseNumber(j, i);
                            newToken.Word = whitespace + num;
                            i += num.Length - 1;
                        }
                        else if (char.IsLetter(c))
                        {
                            newToken.Type = TokenType.Word;
                            substring = ParseWord(j, i);
                            newToken.Word = whitespace + substring;
                            i += substring.Length - 1;
                        }
                        else
                        {
                            newToken = null;
                            _logger.Error("Invalid lexeme encountered! Disregarding: {invalidToken}", c.ToString());
                            _failed = true;
                        }
                        break;
                }

                if (newToken == null) continue;
                _tokens.Add(newToken);
                whitespace = "";
            }
            whitespace = "";
        }
        return _failed ? 1 : 0;
    }

    private string ParseNumber(int line, int index)
    {
        var numberString = _lines[line][index].ToString();
        bool isDecimal = false;
        while (index+1 < _lines[line].Length)
        {
            if (Char.IsDigit(_lines[line][index + 1]))
            {
                numberString += _lines[line][++index];    
            }
            else if (!isDecimal && _lines[line][index + 1] == '.' && index+2 < _lines[line].Length && char.IsDigit(_lines[line][index+2]))
            {
                //if '.' ensure there's not already '.' and at least one digit afterwards
                isDecimal = true;
                numberString += _lines[line][++index];
            }
            else
            {
                break;
            }
        }
        return numberString;
    }
    private string ParseWord(int line, int index)
    {
        var wordString = _lines[line][index].ToString();
        while (index+1 < _lines[line].Length)
        {
            if (Char.IsLetter(_lines[line][index + 1]))
            {
                wordString += _lines[line][++index];    
            }
            else
            {
                break;
            }
        }
        return wordString.ToLower();
    }


    private string ParseString(int line, int index)
    {
        var wordString = _lines[line][index].ToString();
        while (index + 1 < _lines[line].Length)
        {
            if (_lines[line][index + 1] != '"')
            {
                wordString += _lines[line][++index];
            }
            else
            {
                break;
            }
        }
        return wordString.ToLower();
    }
}