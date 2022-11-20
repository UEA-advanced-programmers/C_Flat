/*
 *	Statements:
 *	<Statement>::=<Expression> | <Logic-Statement>

 *	Numerical expressions:
	 *  <Expression>::= <Term> {('+'|'-') <Term>}
	 *	<Term>::= <Value> {('*'|'/') <Value>}
	 *	<Value>::= '('<Expression>')' | <Number> | '-'<Value>

 *	Logical expressions:
	 *	<Logic-Statement>::= <Boolean> {<Condition>}
	 *  <Condition>::= ('==' | '&' | '|') <Boolean>
	 *	<Boolean>::= '!’<Logic-Statement> | 'true' | 'false' | <Expression-Query> | '('<Logic-Statement>')'
	 *	<Expression-Query> ::= <Expression> ('=='|'>'|'<') <Expression>
 
 * */

using System.Data;
using C_Flat_Interpreter.Common;
using C_Flat_Interpreter.Common.Enums;

namespace C_Flat_Interpreter.Parser;

public class Parser : InterpreterLogger
{
	private TokenType _tokenType;
	private int _currentIndex;
	private ParseNode currentNode;
	private List<ParseNode> ParseTree; //TODO - define max with the group
	
	//TODO: Move to class with overload for ToString() for better error logging (see refactor logging)
	struct ParseNode
	{
		public int nodeIndex;
		public List<KeyValuePair<int, Token>> tokens;

		public ParseNode(int i, List<KeyValuePair<int, Token>> nodeTokens)
		{
			nodeIndex = i;
			tokens = nodeTokens;
		}
	}
	//constructor
	public Parser()
	{
		GetLogger("Parser");
	}

	//Helper Functions
	
	private bool CheckBoolLiteral()
	{
		var word = currentNode.tokens[_currentIndex].Value.Word;
		if (word.Equals("true") || word.Equals("false")) return true;
		_logger.Error($"Bool parse error! Expected boolean literal, actual: \"{word}\"");
		return false;
	}
	private bool Match(TokenType tokenType)
	{
		if (_tokenType == TokenType.Null) //only on first call,  TODO - find better way to do this that means we don't need to set to null and/or we don't need this check
        {
			_tokenType = currentNode.tokens[_currentIndex].Value.Type;
		}

		if (tokenType == _tokenType)
		{
			_logger.Information("Token at index {index} matches {tokenType}", _currentIndex, tokenType);
		}
		else
		{
			_logger.Information("Token at index {index} does NOT MATCH {tokenType}", _currentIndex, tokenType);
			return false;
		}
		return true;
	}

	private void Reset()
	{
		_currentIndex = 0;
		_tokenType = TokenType.Null;
	}
	private void Advance(int level)
	{
		if (++_currentIndex >= currentNode.tokens.Count) return; //todo - might be able to find a way to exit everything else quicker when we're at the end - EOF token!
		_tokenType = currentNode.tokens[_currentIndex].Value.Type;
		_logger.Information("advance() called at level {level}. Next token is {@token}", level, currentNode.tokens[_currentIndex]);
	}

	private void ConstructParseTree(List<Line> lines)
	{
		ParseTree = new();
		int i = 0;
		int openBlocks = 0;
		List<KeyValuePair<int, Token>> nodeTokens = new();
		foreach (var line in lines)
		{
			//ignore empty lines
			if(line.Tokens.Count == 0)
				continue;
			foreach (var tok in line.Tokens)
			{
				nodeTokens.Add(new(line.LineNumber, tok));
				//TODO: Check token type not word when these tokens are added!
				if(tok.Word is "{")
					openBlocks++;
				else if (tok.Word is "}")
					openBlocks--;
				if (openBlocks <= 0 && tok.Word is "}" or ";")
				{
					//Terminal token signalling a new node
					ParseTree.Add(new ParseNode(i++, nodeTokens));
					nodeTokens = new();
				}
			}
			//REMOVE BELOW LINES WHICH ALLOWS FOR NON TERMINATED EXPRESSIONS E.G. 1+1
			ParseTree.Add(new ParseNode(i++, nodeTokens));
			nodeTokens = new();
		}
	}
	
	
	//End Helper Functions

	public int Parse(List<Line> lines)
	{
		ConstructParseTree(lines);
		bool fail = false;
		foreach (var parseNode in ParseTree)
		{
			currentNode = parseNode;
			Statement(0);
			if (_currentIndex < currentNode.tokens.Count)
			{
				_logger.Error("Syntax error! Could not parse node: {@node}", currentNode);
				fail = true;
			}
			Reset();
		}
		return fail? 1 : 0;
	}
	
	//EBNF Functions

