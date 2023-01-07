namespace C_Flat_Interpreter.Common;

public class ScopeManager : InterpreterLogger
{
    private Stack<string> _scopedVariables = new();

    public ScopeManager()
    {
        GetLogger("Scope Manager");
    }
    public bool InScope(string identifier)
    {
        return _scopedVariables.Contains(identifier);
    }

    public void DeScope(int previousCount)
    {
        for(int i  = 0; i < _scopedVariables.Count - previousCount; i++)
        {
            var identifier = _scopedVariables.Pop();
            _logger.Information("Removed variable '{identifier}' from scope", identifier);
        }
    }

    public int ScopeCount()
    {
        return _scopedVariables.Count;
    }

    public void AddToScope(string identifier)
    {
        _scopedVariables.Push(identifier);
        _logger.Information("Added variable '{identifier}' to scope", identifier);
    }

    public void Reset()
    {
        _scopedVariables.Clear();
    }

    //TODO: Remove this and turn into a unit test
    public void TestScopeManager()
    {
        var test = InScope("ramdom");
        AddToScope("Sugar");
        AddToScope("Salt");
        AddToScope("Pepper");
        var pepperScoped = InScope("Pepper");
        DeScope(1);
        pepperScoped = InScope("Pepper");
        var saltScoped = InScope("Salt");
        var sugarScoped = InScope("Sugar");
    }
}