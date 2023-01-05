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
	private List<Token> _tokens;
	private List<ParseNode> _parseTree = new();
	private VariableLUT _variableLut = new();

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
		if (word.Equals("true") || word.Equals("false")) return true;
		_logger.Error($"Bool parse error! Expected boolean literal, actual: \"{word}\"");
		return false;
	}
	private bool CheckIf()
    {
		var word = _tokens[_currentIndex].Word.Trim();
		if (word.Equals("if")) return true;
		_logger.Error($"If parse error! Expected if, actual: \"{word}\"");
		return false;
	}
	private bool CheckElse()
	{
		var word = _tokens[_currentIndex].Word.Trim();
		if (word.Equals("else")) return true;
		_logger.Error($"Else parse error! Expected else, actual: \"{word}\"");
		return false;
	}
	private bool CheckWhile()
	{
		var word = _tokens[_currentIndex].Word.Trim();
		if (word.Equals("while")) return true;
		_logger.Error($"While parse error! Expected while, actual: \"{word}\"");
		return false;
	}
	
	private bool CheckVarLiteral()
	{
		var word = _tokens[_currentIndex].Word.Trim();
		if (word.Equals("var"))
		{
			return true;
		}
		_logger.Error($"Var parse error! Expected variable literal, actual: \"{word}\"");
		return false;
	}
	
	private bool Match(TokenType tokenType)
	{
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

	private void Reset(int index = 0)
    {
		_currentIndex = index;
		_tokenType = _tokens[_currentIndex].Type;
    }
	
	private void Advance()
	{
		if (++_currentIndex >= _totalTokens) return; //todo - might be able to find a way to exit everything else quicker when we're at the end - EOF token!
		_tokenType = _tokens[_currentIndex].Type;
	}
    #endregion

    //TODO - Investigate if this is correct use of delegates
    private ParseNode CreateNode(NodeType type, Delegate func)
    {
	    ParseNode newNode = new ParseNode(type);
	    func(newNode);
	    return newNode;
    }

    //End Helper Functions

	public int Parse(List<Token> tokens)
	{
		_tokens = tokens;
		_totalTokens = tokens.Count;
		_parseTree = new();
		_variableLut = new();
		Reset(0);
		
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
		
		//TODO - Improve logging overall
		_logger.Error("Syntax error! Could not parse Token: {@token}", _tokens[_currentIndex]); //todo - Create test for this!
		return 1;
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
		catch (Exception e)
		{
			_logger.Warning(e.Message);
			Reset(currentIndex);
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
			Reset(currentIndex);
		}
		
		try
		{
			node.AddChild(CreateNode(NodeType.DeclareVariable, DeclareVariable));
			currentIndex = _currentIndex;
			return;
		}
		catch (Exception e)
		{
			_logger.Warning(e.Message);
			Reset(currentIndex);
		}
		try
		{
			var word = _tokens[_currentIndex].Word.Trim();

			if (_variableLut.IsDeclared(word))
			{
				ParseNode childNode = CreateNode(NodeType.VarAssignment, VarAssignment);
				node.AddChild(childNode);

				if (_variableLut.GetVarType(word).type != NodeType.Null && _variableLut.GetVarType(word) != childNode.getChildren()[2]) //gets the third child which is what the variable is assigned to
				{
					_logger.Error("Syntax Error! variable is not of type: {@type} ", childNode.getChildren()[2]); //todo - clean this!
				}
				currentIndex = _currentIndex;
				return;
			}
			_logger.Error("Syntax Error! variable is not declared: {@word} ", _tokens[_currentIndex].Word);
		}
		catch (Exception e)
		{
			_logger.Warning(e.Message);
			Reset(currentIndex);
		}
		throw new Exception("Syntax error! invalid statement");
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
			_logger.Error("Syntax Error! variable is not declared, expected \"var\" actual: {@word} ", _tokens[_currentIndex].Word);
			throw new SyntaxErrorException();
		}
		
		var word = _tokens[_currentIndex].Word.Trim();
		int Rein = _currentIndex;
		
		// check if a value is being assigned
		try
		{
			ParseNode childNode = CreateNode(NodeType.VarAssignment, VarAssignment);
			node.AddChild(childNode);
			
			if (_variableLut.IsDeclared(word))
			{
				_logger.Error("Syntax Error! variable has already been declared: {@word} ", _tokens[_currentIndex].Word);
				throw new SyntaxErrorException();
			}
			_variableLut.AddToLut(word, childNode.getChildren()[2]); //this gets the third child which is what the variable is assigned to
			return;
		}
		catch (Exception e)
		{
			Reset(Rein);
			_logger.Warning(e.Message);
		}

		ParseNode childNode2 = CreateNode(NodeType.VarIdentifier, VarIdentifier); //todo - rename
		node.AddChild(childNode2);

		if (_variableLut.IsDeclared(word))
		{
			_logger.Error("Syntax Error! variable has already been declared: {@word} ", _tokens[_currentIndex].Word);
			throw new SyntaxErrorException();
		}

		_variableLut.AddToLut(word);

		if (Match(TokenType.SemiColon))
		{
			node.AddChild(new ParseNode(NodeType.Terminal, _tokens[_currentIndex]));
			Advance();
		}
		else
		{
			_logger.Error("Syntax Error! declaration is not closed, expected \";\" actual: {@word} ", _tokens[_currentIndex].Word);
			throw new SyntaxErrorException();
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
			_logger.Error("Syntax Error! variable does not have a name, expected \"X\" actual: {@word} ", _tokens[_currentIndex].Word);
			throw new SyntaxErrorException();
		}
	}
	//TODO - Rename to match ebnf (Assignment)
	private void VarAssignment(ParseNode node)
	{
		// check for identifier
		node.AddChild(CreateNode(NodeType.VarIdentifier, VarIdentifier));

		if (!Match(TokenType.Assignment)) throw new SyntaxErrorException();
		node.AddChild(new ParseNode(NodeType.Terminal, _tokens[_currentIndex]));
		Advance();
		
		//TODO - Also try check for word, and logic statement
		try
		{
			node.AddChild(CreateNode(NodeType.Expression, Expression));
		}
		catch (Exception e)
		{
			_logger.Warning(e.Message);
			throw new SyntaxErrorException("invalid assignment expression");
		}
		
		if (Match(TokenType.SemiColon))
		{
			node.AddChild(new ParseNode(NodeType.Terminal, _tokens[_currentIndex]));
			Advance();
		}
		else
		{
			_logger.Error("Syntax Error! declaration is not closed, expected \";\" actual: {@word} ", _tokens[_currentIndex].Word);
			throw new SyntaxErrorException();
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
		
		try
		{
			node.AddChild(CreateNode(NodeType.Condition, Condition));
		}
		catch (Exception e)
		{
			_logger.Warning(e.Message);
		}
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
				Reset(index);

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
		var index = _currentIndex;
		if (!Match(TokenType.NotEqual) && !Match(TokenType.Equals) && !Match(TokenType.And) &&
		    !Match(TokenType.Or)) throw new SyntaxErrorException("Syntax Error! Unexpected token at token " + _currentIndex);
		node.AddChild(new ParseNode(NodeType.Terminal, _tokens[_currentIndex]));
		Advance();
		
		try
		{
			node.AddChild(CreateNode(NodeType.Boolean, Boolean));
		}
		catch (Exception e)
		{
			_logger.Warning(e.Message);
			Reset(index);
		}
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
		
		if (Match(TokenType.String) && CheckElse())
		{
			node.AddChild(new ParseNode(NodeType.Terminal, _tokens[_currentIndex]));
			Advance();
			
			node.AddChild(CreateNode(NodeType.Block, Block));
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
			while (_currentIndex < _totalTokens && !Match(TokenType.RightCurlyBrace))
			{
				node.AddChild(CreateNode(NodeType.Statement, Statement));
			}
		}
		catch (Exception e)
		{
			_logger.Warning(e.Message);
			throw new SyntaxErrorException("Syntax Error!" + _currentIndex); //todo - this message!
		}

		if (Match(TokenType.RightCurlyBrace))
		{
			node.AddChild(new ParseNode(NodeType.Terminal, _tokens[_currentIndex]));
			Advance();
		}
		else
		{
			_logger.Error("Syntax Error! Expected \"}\" actual: {@word} ", _tokens[_currentIndex].Word);
			throw new SyntaxErrorException("Syntax Error! Expected \"}\" at token " + _currentIndex);
		}
	}

	#endregion
}
