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
using Serilog;

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
		_tokenType = TokenType.Null;
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
		//_logger.Information("advance() called at level {level}. Next token is {@token}", level, _tokens[_currentIndex]); //todo - fix
	}
    #endregion

    private void printNodesAndChildren(ParseNode node) //todo - remove
    {
	    _logger.Error("Node: " + node);
	    foreach (var child in node.getChildren())
	    {
		    _logger.Error("{@node} Child: " + child, node.ToString());
	    }
    }

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
		Reset();
		
		try
		{
			while (_currentIndex < _totalTokens)
			{
				ParseNode statementNode = new ParseNode(NodeType.Statement);
				Statement(statementNode);
				_parseTree.Add(statementNode);
				printNodesAndChildren(statementNode);
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
		//_logger.Information( "Statement() called at level {level}", level); //todo - fix
		
		try
		{
			//Expression(level + 1);
			//ParseNode newNode = new ParseNode(NodeType.Expression);
			//Expression(newNode);
			//node.assignChild(newNode);
			node.AddChild(CreateNode(NodeType.Expression, Expression)); //todo - check this will work everywhere

			//currentIndex = _currentIndex;
			//return;
		}
		catch (Exception e)
		{
			_logger.Warning(e.Message);
			//Reset();
			//Set(currentIndex); //todo - check this as it isn't right
			//_currentIndex = currentIndex;
		}
		if (_currentIndex >= _totalTokens) return;
		Set(currentIndex);
		
		try
		{
			node.AddChild(CreateNode(NodeType.Conditional, IfStatements));
			currentIndex = _currentIndex;
			//ParseNode newNode = new ParseNode(NodeType.LogicStatement);
			//IfStatements(newNode);
			//node.assignChild(newNode);
			//currentIndex = _currentIndex;
			//return;
		}
		catch (Exception e)
		{
			_logger.Warning(e.Message);
			//Reset();
			Set(currentIndex);
			//_currentIndex = currentIndex;
		}
		
		try
		{
			node.AddChild(CreateNode(NodeType.WhileStatement, WhileStatement));
			currentIndex = _currentIndex;
			//ParseNode newNode = new ParseNode(NodeType.WhileStatement);
			//WhileStatement(newNode);
			//node.assignChild(newNode);
			//currentIndex = _currentIndex;
			//return;
		}
		catch (Exception e)
		{
			_logger.Warning(e.Message);
			//Reset();
			Set(currentIndex);
			//_currentIndex = currentIndex;
		}

		//try
		//{
		node.AddChild(CreateNode(NodeType.LogicStatement, LogicStatement));
			//ParseNode nextNode = new ParseNode(NodeType.LogicStatement);
			//LogicStatement(nextNode);
			//node.assignChild(nextNode);
		//}
		//catch (Exception e)
		//{
		//	_logger.Warning(e.Message);
		//}
	}

    #region Expressions
	private void Expression(ParseNode node)
	{
		//_logger.Information( "expression() called at level {level}", level); //todo - fix
		node.AddChild(CreateNode(NodeType.Term, Term));
		//ParseNode newNode = new ParseNode(NodeType.Term);
		//Term(newNode);
		//node.assignChild(newNode);
		if (!Match(TokenType.Add) && !Match(TokenType.Sub)) return; //this is terminal
		node.AddChild(new ParseNode(NodeType.Terminal, _tokens[_currentIndex]));
		Advance(); //todo - fix Advance - probably doesnt need to take a node
		node.AddChild(CreateNode(NodeType.Term, Term));
		//ParseNode nextNode = new ParseNode(NodeType.Term); //todo - sort naming convention for node or do differently
		//Term(nextNode);
		//node.assignChild(nextNode);
	}

	private void Term(ParseNode node)
	{
		//_logger.Information( "term() called at level {level} ", level); //todo - fix
		node.AddChild(CreateNode(NodeType.Factor, Factor));
		//ParseNode newNode = new ParseNode(NodeType.Factor);
		//Factor(newNode);
		//node.assignChild(newNode);
		if (!Match(TokenType.Multi) && !Match(TokenType.Divide)) return; //this is terminal
		node.AddChild(new ParseNode(NodeType.Terminal));
		Advance();
		node.AddChild(CreateNode(NodeType.Factor, Factor));
		//ParseNode nextNode = new ParseNode(NodeType.Factor);
		//Factor(nextNode);
		//node.assignChild(nextNode);
	}

	private void Factor(ParseNode node)
	{
		//_logger.Information( "factor() called at level {level}", level); //todo - fix
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
			//Factor(node);
			//node.AddChild(subNode);
		}
		else if (Match(TokenType.LeftParen))
		{
			node.AddChild(new ParseNode(NodeType.Terminal, _tokens[_currentIndex]));
			Advance();

			node.AddChild(CreateNode(NodeType.Expression, Expression));
			//ParseNode expressionNode = new ParseNode(NodeType.Terminal, _tokens[_currentIndex]);
			//Expression(expressionNode);
			//node.AddChild(expressionNode);
			//printNodesAndChildren(expressionNode);
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
		//_logger.Information( "LogicStatement() called at level {level}", level);
		node.AddChild(CreateNode(NodeType.Boolean, Boolean));
		//ParseNode newNode = new ParseNode(NodeType.Boolean);
		//Boolean(newNode);
		//node.AddChild(newNode);

		node.AddChild(CreateNode(NodeType.Conditional, Condition));
		//ParseNode nextNode = new ParseNode(NodeType.Conditional);
		//Condition(nextNode); //todo - only assign if this happened
		//node.AddChild(nextNode);
	}

	private void Boolean(ParseNode node)
	{
		//_logger.Information( "Boolean() called at level {level}", level);

		if (Match(TokenType.Not))
		{
			node.AddChild(new ParseNode(NodeType.Terminal));
			Advance();
			
			node.AddChild(CreateNode(NodeType.LogicStatement, LogicStatement));
			//ParseNode nextNode = new ParseNode(NodeType.LogicStatement);
			//LogicStatement(nextNode);
			//node.AddChild(nextNode);
		}
		else if (Match(TokenType.String))
		{
			if (CheckBoolLiteral())
			{
				node.AddChild(new ParseNode(NodeType.Terminal));
				Advance();
			}
			else
			{
				//_logger.Error("Syntax Error! Expected Boolean Literal but found {index}", _currentIndex);
				throw new SyntaxErrorException("Syntax Error! Unexpected Boolean Literal at token " + _currentIndex);
			}
		}
		else
		{
			var index = _currentIndex;
			try
			{
				node.AddChild(CreateNode(NodeType.ExpressionQuery, ExpressionQuery));
				//ParseNode newNode = new ParseNode(NodeType.ExpressionQuery);
				//ExpressionQuery(newNode);
				//node.AddChild(newNode);
			}
			catch (Exception e)
			{
				_logger.Warning(e.Message);
				
				_currentIndex = index;
				//_tokenType = TokenType.Null;
				//if (_currentIndex >= _totalTokens) //todo - make this better
				//{
				//	return;
				//}

				if (Match(TokenType.LeftParen))
				{
					node.AddChild(new ParseNode(NodeType.Terminal));
					Advance();
					
					node.AddChild(CreateNode(NodeType.LogicStatement, LogicStatement));
					//ParseNode nextNode = new ParseNode(NodeType.LogicStatement);
					//LogicStatement(nextNode);
					//node.AddChild(nextNode);

					if (Match(TokenType.RightParen))
					{
						node.AddChild(new ParseNode(NodeType.Terminal));
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
		//_logger.Information( "Condition() called at level {level}", level);

		if (!(Match(TokenType.Equals) || Match(TokenType.And) || Match(TokenType.Or) || Match(TokenType.Not)))
			return;
		if (Match(TokenType.Not))
		{
			Advance();
			if (!Match(TokenType.Equals))
			{
				_logger.Error("Syntax Error! Mismatched inequality operator, expected \"=\" actual: {@word} ", _tokens[_currentIndex].Word);
				return;
			}
			node.AddChild(new ParseNode(NodeType.Terminal));
			Advance();
		}
		else if (Match(TokenType.Equals))
		{
			Advance();
			if (!Match(TokenType.Equals))
			{
				_logger.Error("Syntax Error! Mismatched equality operator, expected \"=\" actual: {@word} ", _tokens[_currentIndex].Word);
				return;
			}
			node.AddChild(new ParseNode(NodeType.Terminal));
			Advance();
		}
		node.AddChild(CreateNode(NodeType.Boolean, Boolean));
		//ParseNode otherNode = new ParseNode(NodeType.Boolean);
		//Boolean(otherNode);
		//node.AddChild(otherNode); //todo - only assign if this works
	}

	private void ExpressionQuery(ParseNode node)
	{
		//_logger.Information( "ExpressionQuery() called at level {level}", level);
		node.AddChild(CreateNode(NodeType.Expression, Expression));
		//ParseNode newNode = new ParseNode(NodeType.Expression);
		//Expression(newNode);
		//node.AddChild(newNode);
		if (!(Match(TokenType.Equals) || Match(TokenType.More) || Match(TokenType.Less) || Match(TokenType.Not)))
			return;
		//ParseNode nextNode = new ParseNode(NodeType.Terminal);
		if (Match(TokenType.Not))
		{
			Advance();
			if (!Match(TokenType.Equals))
			{
				_logger.Error("Syntax Error! Mismatched inequality operator, expected \"=\" actual: {@word} ", _tokens[_currentIndex].Word);
				return;
			}
		}
		else if (Match(TokenType.Equals))
		{
			Advance();
			if (!Match(TokenType.Equals))
			{
				_logger.Error("Syntax Error! Mismatched equality operator, expected \"=\" actual: {@word} ", _tokens[_currentIndex].Word);
				return;
			}
		}
		node.AddChild(new ParseNode(NodeType.Terminal));
		Advance();
		
		node.AddChild(CreateNode(NodeType.Expression, Expression));
		//ParseNode otherNode = new ParseNode(NodeType.Expression);
		//Expression(otherNode);
		//node.AddChild(otherNode);
	}
    #endregion
    #region Condition-Statements and iterators
    
	private void IfStatements(ParseNode node)
    {
		//_logger.Information("if-statement() called at level {level}", level);
		if (Match(TokenType.String) && CheckIf())
		{
			node.AddChild(new ParseNode(NodeType.Terminal));
			Advance();
		}
		else {
			_logger.Error("Syntax Error! Expected \"if\" actual: {@word} ", _tokens[_currentIndex].Word);
			throw new SyntaxErrorException("Syntax Error! Expected \"if\" at token " + _currentIndex);
		}

		if (Match(TokenType.LeftParen))
		{
			node.AddChild(new ParseNode(NodeType.Terminal));
			Advance();
		}
		else
        {
            _logger.Error("Syntax Error! Expected \"(\" actual: {@word} ", _tokens[_currentIndex].Word);
			throw new SyntaxErrorException("Syntax Error! Expected \"(\" at token " + _currentIndex);
        }

		node.AddChild(CreateNode(NodeType.LogicStatement, LogicStatement));
		//ParseNode logicStatementNode = new ParseNode(NodeType.LogicStatement); 
		//LogicStatement(logicStatementNode); //todo - check this is working correctly and won't cause an error
		//node.AddChild(logicStatementNode);
		if (Match(TokenType.RightParen))
		{
			node.AddChild(new ParseNode(NodeType.Terminal));
			Advance();
		}
		else
        {
			_logger.Error("Syntax Error! Expected \"(\" actual: {@word} ", _tokens[_currentIndex].Word);
			throw new SyntaxErrorException("Syntax Error! Expected \")\" at token " + _currentIndex);
		}

		node.AddChild(CreateNode(NodeType.Block, Block));
		//ParseNode blockNode = new ParseNode(NodeType.Block);
		//Block(blockNode);
		//node.AddChild(blockNode);
		try
		{
			if (Match(TokenType.String) && CheckElse())
			{
				node.AddChild(new ParseNode(NodeType.Terminal));
				Advance();
				
				node.AddChild(CreateNode(NodeType.Block, Block));
			}
			else
			{
				_logger.Error("Syntax Error! Expected \"else\" actual: {@word} ", _tokens[_currentIndex].Word);
				//throw new SyntaxErrorException("Syntax Error! Expected \"else\" at token " + _currentIndex);
			}
		}
		catch (Exception e)
		{
			_logger.Warning(e.Message);
		}
	}

	private void WhileStatement(ParseNode node)
	{
		//_logger.Information("while-statement() called at level {level}", level);
		if (Match(TokenType.String) && CheckWhile())
		{
			node.AddChild(new ParseNode(NodeType.Terminal));
			Advance();
		}
		else
		{
			_logger.Error("Syntax Error! Expected \"while\" actual: {@word} ", _tokens[_currentIndex].Word);
			throw new SyntaxErrorException("Syntax Error! Expected \"while\" at token " + _currentIndex);
		}

		if (Match(TokenType.LeftParen))
		{
			node.AddChild(new ParseNode(NodeType.Terminal));
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
			node.AddChild(new ParseNode(NodeType.Terminal));
			Advance();
		}
		else
		{
			_logger.Error("Syntax Error! Expected \"(\" actual: {@word} ", _tokens[_currentIndex].Word);
			throw new SyntaxErrorException("Syntax Error! Expected \")\" at token " + _currentIndex);
		}

		node.AddChild(CreateNode(NodeType.Block, Block));
	}

	//checks that there is a valid block as described in the EBNF above
	private void Block(ParseNode node) {
		if (Match(TokenType.LeftCurlyBrace))
		{
			node.AddChild(new ParseNode(NodeType.Terminal));
			Advance();
		}
		else
		{
			_logger.Error("Syntax Error! Expected \"{\" actual: {@word} ", _tokens[_currentIndex].Word);
			throw new SyntaxErrorException("Syntax Error! Expected \"{\" at token " + _currentIndex);
		}
		
		try
		{
			if (Match(TokenType.RightCurlyBrace))
			{
				node.AddChild(new ParseNode(NodeType.Terminal));
				Advance();
			}
		}
		catch (Exception e)
		{
			_logger.Warning(e.Message);
		}
		
		//TODO - For now, blocks cannot contain anything, once variables have been added we can change this

		/* //TODO - Add back in once variables are added
		if (Match(TokenType.RightCurlyBrace))
		{
			ParseNode newNode = new ParseNode(NodeType.Terminal);
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
