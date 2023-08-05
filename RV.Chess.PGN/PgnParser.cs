using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using FluentResults;
using RV.Chess.PGN.Readers;
using RV.Chess.PGN.Utils;
using RV.Chess.Shared.Types;

namespace RV.Chess.PGN
{
    public class PgnParser : IDisposable
    {
        private bool _disposed = false;
        private long _gameChunkStart = 0;
        private int _cursor = 0;
        private IPgnReader _reader;

#pragma warning disable CS8618
        private PgnParser() { }
#pragma warning restore CS8618

        ~PgnParser() => Dispose(false);

        [MemberNotNullWhen(returnValue: false, nameof(_reader))]
        public bool IsError { get; private set; }

        public string ErrorMessage { get; private set; } = string.Empty;

        public static PgnParser FromFile(string path, bool useStrictReader = false)
        {
            try
            {
                IPgnReader reader = useStrictReader
                    ? StrictPgnFileReader.Open(path)
                    : SimplePgnFileReader.Open(path);

                return new PgnParser
                {
                    _reader = reader,
                };
            }
            catch (Exception ex)
            {
                return new PgnParser
                {
                    IsError = true,
                    ErrorMessage = ex.Message,
                };
            }
        }

        public static PgnParser FromString(string data)
        {
            try
            {
                return new PgnParser
                {
                    _reader = StringPgnReader.Open(data),
                };
            }
            catch (Exception ex)
            {
                return new PgnParser
                {
                    IsError = true,
                    ErrorMessage = ex.Message,
                };
            }
        }

        public IEnumerable<Result<PgnGame>> GetGames(bool useLaxTagParsing = false)
        {
            if (IsError)
            {
                yield break;
            }

            while (_reader.TryGetGameChunk(out var game))
            {
                _gameChunkStart = game.ChunkStartPos;
                _cursor = 0;

                Result<PgnGame> result;

                try
                {
                    result = Result.Ok(ParseGame(game.Text, useLaxTagParsing));
                }
                catch (Exception ex)
                {
                    var gameText = game.Text[..Math.Min(2048, game.Text.Length)];
                    result = Result.Fail(new Error(ex.Message).CausedBy(gameText.ToString()));
                }

                yield return result;
            }

            _reader.Reset();
            yield break;
        }

        private PgnGame ParseGame(ReadOnlySpan<char> text, bool useLaxTagParsing)
        {
            var moves = new Stack<List<PgnNode>>();
            moves.Push(new List<PgnNode>());
            var tags = new Dictionary<string, string>();
            var moveNo = 1;
            var side = Side.White;

            while (_cursor < text.Length)
            {
                switch (text[_cursor])
                {
                    case '[':
                        var (name, value) = useLaxTagParsing ? ReadTagPairLax(text) : ReadTagPairStrict(text);
                        tags.TryAdd(name, value);
                        if (name == "FEN")
                        {
                            side = value.Split(' ')[1] == "b" ? Side.Black : Side.White;
                        }
                        break;
                    case ' ':
                        SkipWhitespace(text);
                        break;
                    case '{':
                        _cursor++;
                        var comment = ReadStringToken(text, '}');
                        moves.Peek().Add(new PgnCommentNode(comment.Trim()));
                        break;
                    case '*':
                        moves.Peek().Add(new PgnTerminatorNode(GameResult.Unknown));
                        return new PgnGame(tags, moves.Pop());
                    case '0':
                        // no need to check if it is a move number, since it can't start with zero
                        if (_cursor == text.Length - 1)
                        {
                            throw new InvalidDataException($"Bad game terminator at {_gameChunkStart + _cursor}");
                        }
                        else
                        {
                            var node = ReadCastlingOrResult(text, moveNo, side);
                            moves.Peek().Add(node);

                            if (node is PgnTerminatorNode)
                            {
                                return new PgnGame(tags, moves.Pop());
                            }

                            side = side.Opposite();
                        }

                        break;
                    case '1':
                        {
                            var next = text.PeekAt(_cursor + 1);

                            if (char.IsDigit(next) || char.IsWhiteSpace(next) || next == '.')
                            {
                                moveNo = ReadMoveNumber(text);
                                SkipDots(text);
                                break;
                            }
                            else if (next == '-' || next == '/')
                            {
                                moves.Peek().Add(ReadWhiteOrDrawResult(text));
                                return new PgnGame(tags, moves.Pop());
                            }
                        }

                        throw new InvalidDataException($"Malformed move number at {_gameChunkStart + _cursor}");
                    case '2': case '3': case '4': case '5': case '6': case '7': case '8': case '9':
                        moveNo = ReadMoveNumber(text);
                        SkipDots(text);
                        break;
                    case 'O':
                        moves.Peek().Add(ReadCastlingOrResult(text, moveNo, side));
                        side = side.Opposite();
                        break;
                    case '(':
                        // variation starts with the same color as the previous move
                        side = (moves.Peek().FindLast(m => m is PgnMoveNode mn) as PgnMoveNode)?.Side ?? side;
                        moves.Push(new List<PgnNode>());
                        _cursor++;
                        break;
                    case ')':
                        var variation = moves.Pop();
                        side = (moves.Peek().FindLast(m => m is PgnMoveNode mn) as PgnMoveNode)?.Side.Opposite() ?? side;
                        moves.Peek().Add(new PgnVariationNode(variation));
                        _cursor++;
                        break;
                    case '\n':
                    case '\r':
                        _cursor++;
                        break;
                    case '.':
                        SkipDots(text);
                        break;
                    case ';':
                        var nextNewLine = text[_cursor..].IndexOf('\n');

                        if (nextNewLine > -1)
                        {
                            moves.Peek().Add(
                                new PgnCommentNode(text.Slice(_cursor + 1, nextNewLine - 1).ToString().Trim()));
                            _cursor += nextNewLine + 1;
                            break;
                        }

                        throw new InvalidDataException($"Malformed single-line comment ad {_gameChunkStart + _cursor}");
                    case '$':
                        moves.Peek().Add(ReadNagGlyph(text));
                        break;
                    default:
                        if (IsValidSanStartingCharacter(text[_cursor]))
                        {
                            moves.Peek().Add(ReadSanAndSuffix(text, moveNo, side));
                            side = side.Opposite();
                            break;
                        }
                        else if (text[_cursor] == 'Z' && text.PeekAt(_cursor + 1) == '0')
                        {
                            // chessbase-style null move
                            moves.Peek().Add(new PgnMoveNode(moveNo, side, "Z0", string.Empty, true));
                            _cursor += 2;
                            break;
                        }
                        else
                        {
                            throw new InvalidDataException($"Unexpected character at {_gameChunkStart + _cursor}");
                        }
                }
            }

            throw new InvalidDataException($"Unterminated game at {_gameChunkStart}");
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            try
            {
                if (disposing)
                {
                    _reader?.Dispose();
                }
            }
            finally
            {
                _disposed = true;
            }
        }

