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
	private int _totalTokens;
	//private List<Token> _tokens; //TODO - define max with the group
	private StatementNode _statement;
	private int _indexBase;
	private int _statementIndex;
	//constructor
	public Parser()
	{
		GetLogger("Parser");
		//_tokens = new List<Token>();
	}

    //Helper Functions
    #region Helper Functions
	private bool CheckBoolLiteral()
	{
		var word = _statement.statement[_statementIndex].Word;//_tokens[_currentIndex].Word;
		if (word.Equals("true") || word.Equals("false")) return true;
		_logger.Error($"Bool parse error! Expected boolean literal, actual: \"{word}\"");
		return false;
	}
	private bool CheckIf()
    {
		var word = _statement.statement[_statementIndex].Word;//_tokens[_currentIndex].Word;
		if (word.Equals("if")) return true;
		_logger.Error($"If parse error! Expected if, actual: \"{word}\"");
		return false;
	}
	private bool CheckElse()
	{
		var word = _statement.statement[_statementIndex].Word;//_tokens[_currentIndex].Word;
		if (word.Equals("else")) return true;
		_logger.Error($"Else parse error! Expected else, actual: \"{word}\"");
		return false;
	}
	private bool CheckWhile()
	{
		var word = _statement.statement[_statementIndex].Word;//_tokens[_currentIndex].Word;
		if (word.Equals("while")) return true;
		_logger.Error($"While parse error! Expected while, actual: \"{word}\"");
		return false;
	}
	private bool Match(TokenType tokenType)
	{
		if (_tokenType == TokenType.Null) //only on first call,  TODO - find better way to do this that means we don't need to set to null and/or we don't need this check
        {
			_tokenType = _statement.statement[_statementIndex].Type;//_tokens[_currentIndex].Type;
		}

		if (tokenType == _tokenType)
		{
			_logger.Information("Token at index {index} matches {tokenType}", _indexBase+_statementIndex, tokenType);
		}
		else
		{
			_logger.Information("Token at index {index} does NOT MATCH {tokenType}", _indexBase + _statementIndex, tokenType);
			return false;
		}
		return true;
	}
	private Token GetToken()
    {
		return _statement.statement[_statementIndex];
    }
	private void Reset()
	{
		_statementIndex = 0;
	}
	private void Advance(int level)
	{
		if (++_statementIndex >= _statement.statement.Count())
			//_totalTokens) 
			return; //todo - might be able to find a way to exit everything else quicker when we're at the end - EOF token!
		_tokenType = _statement.statement[_statementIndex].Type;//_tokens[_currentIndex].Type;
		_logger.Information("advance() called at level {level}. Next token is {@token}", level, _statement.statement[_statementIndex]);//_tokens[_currentIndex]);
	}
    #endregion
    
	
	//End Helper Functions

	public int Parse(List<Token> tokens)
	{
		//_tokens = tokens;
		_totalTokens = tokens.Count;
		BlockNode block = new BlockNode();
		_indexBase = 0;
		block.Load(tokens, 0);
		ParseTree(block, 0);
		//if (_indexBase >= _totalTokens)
		return 0;
		_logger.Error("Syntax error! Could not parse Token: {@token}", _indexBase);//_tokens[_currentIndex]); //todo - Create test for this!
		return 1;
	}
	private void ParseTree(BlockNode block, int level)
    {
		_logger.Information("ParseTree() called at level {level}", level);
		for (int i = 0; i < block.statements.Count; i++)
		{
			_statement = block.statements[i];
			_statementIndex = 0;
            try
            {
				Statement(level + 1);
            }
            catch (Exception e)
            {
				_logger.Error("Syntax error! Could not parse Token: {@token}", _indexBase);
				throw;
            }
			_indexBase += _statement.Length;
		}
	}
	//EBNF Functions
	private void Statement(int level)
	{
		_logger.Information( "Statement() called at level {level}", level);
		try
		{
			Expression(level + 1);
			if (!Match(TokenType.SemiColon)) return; //this is terminal
		}
		catch (Exception e)
		{
			_logger.Warning(e.Message);
			Reset();
		}
		try
		{
			IfStatements(level + 1);
			return;
		}
		catch (Exception e)
		{
			_logger.Warning(e.Message);
			Reset();	
		}
		try
		{
			WhileStatement(level + 1);
			return;
		}
		catch (Exception e)
		{
			_logger.Warning(e.Message);
			Reset();
		}
		LogicStatement(level + 1);
		if (!Match(TokenType.SemiColon)) return; //this is terminal
	}

    #region Expressions
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
				throw new SyntaxErrorException("Syntax Error! Mismatched parentheses at token " + _indexBase + _statementIndex);
			}
		}
		else
		{
			throw new SyntaxErrorException("Syntax Error! Unexpected token at token " + _indexBase + _statementIndex);
		}
	}
    #endregion

    #region Boolean expressions
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
			var index = _statementIndex;//_currentIndex;
			try
			{
				ExpressionQuery(level+1);
			}
			catch (Exception e)
			{
				_logger.Warning(e.Message);
				_statementIndex = index;
				//_currentIndex = index;
				_tokenType = TokenType.Null;
				if (Match(TokenType.LeftParen))
				{
					Advance(level + 1);
					LogicStatement(level + 1);
					if (Match(TokenType.RightParen)) Advance(level + 1);
					else
					{
						_logger.Error("Syntax Error! Mismatched parentheses at token {index}", _indexBase + _statementIndex);
					}
				}
			}
		}
	}

	private void Condition(int level)
	{
		_logger.Information( "Condition() called at level {level}", level);

		if (!(Match(TokenType.Equals) || Match(TokenType.And) || Match(TokenType.Or)))
			return;
		if (Match(TokenType.Equals))
		{
			Advance(level);
			if (!Match(TokenType.Equals))
			{
				_logger.Error("Syntax Error! Mismatched equality operator, expected \"=\" actual: {@word} ", _statement.statement[_statementIndex].Word);
				//_tokens[_currentIndex].Word);
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
		if (Match(TokenType.Equals))
		{
			Advance(level);
			if (!Match(TokenType.Equals))
			{
				_logger.Error("Syntax Error! Mismatched equality operator, expected \"=\" actual: {@word} ",
					_statement.statement[_statementIndex].Word);//_tokens[_currentIndex].Word);
				return;
			}
		}
		Advance(level + 1);
		Expression(level + 1);
	}
    #endregion
    #region Conditions and iterators
	private void IfStatements(int level)
    {
		_logger.Information("if-statement() called at level {level}", level);
		if (Match(TokenType.String) && CheckIf()) 
			Advance(level + 1);
        else return;
		if (Match(TokenType.LeftParen)) Advance(level + 1);
        else
        {
            _logger.Error("Syntax Error! Expected \"(\" actual: {@word} ", _statement.statement[_statementIndex].Word);
			//_tokens[_currentIndex].Word);
			return;
        }
		LogicStatement(level + 1);
		if (Match(TokenType.RightParen)) Advance(level + 1);
		else
        {
			_logger.Error("Syntax Error! Expected \"(\" actual: {@word} ", _statement.statement[_statementIndex].Word);
			//_tokens[_currentIndex].Word);
			return;
		}
		SubBlock(level + 1);
		try
		{
			ElseStatements(level + 1);
		}
		catch (Exception e)
		{
			_logger.Warning(e.Message);
		}
	}
	private void ElseStatements(int level) {
		if (Match(TokenType.String))
		{
			if (CheckElse())
				Advance(level + 1);
			SubBlock(level + 1);
		}
	}
	private void WhileStatement(int level)
	{
		_logger.Information("while-statement() called at level {level}", level);
		if (Match(TokenType.String) && CheckWhile())
			Advance(level + 1);
		else return;
		if (Match(TokenType.LeftParen)) Advance(level + 1);
		else
		{
			_logger.Error("Syntax Error! Expected \"(\" actual: {@word} ", _statement.statement[_statementIndex].Word);
			//_tokens[_currentIndex].Word);
			return;
		}
		LogicStatement(level + 1);
		if (Match(TokenType.RightParen)) Advance(level + 1);
		else
		{
			_logger.Error("Syntax Error! Expected \"(\" actual: {@word} ", _statement.statement[_statementIndex].Word);
			//_tokens[_currentIndex].Word);
			return;
		}
		SubBlock(level + 1);
	}
	private void SubBlock(int level) {
		if (Match(TokenType.LeftCurlyBrace)) Advance(level + 1);
		else
		{
			_logger.Error("Syntax Error! Expected \"{\" actual: {@word} ", _statement.statement[_statementIndex].Word);
			//_tokens[_currentIndex].Word);
			return;
		}
		if (Match(TokenType.RightCurlyBrace)) Advance(level + 1);
		else
		{
			_logger.Error("Syntax Error! Expected \"}\" actual: {@word} ", _statement.statement[_statementIndex].Word);
			//_tokens[_currentIndex].Word);
			return;
		}
		try
		{
			ParseTree(_statement.subBlock, level + 1);
		}
		catch (Exception e)
		{
			_logger.Warning(e.Message);
		}
	}
	#endregion
	//EBNF Functions
}
