using System.Collections.Immutable;
using System.Text;

namespace RV.Chess.PGN
{
    internal class Lexer
    {
        private readonly string _text;
        private int _position;

        private int _tokenStart;
        private bool _insideComment = false;
        private object? _tokenValue;
        private TokenKind _tokenKind;

        internal Lexer(string text)
        {
            _text = text;
        }

        internal ImmutableArray<Token> ParseTokens()
        {
            var tokens = new List<Token>();

            while (true)
            {
                var token = ReadToken();

                if (token.Kind == TokenKind.Invalid)
                {
                    throw new InvalidPgnException("Invalid token", _tokenStart, _text);
                }

                tokens.Add(token);

                if (token.Kind == TokenKind.EndOfFile)
                {
                    break;
                }
            }

            return tokens.ToImmutableArray();
        }

        private Token ReadToken()
        {
            _tokenStart = _position;
            _tokenKind = TokenKind.Invalid;
            _tokenValue = null;

            if (_insideComment)
            {
                var closeBracketIdx = _text.IndexOf('}', _position);

                if (closeBracketIdx > 0)
                {
                    _tokenKind = TokenKind.Symbol;
                    var _tokenText = _text[_position..closeBracketIdx];
                    _insideComment = false;
                    _position = closeBracketIdx;
                    return new Token(_tokenText, _tokenKind, _tokenStart, _position, _tokenText);
                }
            }

            switch (Current)
            {
                case '\0':
                    _tokenKind = TokenKind.EndOfFile;
                    break;
                case '\r':
                    _tokenKind = TokenKind.NewLine;
                    if (Lookahead == '\n')
                    {
                        _position += 2;
                    }
                    else
                    {
                        _position++;
                    }
                    break;
                case '\n':
                    _tokenKind = TokenKind.NewLine;
                    _position++;
                    break;
                case ' ':
                case '\t':
                    _tokenKind = TokenKind.Whitespace;
                    _position++;
                    break;
                case '[':
                    _tokenKind = TokenKind.SquareBracketOpen;
                    _position++;
                    break;
                case ']':
                    _tokenKind = TokenKind.SquareBracketClose;
                    _position++;
                    break;
                case '(':
                    _tokenKind = TokenKind.ParenthesisOpen;
                    _position++;
                    break;
                case ')':
                    _tokenKind = TokenKind.ParenthesisClose;
                    _position++;
                    break;
                case '<':
                    _tokenKind = TokenKind.AngleBracketOpen;
                    _position++;
                    break;
                case '>':
                    _tokenKind = TokenKind.AngleBracketClose;
                    _position++;
                    break;
                case '{':
                    _tokenKind = TokenKind.CurlyBracketOpen;
                    _position++;
                    _insideComment = true;
                    break;
                case '}':
                    _tokenKind = TokenKind.CurlyBracketClose;
                    _position++;
                    break;
                case '.':
                    _tokenKind = TokenKind.Period;
                    _position++;
                    break;
                case '*':
                    _tokenKind = TokenKind.Asterisk;
                    _position++;
                    break;
                case ';':
                    _tokenKind = TokenKind.Semicolon;
                    _position++;
                    break;
                case '$':
                    ReadNAG();
                    break;
                case '"':
                    ReadString();
                    break;
                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                    if ((Current == '0' || Current == '1')
                        && Lookahead == '-'
                        || Lookahead == '/')
                    {
                        ReadGameTerminator();
                    }
                    else
                    {
                        ReadNumber();
                    }
                    break;
                case '!':
                case '?':
                    _tokenKind = TokenKind.Annotation;

                    if (Lookahead == '!' || Lookahead == '?')
                    {
                        _tokenValue = $"{Current}{Lookahead}";
                        _position += 2;
                    }
                    else
                    {
                        _tokenValue = Current.ToString();
                        _position++;
                    }
                    break;
                default:
                    if (char.IsLetterOrDigit(Current)
                        || Current == '_'
                        || Current == '+'
                        || Current == '#'
                        || Current == '='
                        || Current == ':'
                        || Current == '-')
                    {
                        ReadSymbol();
                    }
                    break;
            }

            var tokenText = _tokenKind == TokenKind.Invalid ? string.Empty : _text[_tokenStart.._position];

            return new Token(tokenText, _tokenKind, _tokenStart, _position, _tokenValue);
        }

        private void ReadGameTerminator()
        {
            if (Lookahead == '-' && _text.Length >= _position + 3)
            {
                var result = _text.Substring(_position, 3);

                if (result == "1-0" || result == "0-1")
                {
                    _tokenKind = TokenKind.GameTerminator;
                    _tokenValue = result;
                    _position += 3;
                }
            }
            else if (Lookahead == '/' && _text.Length >= _position + 7)
            {
                var result = _text.Substring(_position, 7);

                if (result == "1/2-1/2")
                {
                    _tokenKind = TokenKind.GameTerminator;
                    _tokenValue = result;
                    _position += 7;
                }
            }
            else
            {
                _position++;
            }
        }

        private void ReadSymbol()
        {
            var sb = new StringBuilder();

            while (true)
            {
                if (char.IsLetterOrDigit(Current)
                    || Current == '_'
                    || Current == '+'
                    || Current == '#'
                    || Current == '='
                    || Current == ':'
                    || Current == '-')
                {
                    sb.Append(Current);
                    _position++;
                }
                else
                {
                    break;
                }
            }

            _tokenKind = TokenKind.Symbol;
            _tokenValue = sb.ToString();
        }

        private void ReadNAG()
        {
            _position++;
            var sb = new StringBuilder("$");

            while (char.IsDigit(Current))
            {
                sb.Append(Current);
                _position++;
            }

            _tokenKind = sb.Length > 1 ? TokenKind.NumericAnnotationGlyph : TokenKind.Invalid;
            _tokenValue = sb.ToString();
        }

        private void ReadString()
        {
            _position++;

            var sb = new StringBuilder();
            var done = false;
            var invalid = false;

            while (!done)
            {
                switch (Current)
                {
                    case '\0':
                    case '\r':
                    case '\n':
                        done = true;
                        invalid = true;
                        break;
                    case '\\':
                        if (Lookahead == '\\')
                        {
                            _position += 2;
                            sb.Append('\\');
                        }
                        else if (Lookahead == '\"')
                        {
                            _position += 2;
                            sb.Append('\"');
                        }
                        else
                        {
                            invalid = true;
                            done = true;
                            _position++;
                        }
                        break;
                    case '\"':
                        _position++;
                        done = true;
                        break;
                    default:
                        sb.Append(Current);
                        _position++;
                        break;
                }
            }

            _tokenKind = invalid ? TokenKind.Invalid : TokenKind.String;
            _tokenValue = sb.ToString();
        }

        private void ReadNumber()
        {
            while (char.IsDigit(Current))
            {
                _position++;
            }

            var text = _text[_tokenStart.._position];

            if (!int.TryParse(text, out var value))
            {
                throw new InvalidPgnException("Invalid number token", _tokenStart, _text);
            }

            _tokenKind = TokenKind.Integer;
            _tokenValue = value;
        }

        private char Current => Peek(0);

        private char Lookahead => Peek(1);

        private char Peek(int offset)
        {
            var idx = _position + offset;

            if (idx >= _text.Length)
            {
                return '\0';
            }

            return _text[idx];
        }
    }
}
