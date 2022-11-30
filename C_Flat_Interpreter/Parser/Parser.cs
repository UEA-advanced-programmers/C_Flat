/*
 *	Statements:
 *	<Statement>::=<Expression> | <Logic-Statement> | <Conditional> | <While>

 *	Numerical expressions:
	 *  <Expression>::= <Term> {('+'|'-') <Term>}
	 *	<Term>::= <Value> {('*'|'/') <Value>}
	 *	<Value>::= '('<Expression>')' | <Number> | '-'<Value>

 *	Logical expressions:
	 *	<Logic-Statement>::= <Boolean> {<Condition>}
	 *  <Condition>::= ('==' | '&' | '|') <Boolean>
	 *	<Boolean>::= '!’<Logic-Statement> | 'true' | 'false' | <Expression-Query> | '('<Logic-Statement>')'
	 *	<Expression-Query> ::= <Expression> ('=='|'>'|'<') <Expression>
 *	Conditional expressions:
	*	<Conditional>::= 'if(' <Logic-Statement> ')' <block> {'else'}
	*	<While>::= 'while(' <Logic-Statement>')'<block>
	*	<block>::= '{' {<Statement>} '}'
 * */

using System.Data;
using C_Flat_Interpreter.Common;
using C_Flat_Interpreter.Common.Enums;

namespace C_Flat_Interpreter.Parser;

public class Parser : InterpreterLogger
{
	private TokenType _tokenType;
	private int _currentIndex;
	private int _totalTokens;
	private List<Token> _tokens; //TODO - define max with the group
	private List<ParseNode> _parseTree = new();

	private delegate void Delegate(ParseNode node);

	//constructor
	public Parser()
	{
		GetLogger("Parser");
		_tokens = new List<Token>();
	}

	public List<ParseNode> GetParseTree()
	{
		return _parseTree;
	}

