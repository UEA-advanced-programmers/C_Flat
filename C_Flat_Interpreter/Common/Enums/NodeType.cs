namespace C_Flat_Interpreter.Common.Enums;

public enum NodeType //will be added to
{
    Null,
    Statement,
    TopLevelStatement,
    ControlStatement,
    Expression,
    Term,
    Factor,
    Terminal,
    LogicStatement,
    Condition,
    Boolean,
    ExpressionQuery,
    ConditionalStatement,
    WhileStatement,
    VariableDeclaration,
    VariableIdentifier,
    VariableAssignment,
    AssignmentValue,
    String,
    FunctionDefinition,
    FunctionIdentifier,
    Parameter,
    FunctionCall,
    Block
}