using C_Flat_Interpreter.Common;
using Microsoft.Extensions.Logging;

namespace C_Flat_Interpreter.Execution;

public class Execution : InterpreterLogger
{
    public enum TokenType //will be replaced by an enumerator created by the work on lex
    {
        PLUS,
        STAR,
        L_PAR,
        R_PAR,
        NUMBER,
    }
    //TODO: Remove temporary static variables when Lex class is implemented
    public struct Token
    {
        public Token(TokenType type, int value = 0)
        {
            this.type = type;
            this.value = value;
        }
        public TokenType type { get; set; }
        public int value { get; set; }
    }

    private readonly ILogger _logger;
    public Execution()
    {
        _logger = GetLogger("Execution");
    }

    public Token[] Tokens;

    private readonly Stack<TokenType> _opStack = new Stack<TokenType>();
    private readonly Stack<int> _outStack = new Stack<int>();
    private int _op1, _op2;
    private TokenType _opType;

    public int ShuntYard()
    {
        var count = 0;
        while (count < Tokens.Length)
        {
            switch (Tokens[count].type)
            {
                case TokenType.NUMBER:
                    _outStack.Push(Tokens[count].value); //Put numbers straight into out stack
                    _logger.LogInformation("Pushed {output} to operator stack", _outStack.Peek());
                    break;
                case TokenType.PLUS:
                    while (_opStack.Count > 0 && _opStack.Peek() != TokenType.L_PAR) //assert stack is not empty and top is not (
                        Evaluate();
                    _opStack.Push(TokenType.PLUS); //push + to op stack
                    _logger.LogInformation("Pushed {operator} to operator stack", _opStack.Peek());
                    break;
                case TokenType.STAR:
                    while (_opStack.Count > 0 && (_opStack.Peek() != TokenType.L_PAR && _opStack.Peek() != TokenType.PLUS)) //assert stack is not empty and top is not ( or +
                        Evaluate();
                    _opStack.Push(TokenType.STAR); //push * to op stack
                    _logger.LogInformation("Pushed {operator} to operator stack", _opStack.Peek());
                    break;
                case TokenType.L_PAR:
                    _opStack.Push(TokenType.L_PAR); //push ( to op stack
                    _logger.LogInformation("Pushed {operator} to operator stack", _opStack.Peek());
                    break;
                case TokenType.R_PAR:
                    while (_opStack.Count > 0 && _opStack.Peek() != TokenType.L_PAR) //assert stack is not empty and top is not (
                        Evaluate();
                    if (_opStack.Peek() == TokenType.L_PAR) //check if top of stack is ( and pop it
                        _opStack.Pop();
                    break;
            }
            count++;
        }
        while (_opStack.Count > 0) //evaluate until op stack is empty
            Evaluate();


        return _outStack.Pop(); //return final out stack value
    }

    private void Evaluate()
    {
        _opType = _opStack.Pop();
        _op1 = _outStack.Pop();
        _op2 = _outStack.Pop();
        switch (_opType) //perform evaluation based on operator
        {
            case TokenType.PLUS: //if plus add op1 and op2
                _logger.LogInformation("Adding {op1} and {op2}", _op1, _op2);
                _outStack.Push(_op1 + _op2);
                break;
            case TokenType.STAR: //if star multiply op1 and op2
                _logger.LogInformation("Multiplying {op1} and {op2}", _op1, _op2);
                _outStack.Push(_op1 * _op2);
                break;
        }
        _logger.LogInformation("Pushed result {result} to output stack \n", _outStack.Peek());
    }
}