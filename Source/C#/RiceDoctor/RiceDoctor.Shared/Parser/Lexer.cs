using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using static RiceDoctor.Shared.TokenType;

namespace RiceDoctor.Shared
{
    public abstract class Lexer
    {
        public const char None = '\0';

        [NotNull] private readonly string _text;

        [NotNull] private readonly IList<Token> _tokens;

        private int _pos;

        [CanBeNull] private ReadOnlyCollection<Token> _readOnlyTokens;

        protected char CurrentChar;

        protected Lexer([NotNull] string text)
        {
            Check.NotEmpty(text, nameof(text));

            _text = text.Trim();
            _pos = 0;
            CurrentChar = _text[_pos];
            _tokens = new List<Token>();
        }

        [NotNull]
        public IReadOnlyList<Token> Tokenize()
        {
            if (_readOnlyTokens != null)
                return _readOnlyTokens;

            var token = GetNextToken();
            while (token.Type != Eof)
            {
                _tokens.Add(token);
                token = GetNextToken();
            }
            _tokens.Add(token);

            _readOnlyTokens = new ReadOnlyCollection<Token>(_tokens);

            return _readOnlyTokens;
        }

        [CanBeNull]
        protected Token? LastToken()
        {
            if (_tokens.Count > 0)
                return _tokens.Last();

            return null;
        }

        protected void Advance(int length = 1)
        {
            _pos = _pos + length;
            CurrentChar = _pos > _text.Length - 1 ? None : _text[_pos];
        }

        [CanBeNull]
        protected char? Peek(int length = 1)
        {
            var peekPos = _pos + length;

            if (peekPos >= _text.Length - 1) return null;
            return _text[peekPos];
        }

        protected void SkipWhitespace()
        {
            while (char.IsWhiteSpace(CurrentChar))
                Advance();
        }

        protected Token GetIdentifier()
        {
            var builder = new StringBuilder();

            while (char.IsLetterOrDigit(CurrentChar) || CurrentChar == '_')
            {
                builder.Append(CurrentChar);
                Advance();
            }

            return new Token(Ident, builder.ToString());
        }

        protected Token GetNewLine()
        {
            string newLine;
            if (CurrentChar == '\r' && Peek() == '\n')
            {
                newLine = "\r\n";
                Advance(2);
            }
            else
            {
                newLine = CurrentChar.ToString();
                Advance();
            }

            return new Token(NewLine, newLine);
        }

        protected Token GetNumber()
        {
            var numberPos = _pos;
            var builder = new StringBuilder();

            if (CurrentChar == '-')
            {
                builder.Append(CurrentChar);
                Advance();
            }

            if (CurrentChar == '0')
            {
                builder.Append(CurrentChar);
                Advance();
            }
            else if ("123456789".IndexOf(CurrentChar) != -1)
            {
                builder.Append(CurrentChar);
                Advance();
                while ("0123456789".IndexOf(CurrentChar) != -1)
                {
                    builder.Append(CurrentChar);
                    Advance();
                }
            }
            else
            {
                throw new InvalidOperationException($": Position {_pos}: '{CurrentChar}' is not a digit.");
            }

            if (CurrentChar == '.')
            {
                builder.Append(CurrentChar);
                Advance();

                if ("0123456789".IndexOf(CurrentChar) == -1)
                    throw new InvalidOperationException($": Position {_pos}: '{CurrentChar}' is not a digit.");
                while ("0123456789".IndexOf(CurrentChar) != -1)
                {
                    builder.Append(CurrentChar);
                    Advance();
                }
            }

            if (CurrentChar == 'e' || CurrentChar == 'E')
            {
                builder.Append(CurrentChar);
                Advance();
                if (CurrentChar == '+' || CurrentChar == '-')
                {
                    builder.Append(CurrentChar);
                    Advance();
                }

                if ("0123456789".IndexOf(CurrentChar) == -1)
                    throw new InvalidOperationException($": Position {_pos}: '{CurrentChar}' is not a digit.");
                while ("0123456789".IndexOf(CurrentChar) != -1)
                {
                    builder.Append(CurrentChar);
                    Advance();
                }
            }

            if (double.TryParse(builder.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out double number))
                return new Token(Number, number);
            throw new InvalidOperationException($": Position {numberPos}: \"{builder}\" is not a number.");
        }


        protected Token GetUnquotedString()
        {
            Advance();

            var stringPos = _pos;
            var builder = new StringBuilder();
            while (CurrentChar != None)
            {
                if (CurrentChar == '"')
                {
                    Advance();
                    return new Token(UnquotedString, builder.ToString());
                }
                if (CurrentChar == '\\')
                {
                    Advance();

                    switch (CurrentChar)
                    {
                        case '"':
                            builder.Append('"');
                            break;
                        case '\\':
                            builder.Append('\\');
                            break;
                        case '/':
                            builder.Append('/');
                            break;
                        case 'b':
                            builder.Append('\b');
                            break;
                        case 'f':
                            builder.Append('\f');
                            break;
                        case 'n':
                            builder.Append('\n');
                            break;
                        case 'r':
                            builder.Append('\r');
                            break;
                        case 't':
                            builder.Append('\t');
                            break;
                        default:
                            if (CurrentChar == 'u' && Peek(4) != null)
                            {
                                var hexPos = _pos;
                                var hexBuilder = new StringBuilder();
                                for (var i = 0; i < 4; ++i)
                                {
                                    Advance();
                                    if ("0123456789ABCDEFabcdef".IndexOf(CurrentChar) != -1)
                                        hexBuilder.Append(CurrentChar);
                                    else
                                        throw new InvalidOperationException(
                                            $": Position {_pos}: '{CurrentChar}' is not a hexadecimal digit.");
                                }

                                if (int.TryParse(hexBuilder.ToString(), NumberStyles.HexNumber,
                                    CultureInfo.InvariantCulture,
                                    out int codePoint))
                                    builder.Append((char) codePoint);
                                else
                                    throw new InvalidOperationException(
                                        $": Position {hexPos}: \"{hexBuilder}\" is not an unicode character.");
                            }
                            else
                            {
                                throw new InvalidOperationException(
                                    $": Position {_pos}: '{CurrentChar}' is not a control character.");
                            }
                            break;
                    }
                }
                else
                {
                    builder.Append(CurrentChar);
                }

                Advance();
            }

            throw new InvalidOperationException($": Position {stringPos}: \"{builder}\" is not a string.");
        }

        protected abstract Token GetNextToken();
    }
}