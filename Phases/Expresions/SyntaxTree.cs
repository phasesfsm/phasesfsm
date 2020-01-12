using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Phases.Expresions
{
    class SyntaxTree
    {
        private SyntaxToken root;

        public SyntaxTree()
        {
            
        }

        public SyntaxToken Root => root;

        private SyntaxToken GetRoot(List<SyntaxToken> tokens, ref int index)
        {
            SyntaxToken token, previous = null;
            SyntaxToken previousId = null, previousOperator = null;
            for (; index < tokens.Count; index++)
            {
                token = tokens[index];
                if(token.Type == Token.Types.GroupBegins)
                {
                    index++;
                    token = GetRoot(tokens, ref index);
                    if(previousOperator != null)
                    {
                        previousOperator.RightChild = token;
                    }
                }
                else if(token.Type == Token.Types.GroupEnds)
                {
                    break;
                }
                else if (token.Type == Token.Types.Id || token.Type == Token.Types.Numeric)
                {
                    if (previousOperator == null)
                    {
                        
                    }
                    else
                    {
                        previousOperator.RightChild = token;
                    }
                    previousId = token;
                }
                else if (token.Type == Token.Types.OperatorSymbol)
                {
                    if(previousOperator == null)
                    {
                        token.LeftChild = previous;
                    }
                    else
                    {
                        if(token.Priority < previousOperator.Priority)
                        {
                            previousOperator.RightChild = token;
                            token.LeftChild = previousId;
                        }
                        else
                        {
                            if (previousOperator.Father != null)
                            {
                                previousOperator.Father.RightChild = token;
                            }
                            token.LeftChild = previousOperator;
                        }
                    }
                    previousOperator = token;
                }
                else if(token.Type == Token.Types.PrefixSymbol)
                {
                    if(previousOperator != null)
                    {
                        previousOperator.RightChild = token;
                    }
                    previousOperator = token;
                }
                previous = token;
            }
            while (previous != null && previous.Father != null)
            {
                previous = previous.Father;
            }
            return previous;
        }

        public bool CreateTree(SyntaxAnalyzer syntaxAnalyzer)
        {
            if (syntaxAnalyzer.Tokens.Any(token => token.Qualifier != SyntaxToken.Qualifiers.Correct)) return false;
            int index = 0;
            syntaxAnalyzer.Tokens.ForEach(token => { token.LeftChild = null; token.RightChild = null; });
            root = GetRoot(syntaxAnalyzer.Tokens, ref index);
            return true;
        }
    }
}