        private PgnAnnotationGlyphNode ReadNagGlyph(ReadOnlySpan<char> text)
        {
            _cursor += 1;
            var start = _cursor;

            while (char.IsDigit(text.PeekAt(_cursor)))
            {
                _cursor++;
            }

            if (_cursor > start)
            {
                return new PgnAnnotationGlyphNode($"{text[start.._cursor]}");
            }

            throw new InvalidDataException($"Malformed NAG node at {_gameChunkStart + _cursor}");
        }

        private int ReadMoveNumber(ReadOnlySpan<char> text)
        {
            var start = _cursor;

            while (char.IsDigit(text[_cursor]))
            {
                _cursor++;
            }

            if (int.TryParse(text[start.._cursor], out var moveNo))
            {
                return moveNo;
            }

            throw new InvalidDataException($"Malformed number at {_gameChunkStart + start}");
        }

        private PgnMoveNode ReadSanAndSuffix(ReadOnlySpan<char> text, int moveNo, Side side)
        {
            var start = _cursor;

            while (start < text.Length && IsValidSanCharacter(text[_cursor]))
            {
                _cursor++;
            }

            if (_cursor - start < 2)
            {
                throw new InvalidDataException($"Malformed SAN notation at {_gameChunkStart + _cursor}");
            }

            var san = text[start.._cursor].ToString();
            start = _cursor;

            while (start < text.Length && (text[_cursor] == '!' || text[_cursor] == '?'))
            {
                _cursor++;
            }

            if (_cursor - start > 0)
            {
                return new PgnMoveNode(moveNo, side, san, text[start.._cursor].ToString());
            }

            return new PgnMoveNode(moveNo, side, san);
        }

        private PgnNode ReadCastlingOrResult(ReadOnlySpan<char> text, int moveNo, Side side)
        {
            // at that point first symbol is either '0' or 'O'
            var start = _cursor;

            if (text.PeekAt(start + 1) == '-') // '0-'
            {
                if (text.PeekAt(start + 2) == '1') // '0-1'
                {
                    _cursor += 3;
                    return new PgnTerminatorNode(GameResult.Black);
                }
                else if (text.PeekAt(start + 2) == text[start]) // '0-0' or 'O-O'
                {
                    if (text.PeekAt(start + 3) == '-' && text.PeekAt(start + 4) == text[start]) // '0-0-0' or 'O-O-O'
                    {
                        _cursor += 5;
                        if (text.PeekAt(_cursor) == '+' || text.PeekAt(_cursor) == '#')
                        {
                            _cursor++;
                            return new PgnMoveNode(moveNo, side, $"O-O-O{text[_cursor - 1]}");
                        }

                        return new PgnMoveNode(moveNo, side, $"O-O-O");
                    }

                    _cursor += 3;

                    if (text.PeekAt(_cursor) == '+' || text.PeekAt(_cursor) == '#')
                    {
                        _cursor++;
                        return new PgnMoveNode(moveNo, side, $"O-O{text[_cursor - 1]}");
                    }

                    return new PgnMoveNode(moveNo, side, "O-O");
                }
            }

            throw new InvalidDataException($"Malformed result or castling tag at {_gameChunkStart + start}");
        }

