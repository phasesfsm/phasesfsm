using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Phases.Expresions
{
    class LexicalFormater
    {
        StringBuilder text;

        public LexicalFormater(LexicalAnalyzer lexic)
        {
            text = new StringBuilder();
            if (lexic.Tokens.Count == 0) return;
            Token.Types context = Token.Types.None;
            foreach (Token token in lexic.Tokens)
            {
                switch (context)
                {
                    case Token.Types.None:
                    case Token.Types.PrefixSymbol:
                        break;
                    default:
                        if(token.Type != Token.Types.SufixSymbol) text.Append(LexicalRules.DisposableChar);
                        break;
                }
                text.Append(token.Text);
                context = token.Type;
            }
        }

        public string Text
        {
            get
            {
                return text.ToString();
            }
        }

        public static string AddPrefixToVariables(string expresion, string prefix)
        {
            LexicalAnalyzer lexical = new LexicalAnalyzer();
            lexical.Source = expresion;
            lexical.AddPrefixToIds(prefix);
            LexicalFormater lexicalFormater = new LexicalFormater(lexical);
            return lexicalFormater.Text;
        }
    }
}
