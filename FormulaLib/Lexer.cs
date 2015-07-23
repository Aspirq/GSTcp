using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FormulaLib
{
    enum LexemType
    {
        Plus,
        Minus,
        Multiply,
        Divide,
        UnarMinus,

        Equals,
        NotEquals,
        Greater,
        Lower,
        GreaterEquals,
        LowerEquals,

        And,
        Or,

        LBracket,
        RBracket,

        Semicolon,
        Identifier,
        Number
    }

    class LocationEntity
    {
        public int Offset;
        public int Length;
    }

    [DebuggerDisplay("{Type} - {Value}")]
    class Lexem : LocationEntity
    {
        public LexemType Type;
        public string Value;
    }

    class LexemDefinition<T>
    {
        public LexemType Kind { get; protected set; }
        public T Representation { get; protected set; }
    }

    class StaticLexemDefinition : LexemDefinition<string>
    {
        public bool IsKeyword;

        public StaticLexemDefinition(string rep, LexemType kind, bool isKeyword = false)
        {
            Representation = rep;
            Kind = kind;
            IsKeyword = isKeyword;
        }
    }

    class DynamicLexemDefinition : LexemDefinition<Regex>
    {
        public DynamicLexemDefinition(string rep, LexemType kind)
        {
            Representation = new Regex(@"\G" + rep, RegexOptions.Compiled);
            Kind = kind;
        }
    }

    static class LexemDefinitions
    {
        public static StaticLexemDefinition[] Statics = new[]
        {
            new StaticLexemDefinition("+", LexemType.Plus),
            new StaticLexemDefinition("-", LexemType.Minus),
            new StaticLexemDefinition("*", LexemType.Multiply),
            new StaticLexemDefinition("/", LexemType.Divide),
            new StaticLexemDefinition("%", LexemType.UnarMinus),
 
            new StaticLexemDefinition("==", LexemType.Equals),
            new StaticLexemDefinition("!=", LexemType.NotEquals),
            new StaticLexemDefinition(">=", LexemType.GreaterEquals),
            new StaticLexemDefinition("<=", LexemType.LowerEquals),
            new StaticLexemDefinition(">", LexemType.Greater),
            new StaticLexemDefinition("<", LexemType.Lower),
 
            new StaticLexemDefinition("&&", LexemType.And),
            new StaticLexemDefinition("||", LexemType.Or),
 
            new StaticLexemDefinition("(", LexemType.LBracket),
            new StaticLexemDefinition(")", LexemType.RBracket),
            new StaticLexemDefinition(";", LexemType.Semicolon),
        };

        public static DynamicLexemDefinition[] Dynamics = new[]
        {
            new DynamicLexemDefinition("[a-zA-Z_][a-zA-Z0-9_]*", LexemType.Identifier),
            new DynamicLexemDefinition(@"([0-9]*\.?[0-9]+)", LexemType.Number),
        };
    }

    class Lexer
    {
        private char[] SpaceChars = new[] { ' ', '\n', '\r', '\t' };
        private Dictionary<String, Double> Variables;
        private string Source;
        private int Offset;

        public IEnumerable<Lexem> Lexems { get; private set; }

        public Lexer(string src, Dictionary<String, Double> vars)
        {
            Variables = vars;
            Source = ReplaceUnarOperator(src);
            Parse();
            ReplaceVariables();
            ReplaceSeparator(NumberFormatInfo.CurrentInfo.NumberDecimalSeparator);
        }

        private void ReplaceSeparator(string sep)
        {
            foreach (Lexem lexem in Lexems.Where(res => res.Type == LexemType.Number))
            {
                lexem.Value = lexem.Value.Replace(".", sep).Replace(",", sep);
            }
        }

        private string ReplaceUnarOperator(String src)
        {
            return Regex.Replace(Regex.Replace(Regex.Replace(Regex.Replace(src, @"(\(\+)", "("), @"(\A[\+]{1})", ""), @"(\(-)", "(%"), @"(\A[-]{1})", "%");
        }

        private void ReplaceVariables()
        {
            foreach (Lexem lexem in Lexems.Where(res=>res.Type == LexemType.Identifier))
            {

                if (Variables.ContainsKey(lexem.Value))
                {
                    lexem.Type = LexemType.Number;
                    lexem.Value = Variables[lexem.Value].ToString();
                }
            }
        }

        private void Parse()
        {
            var lexems = new List<Lexem>();

            while (InBounds())
            {
                SkipSpaces();
                if (!InBounds()) break;

                var lex = ProcessStatic() ?? ProcessDynamic();
                if (lex == null) throw new Exception(string.Format("Unknown lexem at {0}", Offset));

                lexems.Add(lex);
            }

            Lexems = lexems;
        }

        private void SkipSpaces()
        {
            while (InBounds() && Source[Offset].IsAnyOf(SpaceChars))
                Offset++;
        }

        private Lexem ProcessStatic()
        {
            foreach (var def in LexemDefinitions.Statics)
            {
                var rep = def.Representation;
                var len = rep.Length;

                if (Offset + len > Source.Length || Source.Substring(Offset, len) != rep)
                    continue;

                if (Offset + len < Source.Length && def.IsKeyword)
                {
                    var nextChar = Source[Offset + len];
                    if (nextChar == '_' || char.IsLetterOrDigit(nextChar))
                        continue;
                }

                Offset += len;
                return new Lexem { Type = def.Kind, Offset = Offset, Length = len };
            }

            return null;
        }

        private Lexem ProcessDynamic()
        {
            foreach (var def in LexemDefinitions.Dynamics)
            {
                var match = def.Representation.Match(Source, Offset);
                if (!match.Success)
                    continue;

                Offset += match.Length;
                return new Lexem { Type = def.Kind, Offset = Offset, Length = match.Length, Value = match.Value };
            }

            return null;
        }

        private bool InBounds()
        {
            return Offset < Source.Length;
        }
    }
}
