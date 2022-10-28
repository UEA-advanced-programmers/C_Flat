using C_Flat_Interpreter.Common;
using C_Flat_Interpreter.Common.Enums;
using Microsoft.Extensions.Logging;

namespace C_Flat_Interpreter.Execution;

public class Execution : InterpreterLogger
{

    private readonly ILogger _logger;
    public Execution()
    {
        _logger = GetLogger("Execution");
    }

    public List<Token> Tokens = new();

    private readonly Stack<TokenType> _opStack = new Stack<TokenType>();
    private readonly Stack<int> _outStack = new Stack<int>();
    private int _op1, _op2;
    private TokenType _opType;

    public int ShuntYard()
    {
        foreach (var token in Tokens)
        {
            switch (token.Type)
            {
                case TokenType.Num:
                    _outStack.Push((int) (token.Value ?? throw new ArgumentNullException(nameof(token.Value), "Number token value null!"))); //Put numbers straight into out stack
                    _logger.LogInformation("Pushed {output} to output stack", _outStack.Peek());
                    break;
                case TokenType.Add:
                case TokenType.Sub:
                    while (_opStack.Count > 0 && _opStack.Peek() != TokenType.LeftParen) //assert stack is not empty and top is not (
                        Evaluate();
                    _opStack.Push(token.Type); //push + to op stack
                    _logger.LogInformation("Pushed {operator} to operator stack", _opStack.Peek());
                    break;
                case TokenType.Multi:
                case TokenType.Divide:
                    while (_opStack.Count > 0 && (_opStack.Peek() != TokenType.LeftParen && _opStack.Peek() != TokenType.Add && _opStack.Peek() != TokenType.Sub)) //assert stack is not empty and top is not ( or + or -
                        Evaluate();
                    _opStack.Push(token.Type); //push * to op stack
                    _logger.LogInformation("Pushed {operator} to operator stack", _opStack.Peek());
                    break;
                case TokenType.LeftParen:
                    _opStack.Push(TokenType.LeftParen); //push ( to op stack
                    _logger.LogInformation("Pushed {operator} to operator stack", _opStack.Peek());
                    break;
                case TokenType.RightParen:
                    while (_opStack.Count > 0 && _opStack.Peek() != TokenType.LeftParen) //assert stack is not empty and top is not (
                        Evaluate();
                    if (_opStack.Peek() == TokenType.LeftParen) //check if top of stack is ( and pop it
                        _opStack.Pop();
                    break;
            }
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
            case TokenType.Add: //if plus add op1 and op2
                _logger.LogInformation("Adding {op1} and {op2}", _op1, _op2);
                _outStack.Push(_op1 + _op2);
                break;
            case TokenType.Sub: //if minus subtract op1 from op2
                _logger.LogInformation("Subtracting {op1} from {op2}", _op1, _op2);
                _outStack.Push(_op2 - _op1);
                break;
            case TokenType.Multi: //if star multiply op1 and op2
                _logger.LogInformation("Multiplying {op1} and {op2}", _op1, _op2);
                _outStack.Push(_op1 * _op2);
                break;
            case TokenType.Divide: //if divide, divide op2 by op1
                _logger.LogInformation("Dividing {op2} by {op1}", _op2, _op1);
                _outStack.Push(_op2 / _op1);
                break;
            default:
                _logger.LogError("Unknown Operator, throwing exception");
                throw new ArgumentException($"{_opType.ToString()} is not a valid operator to evaluate");
        }
        _logger.LogInformation("Pushed result {result} to output stack \n", _outStack.Peek());
    }
}