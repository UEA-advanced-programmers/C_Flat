using C_Flat_Interpreter.Common;
using C_Flat_Interpreter.Common.Enums;
namespace C_Flat_Interpreter.Parser
{
    public class BlockNode
    {
        public List<StatementNode> statements;
        public BlockNode()
        {
            statements = new List<StatementNode>();
        }
        public int Load(List<Token> tokens, int index)
        {
            while (tokens.Count > index)
            {
                Token token = tokens[index];
                if (tokens[index].Type == TokenType.RightCurlyBrace)
                {
                    return index;
                }
                else
                {
                    StatementNode statement = new StatementNode();
                    index = statement.Load(tokens, index);
                    statements.Add(statement);
                }
            }
            return index;
        }
        public int Length
        {
            get
            {
                int length = 0;
                foreach (StatementNode statement in statements)
                    length+=statement.Length;
                return length;
            }
        }
    }
}
