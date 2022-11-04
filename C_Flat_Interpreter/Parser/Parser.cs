/*
 *  <Expression>::= <Term> {('+'|'-') <Term>}
 *	<Term>::= <Value> {('*'|'/') <Value>}
 *	<Value>::= '('<Expression>')' | <Number> | '-'<Value>
 *	<Number>::= <Digit>*
 *	<Digit> ::= #'[0-9]'
 * */

using System.Data;
using C_Flat_Interpreter.Common;
using C_Flat_Interpreter.Common.Enums;
using Microsoft.Extensions.Logging;

namespace C_Flat_Interpreter.Parser;

public class Parser : InterpreterLogger
{
	private TokenType _tokenType;
	private int _currentIndex;
	private int _totalTokens;
	private List<Token> _tokens; //TODO - define max with the group
	private readonly ILogger _logger;

	//constructor
	public Parser()
	{
		_logger = GetLogger("Parser");
	}

	//Helper Functions
	
	private bool Match(TokenType tokenType)
	{
		if (_tokenType == TokenType.Null) //only on first call,  TODO - find better way to do this that means we don't need to set to null and/or we don't need this check
        {
			_tokenType = _tokens[_currentIndex].Type;
		}

		if (tokenType == _tokenType)
		{
			_logger.LogInformation("Token[" + _currentIndex + "] matches {tokenType}", tokenType);
		}
		else
		{
			_logger.LogInformation("Token[" + _currentIndex + "] does NOT MATCH {tokenType}", tokenType);
			return false;
		}
		return true;
	}

	private void Advance(int level)
	{
		if (++_currentIndex == _totalTokens) return; //todo - might be able to find a way to exit everything else quicker when we're at the end - EOF token!
		_tokenType = _tokens[_currentIndex].Type;
		_logger.LogInformation("advance() called at level {level}. Next token is {tokenType}", level, _tokenType);
	}
	
	//End Helper Functions

	public List<Token> Parse(List<Token> tokens)
	{
		_tokens = tokens;
		_totalTokens = tokens.Count;
		_currentIndex = 0;
		_tokenType = TokenType.Null;
		Expression(0);
		if (_currentIndex >= _totalTokens) return _tokens;
		throw new SyntaxErrorException("SYNTAX ERROR - token "+ _currentIndex+ " {current} is of type " +
		                                            _tokens[_currentIndex].Type); //todo - Create test for this!
	}
	
	//EBNF Functions

	private void Expression(int level)
	{
		_logger.LogInformation( "expression() called at level {level}", level);
		Term(level + 1);
		expression_p(level + 1);
	}

	private void expression_p(int level)
	{
		while (true)
		{
			_logger.LogInformation("expression_p() called at level {level}", level);
			if (!Match(TokenType.Add) && !Match(TokenType.Sub)) return; //this is terminal
			Advance(level + 1);
			Term(level + 1);
			level = level + 1;
		}
	}
	
	private void Term(int level)
	{
		_logger.LogInformation( "term() called at level {level} ", level);
		Factor(level + 1);
		term_p(level + 1);
	}

	private void term_p(int level)
	{
		_logger.LogInformation("term_p() called at level {level}", level);
		if (!Match(TokenType.Multi) && !Match(TokenType.Divide)) return; //this is terminal
		Advance(level + 1);
		Factor(level + 1);
		term_p(level + 1);
	}
	
	private void Factor(int level)
	{
		_logger.LogInformation( "factor() called at level {level}", level);
		if (Match(TokenType.Num))
		{
			Advance(level + 1);
		}
		else if (Match(TokenType.Sub))
		{
			Advance(level+1);
			Factor(level+1);
		}
		else if (Match(TokenType.LeftParen))
		{
			Advance(level + 1);
			Expression(level + 1);
			if (Match(TokenType.RightParen)) Advance(level + 1);
			else
			{
				throw new SyntaxErrorException("SYNTAX ERROR: Mismatched parentheses at token " + _currentIndex);
			}
		}
		else
		{
			throw new SyntaxErrorException("SYNTAX ERROR: Number expected at token " + _currentIndex);
		}
		//todo - include negative numbers
	}
	//EBNF Functions
}
