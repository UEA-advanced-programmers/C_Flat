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
    private ScopeManager _scopeManager = new();

    private readonly Dictionary<NodeType, NodeFuncDelegate> _statementsDictionary = new();
    private readonly Dictionary<NodeType, NodeFuncDelegate> _assignmentValueDictionary = new();

    private delegate void NodeFuncDelegate(ParseNode node);

    // Constructor
    public Parser()
    {
        GetLogger("Parser");
        // Add all statements that can simply be added as a node without the need for extra checks 
        _statementsDictionary.Add(NodeType.ConditionalStatement, ConditionalStatement);
        _statementsDictionary.Add(NodeType.WhileStatement, WhileStatement);
        _statementsDictionary.Add(NodeType.VariableDeclaration, VariableDeclaration);
        
        
        // Add all variable assignment values
        _assignmentValueDictionary.Add(NodeType.LogicStatement, LogicStatement);
        _assignmentValueDictionary.Add(NodeType.Expression, Expression);
        _assignmentValueDictionary.Add(NodeType.String, String);
        _assignmentValueDictionary.Add(NodeType.FunctionCall, FunctionCall);
        
        _tokens = new List<Token>();
    }

    public List<ParseNode> GetParseTree()
    {
        return _parseTree;
    }

    // Helper Functions
    private bool CheckBoolLiteral()
    {
        var word = _tokens[_currentIndex].ToString();
        return word is "true" or "false";
    }
    private bool CheckKeyword(string keyword)
    {
        var word = _tokens[_currentIndex].ToString();
        return word == keyword;
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
        var currentToken = _tokens[_currentIndex];
        _tokenType = currentToken.Type;
        _currentLine = currentToken.Line + 1;
    }

    private void Advance()
    {
        if (++_currentIndex >= _totalTokens) return; //todo - might be able to find a way to exit everything else quicker when we're at the end - EOF token!
        var currentToken = _tokens[_currentIndex];
        _tokenType = currentToken.Type;
        _currentLine = currentToken.Line + 1;
    }

    private ParseNode CreateNode(NodeType type, NodeFuncDelegate nodeFunc)
    {
        ParseNode newNode = new ParseNode(type);
        nodeFunc(newNode);
        _logger.Information($"Successfully parsed {newNode.type}!");
        return newNode;
    }

    //End Helper Functions

    public int Parse(List<Token> tokens)
    {
        _tokens = tokens;
        _totalTokens = tokens.Count;
        _parseTree = new List<ParseNode>();
        VariableTable.Clear();
        _scopeManager.Reset();
        Reset();
        try
        {
            while (_currentIndex < _totalTokens)
            {
                var statementNode = new ParseNode(NodeType.Statement);
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

    private void Statement(ParseNode node)
    {
        int currentIndex = _currentIndex;

        foreach (var statement in _statementsDictionary)
        {
            try
            {
                node.AddChild(CreateNode(statement.Key, statement.Value));
                currentIndex = _currentIndex;
                return;
            }
            catch (InvalidSyntaxException e)
            {
                _logger.Warning(e.Message);
                Reset(currentIndex);
            }
        }
        
        try
        {
            
            var identifier = _tokens[_currentIndex].ToString();
            if (VariableTable.Exists(identifier))
            {
                node.AddChild(CreateNode(NodeType.VariableAssignment, VariableAssignment));
                currentIndex = _currentIndex;
                return;
            }
            throw new InvalidSyntaxException($"Invalid variable assignment! Variable '{_tokens.ElementAtOrDefault(_currentIndex)}' has not been declared!", _currentLine);
        }
        catch (InvalidSyntaxException e)
        {
            _logger.Debug(e.Message);
            Reset(currentIndex);
        }

        try
        {
            node.AddChild(CreateNode(NodeType.FunctionCall, FunctionCall));
            currentIndex = _currentIndex;
            // Check for ';'
            if (Match(TokenType.SemiColon))
            {
                node.AddChild(new ParseNode(NodeType.Terminal, _tokens[_currentIndex]));
                Advance();
            }
            else
            {
                throw new SyntaxErrorException($"Unterminated assignment, expected ';'. Actual: '{_tokens.ElementAtOrDefault(_currentIndex)}'", _currentLine);
            }
            return;
        }
        catch (InvalidSyntaxException e)
        {
            _logger.Debug(e.Message);
            Reset(currentIndex);
        }
        throw new InvalidSyntaxException("Unable to parse statement!");
    }
    
    private void VariableDeclaration(ParseNode node)
    {
        // check for String token with the name "var"
        if (Match(TokenType.Word) && CheckKeyword("var"))
        {
            node.AddChild(new ParseNode(NodeType.Terminal, _tokens[_currentIndex]));
            Advance();
        }
        else
        {
            throw new InvalidSyntaxException($"Expected keyword 'var'. Actual: '{_tokens.ElementAtOrDefault(_currentIndex)}'", _currentLine);
        }

        var identifier = _tokens[_currentIndex].ToString();
        int currentIndex = _currentIndex;

        //	Throw syntax error if variable already exists
        if (_scopeManager.InScope(identifier))
        {
            if (VariableTable.Exists(identifier))
                throw new SyntaxErrorException($"Variable '{_tokens.ElementAtOrDefault(_currentIndex)}' has already been declared!", _currentLine);
        }
        else
        {
            //	Re-declare if previously declared.
            if (VariableTable.Exists(identifier))
            {
                try
                {
                    VariableTable.Add(identifier);
                }
                catch (Exception e)
                {
                    throw new SyntaxErrorException(e.Message, _currentLine);
                }
            }
            _scopeManager.AddToScope(identifier);
        }

        // check if a value is being assigned
        try
        {
            ParseNode childNode = CreateNode(NodeType.VariableAssignment, VariableAssignment);
            node.AddChild(childNode);
            return;
        }
        catch (InvalidSyntaxException e)
        {
            Reset(currentIndex);
            _logger.Warning(e.Message);
        }

        node.AddChild(CreateNode(NodeType.VariableIdentifier, Identifier));

        try
        {
            VariableTable.Add(identifier);
        }
        catch (Exception e)
        {
            throw new SyntaxErrorException(e.Message, _currentLine);
        }

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

    private void Identifier(ParseNode node)
    {
        if (Match(TokenType.Word))
        {
            node.AddChild(new ParseNode(NodeType.Terminal, _tokens[_currentIndex]));
            Advance();
        }
        else
        {
            throw new InvalidSyntaxException($"Invalid identifier! Expected token type word. Actual: '{_tokenType}'", _currentLine);
        }
    }
    private void VariableAssignment(ParseNode node)
    {
        // check for identifier
        var identifierNode = CreateNode(NodeType.VariableIdentifier, Identifier);
        node.AddChild(identifierNode);
        
        if (!Match(TokenType.Assignment))
            throw new InvalidSyntaxException($"Invalid variable assignment, expected '='. Actual: '{_tokens.ElementAtOrDefault(_currentIndex)}'", _currentLine);

        node.AddChild(new ParseNode(NodeType.Terminal, _tokens[_currentIndex]));
        Advance();

        //	Try and parse assignment value
        try
        {
            var identifier = identifierNode.GetChild().token?.ToString() ?? throw new SyntaxErrorException("Variable has no identifier", _currentLine);
            var valueNode = CreateNode(NodeType.AssignmentValue, AssignmentValue);
            var assignmentValue = valueNode.GetChild();
            
            var assignmentValueType = assignmentValue.type;
            if(assignmentValueType is NodeType.FunctionCall)
            {
                var functionIdentifier = assignmentValue.GetChild().GetChild().token?.ToString() ?? "";
                var returnType = FunctionTable.GetReturnType(functionIdentifier);
                if (returnType is NodeType.Null)
                    throw new IncorrectTypeException("You cannot assign a function which returns null here!", _currentLine);
                assignmentValueType = returnType;
            }

            //	Check whether the value node is of the same type and is in scope
            if (VariableTable.Exists(identifier))
            {
                if (_scopeManager.InScope(identifier))
                {
                    var existingType = VariableTable.GetType(identifier);
                    if (existingType != NodeType.Null && existingType != assignmentValueType)
                    {
                        throw new SyntaxErrorException($"Invalid variable assignment! Type '{assignmentValueType}' cannot be assigned to '{identifier}' of type '{existingType}'", _currentLine);
                    }
                }
                else
                {
                    throw new OutOfScopeException($"Variable '{identifier}' does not exist in this context", _currentLine);
                }
            }
            try
            {
                VariableTable.Add(identifier, assignmentValueType);
            }
            catch (Exception e)
            {
                throw new SyntaxErrorException(e.Message, _currentLine);
            }
            //	Only add if the assignment is valid
            node.AddChild(valueNode);
        }
        catch (InvalidSyntaxException)
        {
            throw new SyntaxErrorException($"Invalid assignment value, unable to parse variable assignment", _currentLine);
        }

        //	Finally check for termination
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

    private void AssignmentValue(ParseNode node)
    {
        //	Store current index
        int index = _currentIndex;
        
        foreach (var assignmentValue in _assignmentValueDictionary)
        {
            try
            {
                node.AddChild(CreateNode(assignmentValue.Key, assignmentValue.Value));
                return;
            }
            catch (ParserException e)
            {
                if (e is SyntaxErrorException)
                    throw;
                Reset(index);
                _logger.Debug(e.Message);
            }
        }
        throw new SyntaxErrorException("Unable to parse assignment value'", _currentLine); 
    }

    private void String(ParseNode node)
    {
        if (Match(TokenType.String))
        {
            node.AddChild(new ParseNode(NodeType.Terminal, _tokens[_currentIndex]));
            Advance();
        }
        else if(Match(TokenType.Word))
        {
            var identifier = _tokens[_currentIndex].ToString();
            if (VariableTable.Exists(identifier))
            {
                if(VariableTable.GetType(identifier) is not NodeType.String)
                    throw new IncorrectTypeException($"Variable '{identifier}' is type '{VariableTable.GetType(identifier)}' expected a string");
                node.AddChild(CreateNode(NodeType.VariableIdentifier, Identifier));

            }
            else
                throw new InvalidSyntaxException("Variable does not exist in table", _currentLine);
        }
        else
        {
            throw new InvalidSyntaxException($"Invalid string, expected type 'String'. Actual: '{_tokens.ElementAtOrDefault(_currentIndex)}'", _currentLine);
        }
    }

    private void FunctionCall(ParseNode node)
    {
        //Check for function identifier
        try
        {
            var identifierNode = CreateNode(NodeType.FunctionIdentifier, Identifier);
            var identifier = identifierNode.GetChild().token?.ToString() ??
                             throw new SyntaxErrorException("Function has no identifier", _currentLine);
            if (!FunctionTable.Exists(identifier))
                throw new InvalidSyntaxException($"Function '{identifier}' does not exist!", _currentLine);
            node.AddChild(identifierNode);
            
            // Check correct arguments are being passed
            var parameters = FunctionTable.GetParams(identifier);
            // Check for '('
            if (Match(TokenType.LeftParen))
            {
                node.AddChild(new ParseNode(NodeType.Terminal, _tokens[_currentIndex]));
                Advance();
            }
            else
            {
                throw new InvalidSyntaxException("Unexpected token, unable to parse a function call!");
            }
            
            foreach (var param in parameters)
            {
                var functionArgument = CreateNode(NodeType.AssignmentValue, AssignmentValue);

                var argumentValue = functionArgument.GetChild();
                var argumentType = argumentValue.type;
                if(argumentType is NodeType.FunctionCall)
                {
                    var functionIdentifier = argumentValue.GetChild().GetChild().token?.ToString() ?? "";
                    var returnType = FunctionTable.GetReturnType(functionIdentifier);
                    if (returnType is NodeType.Null)
                        throw new IncorrectTypeException("You cannot pass a function which returns null here!");
                    argumentType = returnType;
                }
                
                //  Ensure argument is same type or the parameter has a null argument type
                if (argumentType == param || param is NodeType.Null)
                {
                    node.AddChild(functionArgument);
                }
                else
                {
                    throw new SyntaxErrorException($"Argument type '{argumentValue.type}' is not the correct type, expected type '{param}'");
                }
                
                if (!Match(TokenType.Comma))
                {
                    break;
                }
                node.AddChild(new ParseNode(NodeType.Terminal, _tokens[_currentIndex]));
                Advance();
            }

            // Check for ')'
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
        catch (SyntaxErrorException e)
        {
            _logger.Warning(e.Message);
            throw new InvalidSyntaxException($"Invalid function!", _currentLine);
        }
    }

    private void Expression(ParseNode node)
    {
        //	Try add first term node
        node.AddChild(CreateNode(NodeType.Term, Term));

        //	Check for '+' | '-' 
        if (!Match(TokenType.Add) && !Match(TokenType.Sub)) return;
        node.AddChild(new ParseNode(NodeType.Terminal, _tokens[_currentIndex]));
        Advance();

        //	Add second term node
        try
        {
            node.AddChild(CreateNode(NodeType.Term, Term));
        }
        catch (ParserException e)
        {
            if (e is SyntaxErrorException)
                throw;
            _logger.Warning(e.Message);
            throw new SyntaxErrorException("Invalid right expression operand", _currentLine);
        }
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
        try
        {
            node.AddChild(CreateNode(NodeType.Factor, Factor));
        }
        catch (ParserException e)
        {
            if (e is SyntaxErrorException)
                throw;
            _logger.Warning(e.Message);
            throw new SyntaxErrorException("Invalid right term operand", _currentLine);
        }
    }

    private void Factor(ParseNode node)
    {
        // Parse factor or throw
        if (Match(TokenType.Num))
        {
            node.AddChild(new ParseNode(NodeType.Terminal, _tokens[_currentIndex]));
            Advance();
        }
        else if (Match(TokenType.Word))
        {
            var identifierNode = CreateNode(NodeType.VariableIdentifier, Identifier);
            //	Check whether the value node is of the same type
            var identifier = identifierNode.GetChild().token?.ToString() ?? throw new SyntaxErrorException("Variable has no identifier", _currentLine);
            if (VariableTable.Exists(identifier))
            {
                if (VariableTable.GetType(identifier) is not NodeType.Expression)
                    throw new IncorrectTypeException($"Variable {identifier} is not of type 'Logic Statement'", _currentLine);
            }
            else 
                throw new InvalidSyntaxException("Variable does not exist in table", _currentLine);
            node.AddChild(identifierNode);
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
            _logger.Debug(e.Message);
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
        else if (Match(TokenType.Word) && CheckBoolLiteral())
        {
            node.AddChild(new ParseNode(NodeType.Terminal, _tokens[_currentIndex]));
            Advance();
        }
        else
        {
            //	Temporarily save index
            var currentIndex = _currentIndex;
            //	Try and parse expression query 
            try
            {
                node.AddChild(CreateNode(NodeType.ExpressionQuery, ExpressionQuery));
                return;
            }
            catch (ParserException e)
            {
                if (e is SyntaxErrorException)
                {
                    throw;
                }
                _logger.Debug(e.Message);
                Reset(currentIndex);
            }
            //	Try and parse variable identifier
            try
            {
                var identifierNode = CreateNode(NodeType.VariableIdentifier, Identifier);
                var identifier = identifierNode.GetChild().token?.ToString()?? throw new SyntaxErrorException("Variable has no identifier", _currentLine);
                // Check whether type is correct
                if (VariableTable.Exists(identifier))
                {
                    if (VariableTable.GetType(identifier) is not NodeType.LogicStatement)
                        throw new IncorrectTypeException($"Variable {identifier} is not of type 'Logic Statement'", _currentLine);
                    node.AddChild(identifierNode);
                }
                else
                    throw new InvalidSyntaxException("Variable does not exist in table", _currentLine);
                return;
            }
            catch (InvalidSyntaxException e)
            {
                _logger.Debug(e.Message);
            }
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

    private void Condition(ParseNode node)
    {
        //	Throw exception if token doesn't match any operators
        if (!Match(TokenType.NotEqual) && !Match(TokenType.Equals) && !Match(TokenType.And) &&
            !Match(TokenType.Or)) throw new InvalidSyntaxException("Unexpected token, unable to parse a condition");

        node.AddChild(new ParseNode(NodeType.Terminal, _tokens[_currentIndex]));
        Advance();

        //	Parse the right boolean operand
        try
        {
            node.AddChild(CreateNode(NodeType.Boolean, Boolean));
        }
        catch (ParserException e)
        {
            _logger.Warning(e.Message);
            throw new SyntaxErrorException("Incorrect type on condition operand", _currentLine);
        }
    }

    private void ExpressionQuery(ParseNode node)
    {
        //	Try and parse left expression operand
        node.AddChild(CreateNode(NodeType.Expression, Expression));

        //TODO: check whether this could cause out of bounds exception!
        //	Throw exception if token doesn't match any operators
        if (!Match(TokenType.NotEqual) && !Match(TokenType.Equals) && !Match(TokenType.More) &&
            !Match(TokenType.Less)) throw new InvalidSyntaxException($"Invalid operator token, expected '!=' | '==' | '>' | '<'. Actual: '{_tokens.ElementAtOrDefault(_currentIndex)}'", _currentLine);

        node.AddChild(new ParseNode(NodeType.Terminal, _tokens[_currentIndex]));
        Advance();

        //	Parse right expression operand
        try
        {
            node.AddChild(CreateNode(NodeType.Expression, Expression));
        }
        catch (ParserException e)
        {
            _logger.Warning(e.Message);
            throw new SyntaxErrorException($"Invalid right expression query operand!", _currentLine);
        }
    }

    private void ConditionalStatement(ParseNode node)
    {
        if (Match(TokenType.Word) && CheckKeyword("if"))
        {
            node.AddChild(new ParseNode(NodeType.Terminal, _tokens[_currentIndex]));
            Advance();
        }
        else
        {
            throw new InvalidSyntaxException("Unexpected token, unable to parse conditional statement!", _currentLine);
        }

        if (Match(TokenType.LeftParen))
        {
            node.AddChild(new ParseNode(NodeType.Terminal, _tokens[_currentIndex]));
            Advance();
        }
        else
        {
            throw new SyntaxErrorException($"Unexpected token, expected '('. Actual: '{_tokens.ElementAtOrDefault(_currentIndex)}'", _currentLine);
        }

        //	Throw an invalid syntax error if we fail to parse a logic statement
        try
        {
            node.AddChild(CreateNode(NodeType.LogicStatement, LogicStatement));
        }
        catch (ParserException e)
        {
            _logger.Warning(e.Message);
            throw new SyntaxErrorException($"Invalid logic statement within conditional statement!", _currentLine);
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
        catch (ParserException e)
        {
            _logger.Warning(e.Message);
            throw new SyntaxErrorException($"Invalid block within if statement!", _currentLine);
        }

        if (!Match(TokenType.Word) || !CheckKeyword("else")) return;
        {
            node.AddChild(new ParseNode(NodeType.Terminal, _tokens[_currentIndex]));
            Advance();

            //	Parse else statement block, throwing if invalid
            try
            {
                node.AddChild(CreateNode(NodeType.Block, Block));
            }
            catch (InvalidSyntaxException e)
            {
                _logger.Warning(e.Message);
                throw new SyntaxErrorException($"Invalid block within else statement!", _currentLine);
            }
        }
    }

    private void WhileStatement(ParseNode node)
    {
        if (Match(TokenType.Word) && CheckKeyword("while"))
        {
            node.AddChild(new ParseNode(NodeType.Terminal, _tokens[_currentIndex]));
            Advance();
        }
        else
        {
            throw new InvalidSyntaxException("Unexpected token, unable to parse while statement!", _currentLine);
        }

        if (Match(TokenType.LeftParen))
        {
            node.AddChild(new ParseNode(NodeType.Terminal, _tokens[_currentIndex]));
            Advance();
        }
        else
        {
            throw new SyntaxErrorException($"Unexpected token, expected '('. Actual: '{_tokens.ElementAtOrDefault(_currentIndex)}'", _currentLine);
        }

        //	Throw an invalid syntax error if we fail to parse a logic statement
        try
        {
            node.AddChild(CreateNode(NodeType.LogicStatement, LogicStatement));
        }
        catch (ParserException e)
        {
            _logger.Warning(e.Message);
            throw new SyntaxErrorException($"Invalid logic statement within while statement!", _currentLine);
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
        catch (InvalidSyntaxException e)
        {
            _logger.Warning(e.Message);
            throw new SyntaxErrorException($"Invalid block within while statement!", _currentLine);
        }
    }

    private void Block(ParseNode node)
    {
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

        // Keep track of previous scope count
        var scopeCount = _scopeManager.ScopeCount();

        while (_currentIndex < _totalTokens && !Match(TokenType.RightCurlyBrace))
        {
            try
            {
                node.AddChild(CreateNode(NodeType.Statement, Statement));
            }
            catch (InvalidSyntaxException e)
            {
                _logger.Warning(e.Message);
                throw new SyntaxErrorException($"Invalid statement within block!", _currentLine);
            }
        }

        if (Match(TokenType.RightCurlyBrace))
        {
            node.AddChild(new ParseNode(NodeType.Terminal, _tokens[_currentIndex]));
            Advance();
            _scopeManager.DeScope(scopeCount);
        }
        else
        {
            throw new SyntaxErrorException("Block not terminated! Expected '}'." + $" Actual: '{_tokens.ElementAtOrDefault(_currentIndex)}'", _currentLine);
        }
    }
}
