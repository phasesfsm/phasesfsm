using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Phases.Expresions
{
    class Token
    {
        public enum Types
        {
            None,
            Invalid,
            Id,
            Numeric,
            OperatorSymbol,
            SufixSymbol,
            UnionSymbol,
            PrefixSymbol,
            GroupBegins,
            GroupEnds
        }

        Types tokenType;
        int sourceIndex;
        int priority;

        public const Token Empty = null;

        public Token(Types tokenType, string text, int sourceIndex)
        {
            this.tokenType = tokenType;
            Text = text;
            this.sourceIndex = sourceIndex;
            if(tokenType == Types.OperatorSymbol)
            {
                priority = LexicalRules.OperatorsPriority[LexicalRules.OperatorSymbols.ToList().IndexOf(text)];
            }
            else
            {
                priority = 0;
            }
        }

        public Types Type => tokenType;

        public string Text { get; set; }

        public int SourceIndex => sourceIndex;

        public int Priority => priority;

        public override string ToString()
        {
            return tokenType.ToString() + "{" + Text + "}";
        }

        public void AddPrefix(string prefix)
        {
            Text = prefix + Text;
        }
    }
}
