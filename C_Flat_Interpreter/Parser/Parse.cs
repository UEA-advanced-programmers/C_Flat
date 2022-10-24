
public abstract class Parser
{

	int lookAhead;
	int currentToken; // Current index of token in Tokens
	int ret;

	// Temporary Vals to get working (delete once classes are connected)

	int[] Tokens;
	int NR_Tokens;

	int T_PLUS = 1;
	int T_NR = 2;
	int T_LPAR = 3;
	int T_RPAR = 4;
	int T_TIMES = 5;

	// end of temp vals
	struct TreeData
	{
		int level;
		int ancestor;
		string name;
		int value;
	};


	int match(int token)
	{
		int result;
		if (lookAhead == -1)
        {
			lookAhead = Tokens[currentToken];
		}

		if (token == lookAhead)
		{
			result = token;
			Console.WriteLine("Token[%d] =  %d matched\n" + currentToken + token);
		}
		else
		{
			result = token;
			Console.WriteLine("Token[%d] =  %d NOT matched\n" + currentToken + token);
		}

		return result;
	}

	void advance(int level)
	{
		lookAhead = Tokens[++currentToken];
		Console.WriteLine( "advance() called at level %d with next token %d\n" + level + lookAhead);
	}

	int Parse()
	{
		currentToken = 0;
		lookAhead = -1; 
		ret = 1;
		expression(0);
		if (currentToken < NR_Tokens)
		{
			Console.WriteLine( "SYNTAX ERROR - token %d is of type %d!\n" + currentToken + Tokens[currentToken]);
			ret = 0;
		}
		return ret;
	}

	void expression(int level)
	{
		Console.WriteLine( "expression() called at level %d\n" + level);
		term(level + 1);
		expression_p(level + 1);
	}

	void term(int level)
	{
		Console.WriteLine( "term() called at level %d\n" + level);
		factor(level + 1);
		term_p(level + 1);
	}

	void expression_p(int level)
	{
		Console.WriteLine( "expression_p() called at level %d\n" + level);
		if (0 < match(T_PLUS))
		{
			advance(level + 1);
			term(level + 1);
			expression_p(level + 1);
		}
	}

	void factor(int level)
	{
		Console.WriteLine( "factor() called at level %d\n" + level);
		if (0 < match(T_NR))
		{
			advance(level + 1);
		}
		else if (0 < match(T_LPAR))
		{
			advance(level + 1);
			expression(level + 1);
			if (0 < match(T_RPAR)) advance(level + 1);
			else
			{
				Console.WriteLine("SYNTAX ERROR: Mismatched parentheses at token %d\n" + currentToken);
				ret = 0;
			}
		}
		else
		{
			Console.WriteLine("SYNTAX ERROR: Number expected at token %d\n" + currentToken);
			ret = 0;
		}
	}

	void term_p(int level)
	{
		Console.WriteLine( "term_p() called at level %d\n" + level);
		if (0 < match(T_TIMES))
		{
			advance(level + 1);
			factor(level + 1);
			term_p(level + 1);
		}
	}


}
