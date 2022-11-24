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
	private void Advance(ParseNode node)
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

    private ParseNode testFunc(NodeType type, Delegate func)
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
			node.assignChild(testFunc(NodeType.Expression, Expression));

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
			ParseNode newNode = new ParseNode(NodeType.LogicStatement);
			IfStatements(newNode);
			node.assignChild(newNode);
			//currentIndex = _currentIndex;
			//return;
		}
		catch (Exception e)
		{
			_logger.Warning(e.Message);
			//Reset();
			//Set(currentIndex);
			//_currentIndex = currentIndex;
		}
		
		try
		{
			ParseNode newNode = new ParseNode(NodeType.WhileStatement);
			WhileStatement(newNode);
			node.assignChild(newNode);
			//currentIndex = _currentIndex;
			//return;
		}
		catch (Exception e)
		{
			_logger.Warning(e.Message);
			//Reset();
			//Set(currentIndex);
			//_currentIndex = currentIndex;
		}

		//try
		//{
			ParseNode nextNode = new ParseNode(NodeType.LogicStatement);
			LogicStatement(nextNode);
			node.assignChild(nextNode);
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
		//Term(level + 1);
		ParseNode newNode = new ParseNode(NodeType.Term);
		Term(newNode);
		node.assignChild(newNode);
		printNodesAndChildren(newNode);
		if (!Match(TokenType.Add) && !Match(TokenType.Sub)) return; //this is terminal
		ParseNode terminalNode = new ParseNode(NodeType.Terminal, _tokens[_currentIndex]);
		node.assignChild(terminalNode);
		printNodesAndChildren(terminalNode);
		Advance(node); //todo - fix
		ParseNode nextNode = new ParseNode(NodeType.Term); //todo - sort naming convention for node or do differently
		Term(nextNode);
		node.assignChild(nextNode);
		printNodesAndChildren(nextNode);
	}

	private void Term(ParseNode node)
	{
		//_logger.Information( "term() called at level {level} ", level); //todo - fix
		ParseNode newNode = new ParseNode(NodeType.Factor);
		Factor(newNode);
		node.assignChild(newNode);
		printNodesAndChildren(newNode);
		if (!Match(TokenType.Multi) && !Match(TokenType.Divide)) return; //this is terminal
		ParseNode terminalNode = new ParseNode(NodeType.Terminal);
		Factor(terminalNode);
		node.assignChild(terminalNode);
		printNodesAndChildren(terminalNode);
		Advance(terminalNode);
		ParseNode nextNode = new ParseNode(NodeType.Factor);
		Factor(nextNode);
		node.assignChild(nextNode);
		printNodesAndChildren(nextNode);
	}

	private void Factor(ParseNode node)
	{
		//_logger.Information( "factor() called at level {level}", level); //todo - fix
		//ParseNode newNode = new ParseNode(node);
		if (Match(TokenType.Num))
		{
			//ParseNode numberNode = new ParseNode(NodeType.NumTerminal); //todo - do I need to add the number here?
			ParseNode numberNode = new ParseNode(NodeType.Terminal, _tokens[_currentIndex]);
			
			Advance(numberNode); //todo - do we really need to pass the node here? We need to advance the list, the tree advances itself
			node.assignChild(numberNode);
			printNodesAndChildren(numberNode);
		}
		else if (Match(TokenType.Sub))
		{
			ParseNode subNode = new ParseNode(NodeType.Terminal, _tokens[_currentIndex]);
			Advance(subNode);
			Factor(subNode);
			node.assignChild(subNode);
			printNodesAndChildren(subNode);
		}
		else if (Match(TokenType.LeftParen))
		{
			ParseNode leftParenNode = new ParseNode(NodeType.Terminal, _tokens[_currentIndex]);
			node.assignChild(leftParenNode);
			printNodesAndChildren(leftParenNode);
			Advance(leftParenNode);
			
			ParseNode expressionNode = new ParseNode(NodeType.Terminal, _tokens[_currentIndex]);
			Expression(expressionNode);
			node.assignChild(expressionNode);
			printNodesAndChildren(expressionNode);
			if (Match(TokenType.RightParen))
			{
				ParseNode rightParenNode = new ParseNode(NodeType.Terminal, _tokens[_currentIndex]);
				node.assignChild(rightParenNode);
				printNodesAndChildren(rightParenNode);
				Advance(rightParenNode);
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
		ParseNode newNode = new ParseNode(NodeType.Boolean);
		Boolean(newNode);
		node.assignChild(newNode);

		ParseNode nextNode = new ParseNode(NodeType.Conditional);
		Condition(nextNode);
		node.assignChild(nextNode);
	}

	private void Boolean(ParseNode node)
	{
		//_logger.Information( "Boolean() called at level {level}", level);

		if (Match(TokenType.Not))
		{
			ParseNode newNode = new ParseNode(NodeType.Terminal);
			Advance(newNode);
			node.assignChild(newNode);
			
			ParseNode nextNode = new ParseNode(NodeType.LogicStatement);
			LogicStatement(nextNode);
			node.assignChild(nextNode);
		}
		else if (Match(TokenType.String))
		{
			if (CheckBoolLiteral())
			{
				ParseNode newNode = new ParseNode(NodeType.Terminal);
				Advance(newNode);
				node.assignChild(newNode);
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
				ParseNode newNode = new ParseNode(NodeType.ExpressionQuery);
				ExpressionQuery(newNode);
				node.assignChild(newNode);
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
					ParseNode newNode = new ParseNode(NodeType.Terminal);
					Advance(newNode);
					node.assignChild(newNode);
					
					ParseNode nextNode = new ParseNode(NodeType.LogicStatement);
					LogicStatement(nextNode);
					node.assignChild(nextNode);

					if (Match(TokenType.RightParen))
					{
						ParseNode otherNode = new ParseNode(NodeType.Terminal);
						Advance(otherNode);
						node.assignChild(otherNode);
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
			ParseNode nextNode = new ParseNode(NodeType.Terminal);
			Advance(nextNode);
			if (!Match(TokenType.Equals))
			{
				_logger.Error("Syntax Error! Mismatched inequality operator, expected \"=\" actual: {@word} ", _tokens[_currentIndex].Word);
				return;
			}
			node.assignChild(nextNode);
		}
		else if (Match(TokenType.Equals))
		{
			ParseNode nextNode = new ParseNode(NodeType.Terminal);
			Advance(nextNode);
			if (!Match(TokenType.Equals))
			{
				_logger.Error("Syntax Error! Mismatched equality operator, expected \"=\" actual: {@word} ", _tokens[_currentIndex].Word);
				return;
			}
			node.assignChild(nextNode);
		}
		ParseNode otherNode = new ParseNode(NodeType.Boolean);
		Boolean(otherNode);
		node.assignChild(otherNode);
	}

	private void ExpressionQuery(ParseNode node)
	{
		//_logger.Information( "ExpressionQuery() called at level {level}", level);
		ParseNode newNode = new ParseNode(NodeType.Expression);
		Expression(newNode);
		node.assignChild(newNode);
		if (!(Match(TokenType.Equals) || Match(TokenType.More) || Match(TokenType.Less)))
			return;
		ParseNode nextNode = new ParseNode(NodeType.Terminal);
		if (Match(TokenType.Not))
		{
			Advance(nextNode);
			if (!Match(TokenType.Equals))
			{
				_logger.Error("Syntax Error! Mismatched inequality operator, expected \"=\" actual: {@word} ", _tokens[_currentIndex].Word);
				return;
			}
		}
		else if (Match(TokenType.Equals))
		{
			Advance(nextNode);
			if (!Match(TokenType.Equals))
			{
				_logger.Error("Syntax Error! Mismatched equality operator, expected \"=\" actual: {@word} ",
					_tokens[_currentIndex].Word);
				return;
			}
		}
		node.assignChild(nextNode);
		
		ParseNode otherNode = new ParseNode(NodeType.Expression);
		Advance(otherNode);
		Expression(otherNode);
		node.assignChild(otherNode);
	}
    #endregion
    #region Condition-Statements and iterators
    
	private void IfStatements(ParseNode node)
    {
		//_logger.Information("if-statement() called at level {level}", level);
		if (Match(TokenType.String) && CheckIf())
		{
			ParseNode newNode = new ParseNode(NodeType.Terminal);
			Advance(newNode);
			node.assignChild(newNode);
		}
		else {
			_logger.Error("Syntax Error! Expected \"if\" actual: {@word} ", _tokens[_currentIndex].Word);
			throw new SyntaxErrorException("Syntax Error! Expected \"if\" at token " + _currentIndex);
		}

		if (Match(TokenType.LeftParen))
		{
			ParseNode newNode = new ParseNode(NodeType.Terminal);
			Advance(newNode);
			node.assignChild(newNode);
		}
		else
        {
            _logger.Error("Syntax Error! Expected \"(\" actual: {@word} ", _tokens[_currentIndex].Word);
			throw new SyntaxErrorException("Syntax Error! Expected \"(\" at token " + _currentIndex);
        }

		ParseNode logicStatementNode = new ParseNode(NodeType.LogicStatement); 
		LogicStatement(logicStatementNode); //todo - check this is working correctly and won't cause an error
		node.assignChild(logicStatementNode);
		if (Match(TokenType.RightParen))
		{
			ParseNode newNode = new ParseNode(NodeType.Terminal);
			Advance(newNode);
			node.assignChild(newNode);
		}
		else
        {
			_logger.Error("Syntax Error! Expected \"(\" actual: {@word} ", _tokens[_currentIndex].Word);
			throw new SyntaxErrorException("Syntax Error! Expected \")\" at token " + _currentIndex);
		}

		ParseNode blockNode = new ParseNode(NodeType.Block);
		Block(blockNode);
		node.assignChild(blockNode);
		try
		{
			ParseNode newNode = new ParseNode(NodeType.Terminal);
			if (Match(TokenType.String) && CheckElse())
			{
				Advance(newNode);
				node.assignChild(newNode);
				ParseNode newBlockNode = new ParseNode(NodeType.Block);
				Block(newBlockNode);
				node.assignChild(newNode);
			}
			else
			{
				_logger.Error("Syntax Error! Expected \"else\" actual: {@word} ", _tokens[_currentIndex].Word);
				throw new SyntaxErrorException("Syntax Error! Expected \"else\" at token " + _currentIndex);
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
			ParseNode newNode = new ParseNode(NodeType.Terminal);
			Advance(newNode);
			node.assignChild(newNode);
		}
		else
		{
			_logger.Error("Syntax Error! Expected \"while\" actual: {@word} ", _tokens[_currentIndex].Word);
			throw new SyntaxErrorException("Syntax Error! Expected \"while\" at token " + _currentIndex);
		}

		if (Match(TokenType.LeftParen))
		{
			ParseNode newNode = new ParseNode(NodeType.Terminal);
			Advance(newNode);
			node.assignChild(newNode);
		}
		else
		{
			_logger.Error("Syntax Error! Expected \"(\" actual: {@word} ", _tokens[_currentIndex].Word);
			throw new SyntaxErrorException("Syntax Error! Expected \"(\" at token " + _currentIndex);
		}
		ParseNode logicStatementNode = new ParseNode(NodeType.LogicStatement); 
		LogicStatement(logicStatementNode); //todo - check this is working correctly and won't cause an error
		node.assignChild(logicStatementNode);
		if (Match(TokenType.RightParen))
		{
			ParseNode newNode = new ParseNode(NodeType.Terminal);
			Advance(newNode);
			node.assignChild(newNode);
		}
		else
		{
			_logger.Error("Syntax Error! Expected \"(\" actual: {@word} ", _tokens[_currentIndex].Word);
			throw new SyntaxErrorException("Syntax Error! Expected \")\" at token " + _currentIndex);
		}

		ParseNode blockNode = new ParseNode(NodeType.Block);
		Block(blockNode);
		node.assignChild(blockNode);
	}

	//checks that there is a valid block as described in the EBNF above
	private void Block(ParseNode node) {
		if (Match(TokenType.LeftCurlyBrace))
		{
			ParseNode newNode = new ParseNode(NodeType.Terminal);
			Advance(newNode);
			node.assignChild(newNode);
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
				ParseNode newNode = new ParseNode(NodeType.Terminal);
				Advance(newNode);
				node.assignChild(newNode);
				return;
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
