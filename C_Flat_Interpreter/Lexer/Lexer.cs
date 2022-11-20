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
    //TEST CODE FOR MULTI-LINE INTERPRETATION
    private readonly List<Line> _lines = new();
    
    //END TEST CODE FOR MULTI-LINE INTERPRETATION
    private bool failed;

    //constructor
    public Lexer()
    {
        GetLogger("Lexer");
    }

    public List<Line> GetLines()
    {
        return _lines;
    }

    //TODO - Handle index out of bounds exceptions here
    public Line GetLine(int line)
    {
        if (line < _lines.Count)
        {
            return _lines[line];
        }
        throw new IndexOutOfRangeException($"Line {line} does not exist");
    }
    
    public int Lex(string input)
    {
        _lines.Clear();
        failed = false;
        input = input.Replace("\r", "");
        var lineStrings = input.Split("\n");
        foreach (var _input in lineStrings)
        {
            Line line = new()
            {
                LineNumber = _lines.Count + 1,
                Tokens =  new(),
            };
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
                            newToken.Word = ParseNumber(i, _input);
                            i += newToken.Word.Length-1;
                            newToken.Value = double.Parse(newToken.Word);
                        }
                        else if(char.IsLetter(c))
                        {
                            newToken.Type = TokenType.String;
                            newToken.Word = ParseWord(i, _input);
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
                    line.Tokens.Add(newToken);
                }
            }
            _lines.Add(line);
        }
        return failed ? 1 : 0;
    }

     private string ParseNumber(int index, string line)
    {
        var numberString = line[index].ToString();
        bool isDecimal = false;
        while (index+1 < line.Length)
        {
            if (Char.IsDigit(line[index+1]))
            {
                numberString += line[++index];    
            }
            else if (!isDecimal && line[index+1] == '.' && index+2 < line.Length && char.IsDigit(line[index+2]))
            {
                //if '.' ensure there's not already '.' and at least one digit afterwards
                isDecimal = true;
                numberString += line[++index];
            }
            else
            {
                break;
            }
        }
        return numberString;
    }
    private string ParseWord(int index, string line)
    {
        var wordString = line[index].ToString();
        while (index+1 < line.Length)
        {
            if (Char.IsLetter(line[index+1]))
            {
                wordString += line[++index];    
            }
            else
            {
                break;
            }
        }
        return wordString.ToLower();
    }

}