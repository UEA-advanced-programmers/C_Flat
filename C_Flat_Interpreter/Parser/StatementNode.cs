using C_Flat_Interpreter.Common;
using C_Flat_Interpreter.Common.Enums;
namespace C_Flat_Interpreter.Parser
{

    public class StatementNode
    {
        public List<Token> statement;
        public BlockNode subBlock;
        public StatementNode()
        {
            statement = new List<Token>();
        }
        public int Load(List<Token> tokens, int index)
        {
            while (tokens.Count > index)
            {
                Token token = tokens[index];
                index++;
                if (token.Type == TokenType.RightCurlyBrace)
                {
                    index--;
                    return index;
                }
                
                else if (token.Type == TokenType.LeftCurlyBrace)
                {
                    statement.Add(token);
                    subBlock = new BlockNode();
                    index = subBlock.Load(tokens, index);
                    statement.Add(tokens[index]);
                    index++;
                    return index;
                }
                else if (token.Type == TokenType.SemiColon)
                {
                    statement.Add(token);
                    return index;
                }
                else
                {
                    statement.Add(token);
                }
            }
            return index;
        }
        public int Length
        {
            get {
                int length = statement.Count;
                if(subBlock != null)
                    length+=subBlock.Length;
                return length;
            }
        }
    }
}
