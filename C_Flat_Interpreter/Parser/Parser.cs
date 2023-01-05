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
using C_Flat_Interpreter.Common.Exceptions;
using SyntaxErrorException = C_Flat_Interpreter.Common.Exceptions.SyntaxErrorException;

namespace C_Flat_Interpreter.Parser;

public class Parser : InterpreterLogger
{
	private TokenType _tokenType;
	private int _currentIndex;
	private int _currentLine = 1;
	private int _totalTokens;
	private List<Token> _tokens;
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
		var word = _tokens[_currentIndex].Word.Trim();
		return word is "true" or "false";
	}
	private bool CheckIf()
    {
		var word = _tokens[_currentIndex].Word.Trim();
		return word is "if";
    }
	private bool CheckElse()
	{
		var word = _tokens[_currentIndex].Word.Trim();
		return word is "else";

	}
	private bool CheckWhile()
	{
		var word = _tokens[_currentIndex].Word.Trim();
		return word is "while";
	}
	
	private bool CheckVarLiteral()
	{
		var word = _tokens[_currentIndex].Word.Trim();
		return word is "var";
	}
	
	private bool Match(TokenType tokenType)
	{
		//	Log matches as information and non-matches as debug.
		if (tokenType == _tokenType)
		{
			_logger.Information("Token at index {index} matches {tokenType}", _currentIndex, tokenType);
		}
		else
		{
			_logger.Debug("Token at index {index} does NOT MATCH {tokenType}", _currentIndex, tokenType);
			return false;
		}
		return true;
	}

	private void Reset(int index = 0)
    {
		_currentIndex = index;
		_tokenType = _tokens[_currentIndex].Type;
		_currentLine = _tokens[_currentIndex].Line +1;
    }
	
	private void Advance()
	{
		if (++_currentIndex >= _totalTokens) return; //todo - might be able to find a way to exit everything else quicker when we're at the end - EOF token!
		_tokenType = _tokens[_currentIndex].Type;
		_currentLine =_tokens[_currentIndex].Line +1;
	}
    #endregion

    //TODO - Investigate if this is correct use of delegates
    private ParseNode CreateNode(NodeType type, Delegate func)
    {
	    ParseNode newNode = new ParseNode(type);
	    func(newNode);
	    _logger.Information($"Successfully parsed {newNode.type}!");
	    return newNode;
    }

    //End Helper Functions

	public int Parse(List<Token> tokens)
	{
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
			_logger.Error(e.Message);
			return 1;
		}
		//If no exceptions are caught, return 0 to indicate success
		return 0;
	}
	
	//EBNF Functions
	private void Statement(ParseNode node)
	{
		int currentIndex = _currentIndex;
		
		try
		{
			node.AddChild(CreateNode(NodeType.ConditionalStatement, IfStatements));
			currentIndex = _currentIndex;
			return;
		}
		catch (InvalidSyntaxException e)
		{
			_logger.Debug(e.Message);
			Reset(currentIndex);
		}
		
		try
		{
			node.AddChild(CreateNode(NodeType.WhileStatement, WhileStatement));
			currentIndex = _currentIndex;
			return;
		}
		catch (InvalidSyntaxException e)
		{
			_logger.Debug(e.Message);
			Reset(currentIndex);
		}
		
		try
		{
			node.AddChild(CreateNode(NodeType.DeclareVariable, DeclareVariable));
			currentIndex = _currentIndex;
			return;
		}
		catch (InvalidSyntaxException e)
		{
			_logger.Debug(e.Message);
			Reset(currentIndex);
		}
		try
		{
			node.AddChild(CreateNode(NodeType.VarAssignment, VarAssignment));
			currentIndex = _currentIndex;
			return;
		}
		catch (InvalidSyntaxException e)
		{
			_logger.Debug(e.Message);
			Reset(currentIndex);
		}
		throw new SyntaxErrorException("Unable to parse statement!");
	}
	
	#region Variables
	//TODO - Rename to match ebnf (Declaration)
	private void DeclareVariable(ParseNode node)
	{
		// check for String token with the name "var"
		if (Match(TokenType.String) && CheckVarLiteral())
		{
			node.AddChild(new ParseNode(NodeType.Terminal, _tokens[_currentIndex]));
			Advance();
		}
		else
		{
			throw new InvalidSyntaxException($"Expected keyword 'var'. Actual: '{_tokens.ElementAtOrDefault(_currentIndex)}'", _currentLine);
		}
		
		int Rein = _currentIndex;
		// check if a value is being assigned
		try
		{
			node.AddChild(CreateNode(NodeType.VarAssignment, VarAssignment));
			return;
		}
		catch (InvalidSyntaxException e)
		{
			Reset(Rein);
			_logger.Warning(e.Message);
		}
		
		node.AddChild(CreateNode(NodeType.VarIdentifier, VarIdentifier));
		if (Match(TokenType.SemiColon))
		{
			node.AddChild(new ParseNode(NodeType.Terminal, _tokens[_currentIndex]));
			Advance();
		}
		else
		{
			throw new SyntaxErrorException($"Variable declaration is not terminated, expected ';'. Actual: '{_tokens.ElementAtOrDefault(_currentIndex)}'", _currentLine);
		}
	}
	//TODO - Rename to match ebnf (Identifier)
	private void VarIdentifier(ParseNode node)
	{
		if (Match(TokenType.String))
		{
			node.AddChild(new ParseNode(NodeType.Terminal, _tokens[_currentIndex]));
			Advance();
		}
		else
		{
			throw new SyntaxErrorException($"Invalid variable identifier! Expected token type 'string'. Actual: '{_tokenType}'", _currentLine);
		}
	}
	//TODO - Rename to match ebnf (Assignment)
	private void VarAssignment(ParseNode node)
	{
		// check for identifier
		node.AddChild(CreateNode(NodeType.VarIdentifier, VarIdentifier));

		if (!Match(TokenType.Assignment)) 
			throw new InvalidSyntaxException($"Invalid variable assignment, expected '='. Actual: '{_tokens.ElementAtOrDefault(_currentIndex)}'", _currentLine);
		
		node.AddChild(new ParseNode(NodeType.Terminal, _tokens[_currentIndex]));
		Advance();
		
		//TODO - Also try check for word, and logic statement
		try
		{
			node.AddChild(CreateNode(NodeType.Expression, Expression));
		}
		catch (InvalidSyntaxException e)
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
			throw new SyntaxErrorException($"Unterminated assignment, expected ';'. Actual: '{_tokens.ElementAtOrDefault(_currentIndex)}'", _currentLine);
		}
	}
	#endregion
	
	#region Expressions
	private void Expression(ParseNode node)
	{
		//	Try add first term node
		node.AddChild(CreateNode(NodeType.Term, Term));
		
		//	Check for '+' | '-' 
		if (!Match(TokenType.Add) && !Match(TokenType.Sub)) return;
		node.AddChild(new ParseNode(NodeType.Terminal, _tokens[_currentIndex]));
		Advance();
		
		//	Add second term node
		node.AddChild(CreateNode(NodeType.Term, Term));
	}

	private void Term(ParseNode node)
	{
		//	Add first factor node
		node.AddChild(CreateNode(NodeType.Factor, Factor));

		//	Check for '*' | '/' 
		if (!Match(TokenType.Multi) && !Match(TokenType.Divide)) return;
		node.AddChild(new ParseNode(NodeType.Terminal, _tokens[_currentIndex]));
		Advance();

		//	Add second factor node
		node.AddChild(CreateNode(NodeType.Factor, Factor));
	}

	private void Factor(ParseNode node)
	{
		// Parse factor or throw
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
				throw new SyntaxErrorException($"Mismatched parentheses, expected ')'. Actual: '{_tokens.ElementAtOrDefault(_currentIndex)}'", _currentLine);
			}
		}
		else
		{
			throw new InvalidSyntaxException("Unexpected token, failed to parse a factor!");
		}
	}
    #endregion

    #region Boolean expressions
	private void LogicStatement(ParseNode node)
	{
		//	Add Boolean node
		node.AddChild(CreateNode(NodeType.Boolean, Boolean));
		
		//	Try and parse a Condition
		try
		{
			node.AddChild(CreateNode(NodeType.Condition, Condition));
		}
		catch (InvalidSyntaxException e)
		{
			_logger.Warning(e.Message);
		}
	}

	private void Boolean(ParseNode node)
	{
		//	Check for a preceding '!' operator
		if (Match(TokenType.Not))
		{
			node.AddChild(new ParseNode(NodeType.Terminal, _tokens[_currentIndex]));
			Advance();
			//	Parse the proceeding logic statement throwing if it fails
			try
			{
				node.AddChild(CreateNode(NodeType.LogicStatement, LogicStatement));
			}
			catch (InvalidSyntaxException e)
			{
				_logger.Warning(e.Message);
				throw new SyntaxErrorException($"Invalid logic statement!");
			}
		}
		//	Otherwise check for a boolean literal
		else if (Match(TokenType.String))
		{
			if (CheckBoolLiteral())
			{
				node.AddChild(new ParseNode(NodeType.Terminal, _tokens[_currentIndex]));
				Advance();
			}
			else
			{
				throw new SyntaxErrorException($"Invalid Boolean literal, expected 'true' or 'false'. Actual: '{_tokens.ElementAtOrDefault(_currentIndex)}'", _currentLine);
			}
		}
		//	Otherwise check for expression query
		else
		{
			//	Temporarily save index
			var index = _currentIndex;
			try
			{
				node.AddChild(CreateNode(NodeType.ExpressionQuery, ExpressionQuery));
			}
			catch (InvalidSyntaxException e)
			{
				_logger.Warning(e.Message);
				Reset(index);
				
				//	Try and parse a parenthesised logic statement
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
						throw new SyntaxErrorException($"Mismatched parentheses, expected ')'. Actual: '{_tokens.ElementAtOrDefault(_currentIndex)}'", _currentLine);
					}
				}
				else
				{
					throw new InvalidSyntaxException("Unexpected token, unable to parse a boolean expression!");
				}
			}
		}
	}

	private void Condition(ParseNode node)
	{
		//	Throw exception if token doesn't match any operators
		if (!Match(TokenType.NotEqual) && !Match(TokenType.Equals) && !Match(TokenType.And) &&
		    !Match(TokenType.Or)) throw new InvalidSyntaxException("Unexpected token, unable to parse a condition");
		
		node.AddChild(new ParseNode(NodeType.Terminal, _tokens[_currentIndex]));
		Advance();
		
		//	Parse the right boolean operand
		node.AddChild(CreateNode(NodeType.Boolean, Boolean));
	}

	private void ExpressionQuery(ParseNode node)
	{
		//	Try and parse left expression operand
		node.AddChild(CreateNode(NodeType.Expression, Expression));
		
		//TODO: check whether this could cause out of bounds exception!
		//	Throw exception if token doesn't match any operators
		if (!Match(TokenType.NotEqual) && !Match(TokenType.Equals) && !Match(TokenType.More) &&
		    !Match(TokenType.Less)) throw new SyntaxErrorException($"Invalid operator token, expected '!=' | '==' | '>' | '<'. Actual: '{_tokens.ElementAtOrDefault(_currentIndex)}'", _currentLine);
		
		node.AddChild(new ParseNode(NodeType.Terminal, _tokens[_currentIndex]));
		Advance();
		
		//	Parse right expression operand
		try
		{
			node.AddChild(CreateNode(NodeType.Expression, Expression));
		}
		catch (InvalidSyntaxException e)
		{
			_logger.Warning(e.Message);
			throw new SyntaxErrorException($"Invalid right expression query operand!");
		}
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
		    throw new InvalidSyntaxException("Unexpected token, unable to parse conditional statement!");
		}

		if (Match(TokenType.LeftParen))
		{
			node.AddChild(new ParseNode(NodeType.Terminal, _tokens[_currentIndex]));
			Advance();
		}
		else
		{
			throw new SyntaxErrorException($"Mismatched parentheses, expected '('. Actual: '{_tokens.ElementAtOrDefault(_currentIndex)}'", _currentLine);
	    }

		//	Throw an invalid syntax error if we fail to parse a logic statement
		try
		{
			node.AddChild(CreateNode(NodeType.LogicStatement, LogicStatement));
		}
		catch(InvalidSyntaxException e)
		{
			_logger.Warning(e.Message);
			throw new SyntaxErrorException($"Invalid logic statement within conditional statement!");
		}
		
		if (Match(TokenType.RightParen))
		{
			node.AddChild(new ParseNode(NodeType.Terminal, _tokens[_currentIndex]));
			Advance();
		}
		else
        {
	        throw new SyntaxErrorException($"Mismatched parentheses, expected ')'. Actual: '{_tokens.ElementAtOrDefault(_currentIndex)}'", _currentLine);
        }

		try
		{
			node.AddChild(CreateNode(NodeType.Block, Block));
		}
		catch(InvalidSyntaxException e)
		{
			_logger.Warning(e.Message);
			throw new SyntaxErrorException($"Invalid block within if statement!");
		}
		
		if (Match(TokenType.String) && CheckElse())
		{
			node.AddChild(new ParseNode(NodeType.Terminal, _tokens[_currentIndex]));
			Advance();
			
			//	Parse else statement block, throwing if invalid
			try
			{
				node.AddChild(CreateNode(NodeType.Block, Block));
			}
			catch(InvalidSyntaxException e)
			{
				_logger.Warning(e.Message);
				throw new SyntaxErrorException($"Invalid block within else statement!");
			}
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
			throw new InvalidSyntaxException("Unexpected token, unable to parse while statement!");
		}

		if (Match(TokenType.LeftParen))
		{
			node.AddChild(new ParseNode(NodeType.Terminal, _tokens[_currentIndex]));
			Advance();
		}
		else
		{
			throw new SyntaxErrorException($"Mismatched parentheses, expected '('. Actual: '{_tokens.ElementAtOrDefault(_currentIndex)}'", _currentLine);
		}
		
		//	Throw an invalid syntax error if we fail to parse a logic statement
		try
		{
			node.AddChild(CreateNode(NodeType.LogicStatement, LogicStatement));
		}
		catch(InvalidSyntaxException e)
		{
			_logger.Warning(e.Message);
			throw new SyntaxErrorException($"Invalid logic statement within while statement!");
		}
		
		if (Match(TokenType.RightParen))
		{
			node.AddChild(new ParseNode(NodeType.Terminal, _tokens[_currentIndex]));
			Advance();
		}
		else
		{
			throw new SyntaxErrorException($"Mismatched parentheses, expected ')'. Actual: '{_tokens.ElementAtOrDefault(_currentIndex)}'", _currentLine);
		}
		
		//	Parse while statement block, throwing if invalid
		try
		{
			node.AddChild(CreateNode(NodeType.Block, Block));
		}
		catch(InvalidSyntaxException e)
		{
			_logger.Warning(e.Message);
			throw new SyntaxErrorException($"Invalid block within while statement!");
		}
	}
	
	private void Block(ParseNode node) {
		//	Try and parse block
		if (Match(TokenType.LeftCurlyBrace))
		{
			node.AddChild(new ParseNode(NodeType.Terminal, _tokens[_currentIndex]));
			Advance();
		}
		else
		{
			throw new InvalidSyntaxException("Unexpected token, unable to parse block!");
		}

		//TODO: investigate if the first condition is needed!
		while (_currentIndex < _totalTokens && !Match(TokenType.RightCurlyBrace))
		{
			try
			{
				node.AddChild(CreateNode(NodeType.Statement, Statement));
			}
			catch (InvalidSyntaxException e)
			{
				_logger.Warning(e.Message);
				throw new SyntaxErrorException($"Invalid statement within block!");
			}
		}
		if (Match(TokenType.RightCurlyBrace))
		{
			node.AddChild(new ParseNode(NodeType.Terminal, _tokens[_currentIndex]));
			Advance();
		}
		else
		{
			throw new SyntaxErrorException("Block not terminated! Expected '}'."+ $" Actual: '{_tokens.ElementAtOrDefault(_currentIndex)}'", _currentLine);
		}
	}
	#endregion
}