	private void Statement(int level)
	{
		_logger.Information( "Statement() called at level {level}", level);
		try
		{
			Expression(level + 1);
		}
		catch (Exception e)
		{
			_logger.Warning(e.Message);
		}
		if (_currentIndex >= currentNode.tokens.Count) return;
		Reset();
		LogicStatement(level + 1);
	}
	private void Expression(int level)
	{
		_logger.Information( "expression() called at level {level}", level);
		Term(level + 1);
		if (!Match(TokenType.Add) && !Match(TokenType.Sub)) return; //this is terminal
		Advance(level + 1);
		Term(level + 1);
	}

	private void Term(int level)
	{
		_logger.Information( "term() called at level {level} ", level);
		Factor(level + 1);
		if (!Match(TokenType.Multi) && !Match(TokenType.Divide)) return; //this is terminal
		Advance(level + 1);
		Factor(level + 1);
	}

	private void Factor(int level) //todo - see if factor prime is needed
	{
		_logger.Information( "factor() called at level {level}", level);
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
				throw new SyntaxErrorException("Syntax Error! Mismatched parentheses at line: "+ currentNode.tokens[_currentIndex-1].Key);
			}
		}
		else
		{
			throw new SyntaxErrorException("Syntax Error! Unexpected token at line: " + currentNode.tokens[_currentIndex].Key);
		}
	}

	private void LogicStatement(int level)
	{
		_logger.Information( "LogicStatement() called at level {level}", level);
		Boolean(level+1);
		Condition(level + 1);
	}

	private void Boolean(int level)
	{
		_logger.Information( "Boolean() called at level {level}", level);

		if (Match(TokenType.Not))
		{
			Advance(level+1);
			LogicStatement(level+1);
		}
		else if (Match(TokenType.String))
		{
			if(CheckBoolLiteral())
				Advance(level+1);
		}
		else
		{
			var index = _currentIndex;
			try
			{
				ExpressionQuery(level+1);
			}
			catch (Exception e)
			{
				_logger.Warning(e.Message);
				
				_currentIndex = index;
				_tokenType = TokenType.Null;
				if (Match(TokenType.LeftParen))
				{
					Advance(level + 1);
					LogicStatement(level + 1);
					if (Match(TokenType.RightParen)) Advance(level + 1);
					else
					{
						_logger.Error("Syntax Error! Mismatched parentheses at line: {line}", currentNode.tokens[_currentIndex-1].Key);
					}
				}
			}
		}
	}

	private void Condition(int level)
	{
		_logger.Information( "Condition() called at level {level}", level);

		if (!(Match(TokenType.Equals) || Match(TokenType.And) || Match(TokenType.Or) || Match(TokenType.Not)))
			return;
		if (Match(TokenType.Not))
		{
			Advance(level);
			if (!Match(TokenType.Equals))
			{
				_logger.Error("Syntax Error! Mismatched inequality operator at line: {line}. Expected \"=\" actual: {@word} ", 
					currentNode.tokens[_currentIndex].Key, 
					currentNode.tokens[_currentIndex].Value.Word);
				return;
			}
		}
		else if (Match(TokenType.Equals))
		{
			Advance(level);
			if (!Match(TokenType.Equals))
			{
				_logger.Error("Syntax Error! Mismatched equality operator at line: {line}. Expected \"=\" actual: {@word} ",
					currentNode.tokens[_currentIndex].Key,
					currentNode.tokens[_currentIndex].Value.Word);
				return;
			}
		}
		Advance(level+1);
		Boolean(level+1);
	}

	private void ExpressionQuery(int level)
	{
		_logger.Information( "ExpressionQuery() called at level {level}", level);
		Expression(level + 1);
		if (!(Match(TokenType.Equals) || Match(TokenType.More) || Match(TokenType.Less)))
			return;
		if (Match(TokenType.Not))
		{
			Advance(level);
			if (!Match(TokenType.Equals))
			{
				_logger.Error("Syntax Error! Mismatched inequality operator at line: {line}. Expected \"=\" actual: {@word} ",
					currentNode.tokens[_currentIndex].Key,
					currentNode.tokens[_currentIndex].Value.Word);
				return;
			}
		}
		else if (Match(TokenType.Equals))
		{
			Advance(level);
			if (!Match(TokenType.Equals))
			{
				_logger.Error("Syntax Error! Mismatched equality operator, at line: {line}. Expected \"=\" actual: {@word} ",
					currentNode.tokens[_currentIndex].Key,
					currentNode.tokens[_currentIndex].Value.Word);
				return;
			}
		}
		Advance(level + 1);
		Expression(level + 1);
	}
	//EBNF Functions
}
