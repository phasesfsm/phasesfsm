using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Phases.Variables;

namespace Phases.Expresions
{
    class SyntaxAnalyzer
    {
        public List<SyntaxToken> Tokens { get; private set; }

        public SyntaxAnalyzer(LexicalAnalyzer lexic, List<string> variables)
        {
            Tokens = new List<SyntaxToken>();
            Stack<Token> branches = new Stack<Token>();
            Token.Types context = Token.Types.None;
            Token previous = null;
            SyntaxToken.Qualifiers qualifiers;
            int groupLevel = 0;

            foreach (Token token in lexic.Tokens)
            {
                qualifiers = SyntaxToken.Qualifiers.Correct;
                switch (token.Type)
                {
                    case Token.Types.Id:
                        if (variables.Contains(token.Text))
                        {
                            if (context == Token.Types.Id || context == Token.Types.SufixSymbol
                                || context == Token.Types.GroupEnds) qualifiers = SyntaxToken.Qualifiers.Unexpected;
                        }
                        else
                        {
                            qualifiers = SyntaxToken.Qualifiers.Wrong;
                        }
                        break;
                    case Token.Types.OperatorSymbol:
                        if (context == Token.Types.GroupBegins || context == Token.Types.None
                            || context == Token.Types.OperatorSymbol || context == Token.Types.PrefixSymbol) qualifiers = SyntaxToken.Qualifiers.Unexpected;
                        break;
                    case Token.Types.PrefixSymbol:
                        if (context == Token.Types.Id || context == Token.Types.SufixSymbol) qualifiers = SyntaxToken.Qualifiers.Unexpected;
                        else if (context == Token.Types.PrefixSymbol && previous.Text != "!") qualifiers = SyntaxToken.Qualifiers.Unexpected;
                        break;
                    case Token.Types.SufixSymbol:
                        if (context != Token.Types.Id) qualifiers = SyntaxToken.Qualifiers.Unexpected;
                        break;
                    case Token.Types.Numeric:
                        if (context != Token.Types.OperatorSymbol) qualifiers = SyntaxToken.Qualifiers.Unexpected;
                        else if (context == Token.Types.OperatorSymbol && (previous.Text != "=" || previous.Text != "!=")) qualifiers = SyntaxToken.Qualifiers.InvalidUseOf;
                        break;
                    case Token.Types.GroupBegins:
                        groupLevel++;
                        branches.Push(token);
                        if (context == Token.Types.SufixSymbol || context == Token.Types.Id
                            || context == Token.Types.GroupEnds) qualifiers = SyntaxToken.Qualifiers.Unexpected;
                        break;
                    case Token.Types.GroupEnds:
                        groupLevel--;
                        if (groupLevel > 0) branches.Pop();
                        if (groupLevel < 0 || context == Token.Types.PrefixSymbol
                            || context == Token.Types.OperatorSymbol || context == Token.Types.None
                            || context == Token.Types.GroupBegins) qualifiers = SyntaxToken.Qualifiers.Unexpected;
                        break;
                }
                Tokens.Add(new SyntaxToken(token, qualifiers));
                context = token.Type;
                previous = token;
            }
            if(groupLevel > 0)  //Mising ')' error
            {
                while (branches.Count > 0)
                {
                    Tokens.Add(new SyntaxToken(branches.Pop(), SyntaxToken.Qualifiers.NonClosed));
                }
            }
        }
    }
}
