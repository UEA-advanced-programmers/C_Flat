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
	private VariableTable _variableTable = new();

	private delegate void NodeFuncDelegate(ParseNode node);

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
		if(word is "true" or "false") return true;
		throw new Exception("Expected a boolean literal, actual: {word}");
	}
	private bool CheckIf()
    {
		var word = _tokens[_currentIndex].Word.Trim();
		if (word is "if") return true;
		throw new Exception($"Parser error! Expected if, actual: {word}");
    }
	private bool CheckElse()
	{
		var word = _tokens[_currentIndex].Word.Trim();
		if (word is "else") return true;
		throw new Exception($"Parser error! Expected else, actual: {word}");
	}
	private bool CheckWhile()
	{
		var word = _tokens[_currentIndex].Word.Trim();
		if (word is "while") return true;
		throw new Exception($"Parser error! Expected while, actual: {word}");
	}
	
	private bool CheckVarLiteral()
	{
		var word = _tokens[_currentIndex].Word.Trim();
		if (word is "var") return true;
		throw new Exception($"Parser error! Expected var, actual: {word}");
	}
	
	private bool Match(TokenType tokenType)
	{
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
    }
	
	private void Advance()
	{
		if (++_currentIndex >= _totalTokens) return; //todo - might be able to find a way to exit everything else quicker when we're at the end - EOF token!
		_tokenType = _tokens[_currentIndex].Type;
	}
    #endregion
    
    private ParseNode CreateNode(NodeType type, NodeFuncDelegate nodeFunc)
    {
	    ParseNode newNode = new ParseNode(type);
	    nodeFunc(newNode);
	    return newNode;
    }

    //End Helper Functions

	public int Parse(List<Token> tokens)
	{
		_tokens = tokens;
		_totalTokens = tokens.Count;
		_parseTree = new List<ParseNode>();
		_variableTable = new VariableTable();
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
			_logger.Error(e.Message);
		}
		if (_currentIndex >= _totalTokens) return 0;
		
		//TODO - Improve logging overall
		//_logger.Error("Syntax error! Could not parse Token: {@token}", _tokens[_currentIndex]); //todo - Create test for this!
		return 1;
	}

	//EBNF Functions
	private void Statement(ParseNode node)
	{
		int currentIndex = _currentIndex;

		Dictionary<NodeType, NodeFuncDelegate> statements = new Dictionary<NodeType, NodeFuncDelegate>
		{
			{ NodeType.ConditionalStatement, ConditionalStatement },
			{ NodeType.WhileStatement, WhileStatement },
			{ NodeType.DeclareVariable, Declaration }
		};

		foreach (var statement in statements)
		{
			try
			{
				node.AddChild(CreateNode(statement.Key, statement.Value));
				currentIndex = _currentIndex;
				return;
			}
			catch (Exception e)
			{
				_logger.Warning(e.Message);
				Reset(currentIndex);
			}
		}

		try
		{
			var identifier = _tokens[_currentIndex].Word.Trim();

			if (_variableTable.Exists(identifier))
			{
				ParseNode childNode = CreateNode(NodeType.VarAssignment, Assignment);
				node.AddChild(childNode);

				ParseNode valueNode = childNode.getChildren()[2]; //gets the value the variable is assigned to
				if (_variableTable.GetType(identifier).type != NodeType.Null && _variableTable.GetType(identifier) != valueNode)
				{
					_logger.Error("Syntax Error! variable is not of type: {@type} ", valueNode);
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
	private void Declaration(ParseNode node)
	{
		// check for String token with the name "var"
		if (Match(TokenType.String) && CheckVarLiteral())
		{
			node.AddChild(new ParseNode(NodeType.Terminal, _tokens[_currentIndex]));
			Advance();
		}
		else
		{
			throw new Exception($"Syntax Error! Expected 'var' keyword, actual: {_tokens[_currentIndex].Word}");
		}
		
		var identifier = _tokens[_currentIndex].Word.Trim();
		int currentIndex = _currentIndex;
		
		if (_variableTable.Exists(identifier))
		{
			_logger.Error("Syntax Error! variable has already been declared: {@word} ", _tokens[_currentIndex].Word);
			throw new SyntaxErrorException();
		}
		
		// check if a value is being assigned
		try
		{
			ParseNode childNode = CreateNode(NodeType.VarAssignment, Assignment);
			node.AddChild(childNode);
			
			_variableTable.Add(identifier, childNode.getChildren()[2]); //gets the value the variable is assigned to
			return;
		}
		catch (Exception e)
		{
			Reset(currentIndex);
			_logger.Warning(e.Message);
		}

		node.AddChild(CreateNode(NodeType.VarIdentifier, Identifier));
		
		_variableTable.Add(identifier);

		if (Match(TokenType.SemiColon))
		{
			node.AddChild(new ParseNode(NodeType.Terminal, _tokens[_currentIndex]));
			Advance();
		}
		else
		{
			throw new Exception($"Syntax Error! Declaration is not terminated, expected ';', actual: {_tokens[_currentIndex].Word}");
		}
	}
	
	private void Identifier(ParseNode node)
	{
		if (Match(TokenType.String))
		{
			node.AddChild(new ParseNode(NodeType.Terminal, _tokens[_currentIndex]));
			Advance();
		}
		else
		{
			throw new Exception($"Syntax Error! variable missing an identifier expected 'string', actual: {_tokenType}");
		}
	}
	private void Assignment(ParseNode node)
	{
		// check for identifier
		node.AddChild(CreateNode(NodeType.VarIdentifier, Identifier));

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
			throw new Exception("Invalid assignment expression");
		}
		
		if (Match(TokenType.SemiColon))
		{
			node.AddChild(new ParseNode(NodeType.Terminal, _tokens[_currentIndex]));
			Advance();
		}
		else
		{
			throw new Exception(
				$"Syntax error! Declaration is not terminated, expected ';' actual: {_tokens[_currentIndex].Word}");
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
				throw new Exception("Syntax Error! Mismatched parentheses at token " + _currentIndex);
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
			var currentIndex = _currentIndex;
			try
			{
				node.AddChild(CreateNode(NodeType.ExpressionQuery, ExpressionQuery));
			}
			catch (Exception e)
			{
				_logger.Warning(e.Message);
				Reset(currentIndex);

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
						throw new SyntaxErrorException($"Syntax error! Mismatched parentheses at token {_currentIndex}");
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
    
	private void ConditionalStatement(ParseNode node)
    {
	    if (Match(TokenType.String) && CheckIf())
		{
			node.AddChild(new ParseNode(NodeType.Terminal, _tokens[_currentIndex]));
			Advance();
		}
		else {
		    throw new SyntaxErrorException($"Syntax Error! Expected 'if' at token {_currentIndex} ");
		}

		if (Match(TokenType.LeftParen))
		{
			node.AddChild(new ParseNode(NodeType.Terminal, _tokens[_currentIndex]));
			Advance();
		}
		else
		{
			throw new SyntaxErrorException($"Syntax Error! Expected '(' actual: {_tokens[_currentIndex].Word}");
	    }

		node.AddChild(CreateNode(NodeType.LogicStatement, LogicStatement));
		
		if (Match(TokenType.RightParen))
		{
			node.AddChild(new ParseNode(NodeType.Terminal, _tokens[_currentIndex]));
			Advance();
		}
		else
        {
	        throw new SyntaxErrorException($"Syntax Error! Expected ')' actual: {_tokens[_currentIndex].Word}");
        }

		node.AddChild(CreateNode(NodeType.Block, Block));

		if (!Match(TokenType.String) || !CheckElse()) return;
		node.AddChild(new ParseNode(NodeType.Terminal, _tokens[_currentIndex]));
		Advance();
			
		node.AddChild(CreateNode(NodeType.Block, Block));
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
			throw new SyntaxErrorException($"Syntax Error! Expected 'while' actual: {_tokens[_currentIndex].Word}");
		}

		if (Match(TokenType.LeftParen))
		{
			node.AddChild(new ParseNode(NodeType.Terminal, _tokens[_currentIndex]));
			Advance();
		}
		else
		{
			throw new SyntaxErrorException($"Syntax Error! Expected '(' actual: {_tokens[_currentIndex].Word}");
		}
		
		node.AddChild(CreateNode(NodeType.LogicStatement, LogicStatement));
		
		if (Match(TokenType.RightParen))
		{
			node.AddChild(new ParseNode(NodeType.Terminal, _tokens[_currentIndex]));
			Advance();
		}
		else
		{
			throw new SyntaxErrorException($"Syntax Error! Expected ')' actual: {_tokens[_currentIndex].Word}");
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
			throw new SyntaxErrorException("Syntax Error! Expected " + @"{" + $"actual: {_tokens[_currentIndex].Word}");
		}

		while (_currentIndex < _totalTokens && !Match(TokenType.RightCurlyBrace))
		{
			node.AddChild(CreateNode(NodeType.Statement, Statement));
		}
		
		if (Match(TokenType.RightCurlyBrace))
		{
			node.AddChild(new ParseNode(NodeType.Terminal, _tokens[_currentIndex]));
			Advance();
		}
		else
		{
			throw new SyntaxErrorException("Syntax Error! Expected " + @"}" + $"actual: {_tokens[_currentIndex].Word}");
		}
	}
	#endregion
}