	//Helper Functions
    #region Helper Functions
	private bool CheckBoolLiteral()
	{
		var word = _tokens[_currentIndex].Word;
		if (word.Equals("true") || word.Equals("false")) return true;
		_logger.Error($"Bool parse error! Expected boolean literal, actual: \"{word}\"");
		return false;
	}
	private bool CheckIf()
    {
		var word = _tokens[_currentIndex].Word;
		if (word.Equals("if")) return true;
		_logger.Error($"If parse error! Expected if, actual: \"{word}\"");
		return false;
	}
	private bool CheckElse()
	{
		var word = _tokens[_currentIndex].Word;
		if (word.Equals("else")) return true;
		_logger.Error($"Else parse error! Expected else, actual: \"{word}\"");
		return false;
	}
	private bool CheckWhile()
	{
		var word = _tokens[_currentIndex].Word;
		if (word.Equals("while")) return true;
		_logger.Error($"While parse error! Expected while, actual: \"{word}\"");
		return false;
	}
	private bool Match(TokenType tokenType)
	{
		if (_tokenType == TokenType.Null) //only on first call,  TODO - find better way to do this that means we don't need to set to null and/or we don't need this check
        {
			_tokenType = _tokens[_currentIndex].Type;
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
	private void Set(int index)
    {
		_currentIndex = index;
		//TODO - Change to index token type?
		_tokenType = _tokens[_currentIndex].Type;
    }
	private void Reset()
	{
		_currentIndex = 0;
		_tokenType = TokenType.Null;
	}
	private void Advance()
	{
		if (++_currentIndex >= _totalTokens) return; //todo - might be able to find a way to exit everything else quicker when we're at the end - EOF token!
		_tokenType = _tokens[_currentIndex].Type;
	}
    #endregion

    private ParseNode CreateNode(NodeType type, Delegate func)
    {
	    ParseNode newNode = new ParseNode(type);
	    func(newNode);
	    return newNode;
    }

    //End Helper Functions

	public int Parse(List<Token> tokens)
	{
		//TODO - recreate parse tree
		_tokens = tokens;
		_totalTokens = tokens.Count;
		_parseTree = new();
		Reset();
		
		//TODO - investigate better way to do this
		try
		{
			while (_currentIndex < _totalTokens)
			{
				ParseNode statementNode = new ParseNode(NodeType.Statement);
				Statement(statementNode);
				_parseTree.Add(statementNode);
			}
		}
		catch (Exception e)
		{
			_logger.Warning(e.Message);
		}
		
		if (_currentIndex >= _totalTokens) return 0;
		_logger.Error("Syntax error! Could not parse Token: {@token}", _tokens[_currentIndex]); //todo - Create test for this!
		return 1;
	}
	
	//EBNF Functions
	
	private void Statement(ParseNode node)
	{
		int currentIndex = _currentIndex;
		//Remove expression try-catch when variables added
		try
		{
			ParseNode expressionNode = new ParseNode(NodeType.Expression);
			Expression(expressionNode);

			if (_currentIndex == _totalTokens)
			{
				node.AddChild(expressionNode);
			}
		}
		catch (Exception e)
		{
			_logger.Warning(e.Message);
		}
		if (_currentIndex >= _totalTokens) return; //todo - check if this is redundant
		Set(currentIndex);
		
		try
		{
			node.AddChild(CreateNode(NodeType.Conditional, IfStatements));
			currentIndex = _currentIndex;
			return;
		}
		catch (Exception e)
		{
			_logger.Warning(e.Message);
			Set(currentIndex);
		}
		
		try
		{
			node.AddChild(CreateNode(NodeType.WhileStatement, WhileStatement));
			currentIndex = _currentIndex;
			return;
		}
		catch (Exception e)
		{
			_logger.Warning(e.Message);
			Set(currentIndex);
		}


		try
		{
			node.AddChild(CreateNode(NodeType.DeclareVariable, DeclareVariable));
			currentIndex = _currentIndex;
		}
		catch (Exception e)
		{
			_logger.Warning(e.Message);
			Set(currentIndex);
		}
		try
		{
			node.AddChild(CreateNode(NodeType.VarAssignment, VarAssignment));
			currentIndex = _currentIndex;
		}
		catch (Exception e)
		{
			_logger.Warning(e.Message);
			Set(currentIndex);
		}



		//TODO - Wrap this in a try catch and throw another exception in catch
		try
		{
			node.AddChild(CreateNode(NodeType.LogicStatement, LogicStatement));
			currentIndex = _currentIndex;
		}
		catch (Exception e)
		{
			_logger.Warning(e.Message);
			Set(currentIndex);
			throw new Exception("Syntax error! invalid statement");
		}
		

	}
	#region Variables
	private bool CheckVarLiteral()
	{
		var word = _tokens[_currentIndex].Word;
		if (word.Equals("var"))
		{
			return true;
		}
		_logger.Error($"Var parse error! Expected variable literal, actual: \"{word}\"");
		return false;
	}
	private void DeclareVariable(ParseNode node)
	{
		//_logger.Information("DeclareVariable() called at level {level}", level);
		// check for String token with the name "var"
		if (Match(TokenType.String) && CheckVarLiteral())
		{
			node.AddChild(new ParseNode(NodeType.Terminal, _tokens[_currentIndex]));
			Advance();
		}
		else
		{
			_logger.Error("Syntax Error! variable is not declared, expected \"var\" actual: {@word} ", _tokens[_currentIndex].Word);
			return;
		}
		int Rein = _currentIndex;
		// check if a value is being assigned
		try
		{
			node.AddChild(CreateNode(NodeType.VarAssignment, VarAssignment));
			return;
		}
		catch (Exception e)
		{
			Set(Rein);
			_logger.Warning(e.Message);
		}
		try
		{
			node.AddChild(CreateNode(NodeType.VarIdentifier, VarIdentifier));
			return;
		}
		catch (Exception e)
		{
			_logger.Warning(e.Message);
		}

	}

	private void VarIdentifier(ParseNode node)
	{
		if (Match(TokenType.String))
		{
			node.AddChild(new ParseNode(NodeType.Terminal, _tokens[_currentIndex]));
			Advance();
		}
		else
		{
			_logger.Error("Syntax Error! variable does not have a name, expected \"X\" actual: {@word} ", _tokens[_currentIndex].Word);
			return;
		}
		if (Match(TokenType.SemiColon))
		{
			node.AddChild(new ParseNode(NodeType.Terminal, _tokens[_currentIndex]));
			Advance();
		}
		else
		{
			_logger.Error("Syntax Error! declaration is not closed, expected \";\" actual: {@word} ", _tokens[_currentIndex].Word);
			return;
		}
	}
	private void VarAssignment(ParseNode node)
	{
		// check for String token (variable name)
		if (Match(TokenType.String))
		{
			node.AddChild(new ParseNode(NodeType.Terminal, _tokens[_currentIndex]));
			Advance();
		}
		else
		{
			_logger.Error("Syntax Error! variable does not have a name, expected \"X\" actual: {@word} ", _tokens[_currentIndex].Word);
			return;
		}
		try
		{
			if (Match(TokenType.Assignment))
			{
				node.AddChild(new ParseNode(NodeType.Terminal, _tokens[_currentIndex]));
				Advance();
			}
			try
			{
				node.AddChild(CreateNode(NodeType.Expression, Expression));
			}
			catch (Exception e)
			{
				_logger.Warning(e.Message);
			}
		}

		catch (Exception e)
		{
			_logger.Warning(e.Message);
		}
		if (Match(TokenType.SemiColon))
		{
			node.AddChild(new ParseNode(NodeType.Terminal, _tokens[_currentIndex]));
			Advance();
		}
		else
		{
			_logger.Error("Syntax Error! declaration is not closed, expected \";\" actual: {@word} ", _tokens[_currentIndex].Word);
			return;
		}
	}
	#endregion
	#region Expressions
	private void Expression(ParseNode node)
	{
		node.AddChild(CreateNode(NodeType.Term, Term));
		if (!Match(TokenType.Add) && !Match(TokenType.Sub)) return;
		node.AddChild(new ParseNode(NodeType.Terminal, _tokens[_currentIndex]));
		Advance();
		
		node.AddChild(CreateNode(NodeType.Term, Term));
	}

	private void Term(ParseNode node)
	{
		node.AddChild(CreateNode(NodeType.Factor, Factor));

		if (!Match(TokenType.Multi) && !Match(TokenType.Divide)) return;
		node.AddChild(new ParseNode(NodeType.Terminal, _tokens[_currentIndex]));
		Advance();
		
		node.AddChild(CreateNode(NodeType.Factor, Factor));
	}

	private void Factor(ParseNode node)
	{
		if (Match(TokenType.Num))
		{
			node.AddChild(new ParseNode(NodeType.Terminal, _tokens[_currentIndex]));
			Advance();
		}
		else if (Match(TokenType.Sub))
		{
			node.AddChild(new ParseNode(NodeType.Terminal, _tokens[_currentIndex]));
			Advance();
			
			node.AddChild(CreateNode(NodeType.Factor, Factor));
		}
		else if (Match(TokenType.LeftParen))
		{
			node.AddChild(new ParseNode(NodeType.Terminal, _tokens[_currentIndex]));
			Advance();

			node.AddChild(CreateNode(NodeType.Expression, Expression));

			if (Match(TokenType.RightParen))
			{
				node.AddChild(new ParseNode(NodeType.Terminal, _tokens[_currentIndex]));
				Advance();
			}
			else
			{
				throw new SyntaxErrorException("Syntax Error! Mismatched parentheses at token " + _currentIndex);
			}
		}
		else
		{
			throw new SyntaxErrorException("Syntax Error! Unexpected token at token " + _currentIndex);
		}
	}
    #endregion

    #region Boolean expressions
	private void LogicStatement(ParseNode node)
	{
		node.AddChild(CreateNode(NodeType.Boolean, Boolean));

		
		//TODO - Investigate and remove early out
		if (_currentIndex >= _totalTokens) return;
		node.AddChild(CreateNode(NodeType.Conditional, Condition));
	}

	private void Boolean(ParseNode node)
	{
		if (Match(TokenType.Not))
		{
			node.AddChild(new ParseNode(NodeType.Terminal, _tokens[_currentIndex]));
			Advance();
			
			node.AddChild(CreateNode(NodeType.LogicStatement, LogicStatement));
		}
		else if (Match(TokenType.String))
		{
			if (CheckBoolLiteral())
			{
				node.AddChild(new ParseNode(NodeType.Terminal, _tokens[_currentIndex]));
				Advance();
			}
			else
			{
				throw new SyntaxErrorException("Syntax Error! Unexpected Boolean Literal at token " + _currentIndex);
			}
		}
		else
		{
			var index = _currentIndex;
			try
			{
				node.AddChild(CreateNode(NodeType.ExpressionQuery, ExpressionQuery));
			}
			catch (Exception e)
			{
				_logger.Warning(e.Message);
				_currentIndex = index;

				if (Match(TokenType.LeftParen))
				{
					node.AddChild(new ParseNode(NodeType.Terminal, _tokens[_currentIndex]));
					Advance();
					
					node.AddChild(CreateNode(NodeType.LogicStatement, LogicStatement));

					if (Match(TokenType.RightParen))
					{
						node.AddChild(new ParseNode(NodeType.Terminal, _tokens[_currentIndex]));
						Advance();
					}
					else
					{
						_logger.Error("Syntax Error! Mismatched parentheses at token {index}", _currentIndex);
					}
				}
				else
				{
					throw new SyntaxErrorException("Syntax Error! Unexpected token at token " + _currentIndex); //todo - check error here!
				}
			}
		}
	}

	private void Condition(ParseNode node)
	{
		if (!Match(TokenType.NotEqual) && !Match(TokenType.Equals) && !Match(TokenType.And) &&
		    !Match(TokenType.Or)) return;
		
		node.AddChild(new ParseNode(NodeType.Terminal, _tokens[_currentIndex]));
		Advance();
		node.AddChild(CreateNode(NodeType.Boolean, Boolean)); //todo - only assign if this works

	}

	private void ExpressionQuery(ParseNode node)
	{
		node.AddChild(CreateNode(NodeType.Expression, Expression));
		
		if (!Match(TokenType.NotEqual) && !Match(TokenType.Equals) && !Match(TokenType.More) &&
		    !Match(TokenType.Less)) throw new SyntaxErrorException("Syntax Error! Unexpected token at token " + _currentIndex);
		node.AddChild(new ParseNode(NodeType.Terminal, _tokens[_currentIndex]));
		Advance();
		node.AddChild(CreateNode(NodeType.Expression, Expression));
	}
    #endregion
    #region Condition-Statements and iterators
    
    //TODO - Rename to match EBNF
	private void IfStatements(ParseNode node)
    {
	    if (Match(TokenType.String) && CheckIf())
		{
			node.AddChild(new ParseNode(NodeType.Terminal, _tokens[_currentIndex]));
			Advance();
		}
		else {
			_logger.Error("Syntax Error! Expected \"if\" actual: {@word} ", _tokens[_currentIndex].Word);
			throw new SyntaxErrorException("Syntax Error! Expected \"if\" at token " + _currentIndex);
		}

		if (Match(TokenType.LeftParen))
		{
			node.AddChild(new ParseNode(NodeType.Terminal, _tokens[_currentIndex]));
			Advance();
		}
		else
        {
            _logger.Error("Syntax Error! Expected \"(\" actual: {@word} ", _tokens[_currentIndex].Word);
			throw new SyntaxErrorException("Syntax Error! Expected \"(\" at token " + _currentIndex);
        }

		node.AddChild(CreateNode(NodeType.LogicStatement, LogicStatement));
		
		if (Match(TokenType.RightParen))
		{
			node.AddChild(new ParseNode(NodeType.Terminal, _tokens[_currentIndex]));
			Advance();
		}
		else
        {
			_logger.Error("Syntax Error! Expected \"(\" actual: {@word} ", _tokens[_currentIndex].Word);
			throw new SyntaxErrorException("Syntax Error! Expected \")\" at token " + _currentIndex);
		}

		node.AddChild(CreateNode(NodeType.Block, Block));
		
		try
		{
			if (Match(TokenType.String) && CheckElse())
			{
				node.AddChild(new ParseNode(NodeType.Terminal, _tokens[_currentIndex]));
				Advance();
				
				node.AddChild(CreateNode(NodeType.Block, Block));
			}
			else
			{
				_logger.Error("Syntax Error! Expected \"else\" actual: {@word} ", _tokens[_currentIndex].Word);
			}
		}
		catch (Exception e)
		{
			_logger.Warning(e.Message);
		}
	}

	private void WhileStatement(ParseNode node)
	{
		if (Match(TokenType.String) && CheckWhile())
		{
			node.AddChild(new ParseNode(NodeType.Terminal, _tokens[_currentIndex]));
			Advance();
		}
		else
		{
			_logger.Error("Syntax Error! Expected \"while\" actual: {@word} ", _tokens[_currentIndex].Word);
			throw new SyntaxErrorException("Syntax Error! Expected \"while\" at token " + _currentIndex);
		}

		if (Match(TokenType.LeftParen))
		{
			node.AddChild(new ParseNode(NodeType.Terminal, _tokens[_currentIndex]));
			Advance();
		}
		else
		{
			_logger.Error("Syntax Error! Expected \"(\" actual: {@word} ", _tokens[_currentIndex].Word);
			throw new SyntaxErrorException("Syntax Error! Expected \"(\" at token " + _currentIndex);
		}
		
		node.AddChild(CreateNode(NodeType.LogicStatement, LogicStatement));
		
		if (Match(TokenType.RightParen))
		{
			node.AddChild(new ParseNode(NodeType.Terminal, _tokens[_currentIndex]));
			Advance();
		}
		else
		{
			_logger.Error("Syntax Error! Expected \"(\" actual: {@word} ", _tokens[_currentIndex].Word);
			throw new SyntaxErrorException("Syntax Error! Expected \")\" at token " + _currentIndex);
		}

		node.AddChild(CreateNode(NodeType.Block, Block));
	}
	
	private void Block(ParseNode node) {
		if (Match(TokenType.LeftCurlyBrace))
		{
			node.AddChild(new ParseNode(NodeType.Terminal, _tokens[_currentIndex]));
			Advance();
		}
		else
		{
			_logger.Error("Syntax Error! Expected \"{\" actual: {@word} ", _tokens[_currentIndex].Word);
			throw new SyntaxErrorException("Syntax Error! Expected \"{\" at token " + _currentIndex);
		}
		
		try
		{
			if (!Match(TokenType.RightCurlyBrace)) return;
			node.AddChild(new ParseNode(NodeType.Terminal, _tokens[_currentIndex]));
			Advance();
		}
		catch (Exception e)
		{
			_logger.Warning(e.Message);
		}
		
		//TODO - For now, blocks cannot contain anything, once variables have been added we can change this

		/* //TODO - Add back in once variables are added
		if (Match(TokenType.RightCurlyBrace))
		{
			ParseNode newNode = new ParseNode(NodeType.Terminal, _tokens[_currentIndex]);
			Advance(newNode);
			node.assignChild(newNode);
		}
		else
		{
			_logger.Error("Syntax Error! Expected \"}\" actual: {@word} ", _tokens[_currentIndex].Word);
			throw new SyntaxErrorException("Syntax Error! Expected \"}\" at token " + _currentIndex);
		}
		*/
	}

	#endregion
	//EBNF Functions
}
