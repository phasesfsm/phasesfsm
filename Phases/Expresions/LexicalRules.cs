using Phases.Variables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Phases.Expresions
{
    sealed class LexicalRules
    {
        public const char DisposableChar = ' ';
        public const char GroupBeginsChar = '(';
        public const char GroupEndsChar = ')';
        public static readonly Predicate<char> ValidDisposableChar = ch => ch == DisposableChar;

        public static readonly string[] OperatorSymbols = { "&", "^", "|", "=", "!=" };
        public static readonly int[] OperatorsPriority = { 8, 9, 10, 7, 7 };
        public static readonly string[] PrefixSymbols = { "!" };
        public static readonly string[] UnionSymbols = { "." };
        public static readonly string[] SufixSymbols = { ".max", ".min" };
        public static readonly string[] OutputPrefixSymbols = { "!", "~", "»" };
        public static readonly string[] OutputUnionSymbols = { };
        public static readonly string[] OutputSufixSymbols = { "+", "-", ".max", ".min" };
        public static readonly Predicate<string> ValidOperatorSymbol = str => OperatorSymbols.Contains(str) || PrefixSymbols.Contains(str) || SufixSymbols.Contains(str);
        public static readonly Predicate<string> ValidPrefixSymbol = str => PrefixSymbols.Contains(str);
        public static readonly Predicate<string> ValidUnionSymbol = str => UnionSymbols.Contains(str);
        public static readonly Predicate<string> ValidSufixSymbol = str => SufixSymbols.Contains(str);

        public static readonly Predicate<char> ValidStartIdExpressionChar = ch => (ch >= 'A' && ch <= 'Z') || (ch >= 'a' && ch <= 'z') || ch == '_' || ch == '.';
        public static readonly Predicate<char> ValidNumericExpressionChar = ch => ch >= '0' && ch <= '9';
        public static readonly Predicate<char> ValidIdExpressionChar = ch => ValidStartIdExpressionChar(ch) || ValidNumericExpressionChar(ch);
        public static readonly Predicate<char> ValidGroupBeginsChar = ch => ch == GroupBeginsChar;
        public static readonly Predicate<char> ValidGroupEndsChar = ch => ch == GroupEndsChar;

        public static readonly Predicate<string> ValidString = str => str != null && str.Length > 0;
        public static readonly Predicate<string> ValidIdExpression = str => ValidString(str) && ValidStartIdExpressionChar(str.First()) && (str.Length == 1 || str.Substring(1).All(ch => ValidIdExpressionChar(ch)));

        public static Token.Types GetCharToken(Token.Types context, ref string text, int startIndex, int index)
        {
            char ch = text[index];
            switch (context)
            {
                case Token.Types.None:
                    if (ValidDisposableChar(ch)) return Token.Types.None;
                    if (ValidGroupBeginsChar(ch)) return Token.Types.GroupBegins;
                    if (ValidGroupEndsChar(ch)) return Token.Types.GroupEnds;
                    if (ValidStartIdExpressionChar(ch)) return Token.Types.Id;
                    if (PrefixSymbols.Any(str => str.First() == ch)) return Token.Types.PrefixSymbol;
                    if (SufixSymbols.Any(str => str.First() == ch)) return Token.Types.SufixSymbol;
                    if (OperatorSymbols.Any(str => str.First() == ch)) return Token.Types.OperatorSymbol;
                    if (ValidNumericExpressionChar(ch)) return Token.Types.Numeric;
                    return Token.Types.Invalid;
                case Token.Types.GroupBegins:
                case Token.Types.GroupEnds:
                    return Token.Types.None;
                case Token.Types.Id:
                    if (ValidIdExpressionChar(ch)) return context;
                    break;
                case Token.Types.Numeric:
                    if (ValidNumericExpressionChar(ch)) return context;
                    if (ValidIdExpressionChar(ch)) return Token.Types.Invalid;
                    break;
                case Token.Types.PrefixSymbol:
                    if (index - startIndex > 0 && ValidPrefixSymbol(text.Substring(startIndex, index - startIndex + 1)))
                    {
                        return Token.Types.PrefixSymbol;
                    }
                    break;
                case Token.Types.UnionSymbol:
                    if (index - startIndex > 0 && ValidUnionSymbol(text.Substring(startIndex, index - startIndex + 1)))
                    {
                        return Token.Types.UnionSymbol;
                    }
                    break;
                case Token.Types.SufixSymbol:
                    if (index - startIndex > 0 && ValidSufixSymbol(text.Substring(startIndex, index - startIndex + 1)))
                    {
                        return Token.Types.SufixSymbol;
                    }
                    break;
                case Token.Types.OperatorSymbol:
                    if (index - startIndex > 0 && ValidOperatorSymbol(text.Substring(startIndex, index - startIndex + 1)))
                    {
                        return Token.Types.OperatorSymbol;
                    }
                    break;
                case Token.Types.Invalid:
                    if (ValidDisposableChar(ch) || PrefixSymbols.Any(str => str.First() == ch) || SufixSymbols.Any(str => str.First() == ch) || OperatorSymbols.Any(str => str.First() == ch)) return Token.Types.None;
                    return context;
            }
            return Token.Types.None;
        }

        public static Token GetNextToken(string text, ref int index)
        {
            if (text == null || text.Length == 0 || index >= text.Length) return Token.Empty;
            Token.Types context = Token.Types.None, newContext;
            do
            {
                context = GetCharToken(context, ref text, index, index);
                index++;
            } while (index < text.Length && context == Token.Types.None);
            int startIndex = index - 1;
            while (index < text.Length)
            {
                newContext = GetCharToken(context, ref text, startIndex, index);
                if (newContext != Token.Types.Invalid && context != newContext) break;
                context = newContext;
                index++;
            }
            return new Token(context, text.Substring(startIndex, index - startIndex), startIndex);
        }

        public static string GetOutputId(string value)
        {
            StringBuilder str = new StringBuilder(value);
            if (str.Length == 0) throw new Exception("Invalid string value.");
            if (OutputPrefixSymbols.Contains(value.First().ToString()))
            {
                str.Remove(0, 1);
            }
            else if (OutputSufixSymbols.Contains(value.Last().ToString()))
            {
                str.Remove(str.Length - 1, 1);
            }
            return str.ToString();
        }

        public static OperationType GetOutputOperation(string value)
        {
            if (value.First() == '!')
            {
                return OperationType.Clear;
            }
            else if (value.First() == '~')
            {
                return OperationType.Toggle;
            }
            else if (value.First() == '»')
            {
                return OperationType.Send;
            }
            else if (value.EndsWith(".max"))
            {
                return OperationType.Maximum;
            }
            else if (value.EndsWith(".min"))
            {
                return OperationType.Minimum;
            }
            else if (value.Last() == '+')
            {
                return OperationType.Increment;
            }
            else if (value.Last() == '-')
            {
                return OperationType.Decrement;
            }
            else
            {
                return OperationType.Set;
            }
        }
    }
}
