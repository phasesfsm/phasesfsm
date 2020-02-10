using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Phases.Variables;

namespace Phases.Expresions
{
    class LexicalAnalyzer
    {
        List<Token> tokens;
        string source = "";

        public LexicalAnalyzer()
        {
            tokens = new List<Token>();
        }

        private void Analyze()
        {
            Token token;
            int index = 0;
            token = LexicalRules.GetNextToken(source, ref index);
            while (token != Token.Empty)
            {
                tokens.Add(token);
                token = LexicalRules.GetNextToken(source, ref index);
            }
        }

        public string Source
        {
            get
            {
                return source;
            }
            set
            {
                tokens.Clear();
                source = value;
                Analyze();
            }
        }

        public void AddPrefixToIds(string prefix)
        {
            foreach(Token token in tokens)
            {
                if (token.Type == Token.Types.Id)
                {
                    token.AddPrefix(prefix);
                }
            }
        }

        public List<Token> Tokens
        {
            get
            {
                return tokens;
            }
        }

        public static string GetId(string expresion)
        {
            StringBuilder expr = new StringBuilder(expresion);
            string prefix = LexicalRules.PrefixSymbols.FirstOrDefault(str => expr.ToString().StartsWith(str));
            if (prefix != null)
            {
                expr.Remove(0, prefix.Length);
            }
            string sufix = LexicalRules.SufixSymbols.FirstOrDefault(str => expr.ToString().EndsWith(str));
            if (sufix != null)
            {
                expr.Remove(expr.Length - sufix.Length, sufix.Length);
            }
            return expr.ToString();
        }

        public static string GetOutputId(string expresion)
        {
            StringBuilder expr = new StringBuilder(expresion);
            string prefix = LexicalRules.OutputPrefixSymbols.FirstOrDefault(str => expr.ToString().StartsWith(str));
            if (prefix != null)
            {
                expr.Remove(0, prefix.Length);
            }
            string sufix = LexicalRules.OutputSufixSymbols.FirstOrDefault(str => expr.ToString().EndsWith(str));
            if (sufix != null)
            {
                expr.Remove(expr.Length - sufix.Length, sufix.Length);
            }
            return expr.ToString();
        }

        public static OperationType GetOperation(string expresion)
        {
            StringBuilder expr = new StringBuilder(expresion);
            string prefix = LexicalRules.OutputPrefixSymbols.FirstOrDefault(str => expr.ToString().StartsWith(str));
            if (prefix != null)
            {
                switch (prefix)
                {
                    case "!":
                        return OperationType.Clear;
                    default:
                        return OperationType.Unknown;
                }
            }
            string sufix = LexicalRules.OutputSufixSymbols.FirstOrDefault(str => expr.ToString().EndsWith(str));
            if (sufix != null)
            {
                switch (sufix)
                {
                    case "+":
                        return OperationType.Increment;
                    case "-":
                        return OperationType.Decrement;
                    case ".max":
                        return OperationType.Maximum;
                    case ".min":
                        return OperationType.Minimum;
                    default:
                        return OperationType.Unknown;
                }
            }
            return OperationType.None;
        }
    }
}