        private PgnNode ReadWhiteOrDrawResult(ReadOnlySpan<char> text)
        {
            // at that point first symbol is always '1'
            if (text.PeekAt(_cursor + 1) == '-') // '1-'
            {
                if (text.PeekAt(_cursor + 2) == '0')
                {
                    _cursor += 3;
                    return new PgnTerminatorNode(GameResult.White);
                }
            }
            else // '1/', here we know it is a slash, because we were in the '-' or '/' branch in the caller method
            {
                _cursor += 2;
                Expect(text, '2');
                Expect(text, '-');
                Expect(text, '1');
                Expect(text, '/');
                Expect(text, '2');
                return new PgnTerminatorNode(GameResult.Tie);
            }

            throw new InvalidDataException($"Malformed result tag at {_gameChunkStart + _cursor}");
        }

        private KeyValuePair<string, string> ReadTagPairStrict(ReadOnlySpan<char> text)
        {
            // _cursor starts at the opening '['
            _cursor++;
            var tagName = ReadSymbolToken(text);
            SkipWhitespace(text);
            Expect(text, '"');
            var tagString = ReadStringToken(text, '"');
            SkipWhitespace(text);
            Expect(text, ']');

            return new KeyValuePair<string, string>(tagName, tagString);
        }

        private KeyValuePair<string, string> ReadTagPairLax(ReadOnlySpan<char> text)
        {
            // _cursor starts at the opening '['
            _cursor++;
            var tagName = ReadSymbolToken(text);
            SkipWhitespace(text);
            Expect(text, '"');

            var lineEnd = text.Slice(_cursor, Math.Min(192, text.Length - _cursor)).IndexOfAny('\r', '\n');

            if (lineEnd == -1)
            {
                throw new InvalidDataException($"Malformed tag at {_gameChunkStart + _cursor}");
            }

            var closingBracketPos = text.Slice(_cursor, lineEnd).LastIndexOf("\"]", StringComparison.Ordinal);

            if (closingBracketPos == -1)
            {
                throw new InvalidDataException($"Malformed tag at {_gameChunkStart + _cursor}");
            }

            var tagString = text.Slice(_cursor, closingBracketPos).ToString();
            _cursor += lineEnd + 1;

            return new KeyValuePair<string, string>(tagName, tagString);
        }

        private string ReadStringToken(ReadOnlySpan<char> text, char endSymbol)
        {
            var symbolEscaped = false;
            var start = _cursor;
            var unescape = false;

            while (_cursor < text.Length)
            {
                if (text[_cursor] == '\\')
                {
                    if (symbolEscaped)
                    {
                        symbolEscaped = false;
                    }
                    else
                    {
                        symbolEscaped = true;
                        unescape = true;
                    }
                }
                else if (text[_cursor] == endSymbol)
                {
                    if (symbolEscaped)
                    {
                        symbolEscaped = false;
                    }
                    else
                    {
                        _cursor++;

                        return unescape
                            ? Regex.Unescape(text[start..(_cursor - 1)].ToString())
                            : text[start..(_cursor - 1)].ToString();
                    }
                }

                _cursor++;
            }

            throw new InvalidDataException($"Malformed string token at {_gameChunkStart + start}");
        }

        private string ReadSymbolToken(ReadOnlySpan<char> text)
        {
            var start = _cursor;

            if (!char.IsLetterOrDigit(text[start]))
            {
                throw new InvalidDataException($"Malformed symbol token at {_gameChunkStart + start}");
            }

            var nextDelimeter = text[start..].IndexOf(' ');

            if (nextDelimeter < 0)
            {
                throw new InvalidDataException($"Malformed symbol token at {_gameChunkStart + start}");
            }

            _cursor += nextDelimeter + 1;

            return text.Slice(start, nextDelimeter).ToString();
        }

        private void SkipWhitespace(ReadOnlySpan<char> text)
        {
            while (_cursor < text.Length && char.IsWhiteSpace(text[_cursor]))
            {
                _cursor++;
            }
        }

        private void SkipDots(ReadOnlySpan<char> text)
        {
            while (_cursor < text.Length && text[_cursor] == '.')
            {
                _cursor++;
            }
        }

        private void Expect(ReadOnlySpan<char> text, char expected)
        {
            if (text[_cursor++] != expected)
            {
                throw new InvalidDataException($"Unexpected symbol at {_gameChunkStart + _cursor}");
            }
        }

        private static bool IsValidSanStartingCharacter(char c)
        {
            var l = char.ToLower(c);
            return (c >= 'a' && c <= 'h') || l == 'r' || l == 'n' || l == 'b' || l == 'q' || l == 'k';
        }

        private static bool IsValidSanCharacter(char c)
        {
            return IsValidSanStartingCharacter(c) || char.IsDigit(c) || c == 'x' || c == '+' || c == '#' || c == '=';
        }
    }
}
