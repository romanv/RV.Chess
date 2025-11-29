using System.Buffers;
using System.Diagnostics;
using System.Text;
using RV.Chess.Shared.Types;

namespace RV.Chess.PGN;

public class PgnParser : IDisposable
{
    private static readonly SearchValues<char> s_tagKeyTerminators = SearchValues.Create(" \"");
    private static readonly SearchValues<char> s_tagValueTerminators = SearchValues.Create("\\\"");
    private static readonly SearchValues<char> s_newLineSymbols = SearchValues.Create("\r\n");

    private const int DefaultBufferSize = 2 * 1024 * 1024;
    private const int MaxTokenLength = 255;

    private readonly BufferedReader _reader;
    private readonly PgnParserState _state = new();
    private readonly char[] _buffer = new char[MaxTokenLength];

    private bool _requiresRewind;
    private bool _disposedValue;

    private PgnParser(BufferedReader reader)
    {
        _reader = reader;
    }

    public long Length => _reader.TotalLength;

    public long Processed => _reader.Processed;

    public static PgnParser FromFile(string path, int bufferSize = DefaultBufferSize)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException();

        var fileStream = File.OpenRead(path);
        var reader = new BufferedReader(fileStream, bufferSize);

        return new PgnParser(reader);
    }

    public static PgnParser FromString(string data, int bufferSize = DefaultBufferSize)
    {
        if (string.IsNullOrEmpty(data))
            throw new InvalidDataException("String is empty");

        var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(data));
        var reader = new BufferedReader(memoryStream, bufferSize);

        return new PgnParser(reader);
    }

    public static PgnParser FromStream(FileStream fileStream, int bufferSize = DefaultBufferSize)
    {
        if (!fileStream.CanRead)
            throw new InvalidOperationException("Can't read from file stream");

        if (fileStream.CanSeek)
            fileStream.Seek(0, SeekOrigin.Begin);

        var reader = new BufferedReader(fileStream, bufferSize, false);

        return new PgnParser(reader);
    }

    public IEnumerable<PgnGame> GetGames()
    {
        if (_requiresRewind)
        {
            _reader.Reset();
        }

        PgnGame? result = default;
        var stopParsing = false;
        _requiresRewind = true;

        while (true)
        {
Start:
            try
            {
                switch (_reader.Current)
                {
                    case ' ':
                    case '\r':
                    case '\n':
                    case '\t':
                        _reader.Advance();
                        break;
                    case '[':
                        var (key, value) = ReadTagPair();

                        if (key == "FEN")
                        {
                            var parts = value.Split(' ');
                            if (parts.Length < 2)
                                throw new PgnParsingException(PgnErrorType.MovetextError, "Malformed FEN tag");
                            _state.Side = parts[1] == "b" ? Side.Black : Side.White;
                        }

                        _state.AddTag(key, value);
                        break;
                    case '{':
                        var mlComment = ReadMultilineComment();
                        _state.AddNode(new PgnCommentNode(mlComment));
                        break;
                    case '0':
                        if (char.IsDigit(_reader.Next) || char.IsWhiteSpace(_reader.Next) || _reader.Next == '.')
                        {
                            _state.MoveNo = ReadNumber();
                            break;
                        }

                        var terminatorOrCastling = ReadTerminatorOrCastling();
                        _state.AddNode(terminatorOrCastling);

                        if (terminatorOrCastling is PgnTerminatorNode tn)
                        {
                            result = _state.GetGame();
                            goto ReturnResult;
                        }

                        _state.Side = _state.Side.Opposite();
                        break;
                    case '1':
                        if (char.IsDigit(_reader.Next) || char.IsWhiteSpace(_reader.Next) || _reader.Next == '.')
                        {
                            _state.MoveNo = ReadNumber();
                            break;
                        }

                        var terminator = ReadTerminator();
                        _state.AddNode(terminator);
                        result = _state.GetGame();
                        goto ReturnResult;
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                    case '9':
                        _state.MoveNo = ReadNumber();
                        break;
                    case '.':
                        while (_reader.Current == '.')
                            _reader.Advance();
                        break;
                    case '(':
                        _state.Side = _state.LastMove?.Side ?? _state.Side;
                        _state.StartVariation();
                        _reader.Advance();
                        break;
                    case ')':
                        var variation = _state.EndVariation();
                        _state.Side = _state.LastMove?.Side.Opposite() ?? _state.Side;
                        _state.AddNode(new PgnVariationNode(variation));
                        _reader.Advance();
                        break;
                    case ';':
                        var slComment = ReadSingleLineComment();
                        _state.AddNode(new PgnCommentNode(slComment));
                        break;
                    case '*':
                        _state.AddNode(new PgnTerminatorNode(GameResult.Unknown));
                        result = _state.GetGame();
                        _reader.Advance();
                        goto ReturnResult;
                    case '\0':
                        if (_state.Tags.Count > 0 || _state.Moves.Count > 0)
                        {
                            _state.AddError(PgnErrorType.UnrecoverableError, "Unterminated game");
                            result = _state.GetGame();
                        }

                        goto ReturnResult;
                    case '+':
                    case '±':
                    case '⩲':
                    case '⩱':
                    case '?':
                    case '!':
                        var suffix = ReadSanSuffix();
                        _state.AddSuffix(suffix);
                        _reader.Advance(suffix.Length);
                        break;
                    case '-':
                        if (_reader.Next == '-')
                        {
                            _reader.Advance(2);
                            _state.AddMove("--", true);
                        }
                        else
                        {
                            var suffixM = ReadSanSuffix();
                            _state.AddSuffix(suffixM);
                            _reader.Advance(suffixM.Length);
                        }

                        break;
                    case 'Z':
                        if (_reader.Next == '0')
                        {
                            _reader.Advance(2);
                            _state.AddMove("--", true);
                            break;
                        }

                        throw new PgnParsingException(PgnErrorType.MovetextError, "Unrecognized SAN");
                    case '$':
                        _reader.Advance();
                        var nag = ReadNumber();
                        _state.AddNode(new PgnAnnotationGlyphNode(nag.ToString()));
                        break;
                    default:
                        _state.AddMove(ReadSan().ToString(), false);
                        break;
                }
            }
            catch (PgnParsingException ex)
            {
                _state.AddError(ex.Type, ex.Message);
                var isRecovered = TryRecoverFromError();

                if (!isRecovered)
                {
                    _state.AddError(PgnErrorType.UnrecoverableError, "Can't recover from an error. Parsing stopped.");
                    stopParsing = true;
                    result = _state.GetGame();
                    goto ReturnResult;
                }

                // Can't continue parsing the current game, if the error occurred in the movetext.
                // In case of the recovered tag error, however, we can process with what we have.
                if (ex.Type == PgnErrorType.MovetextError)
                {
                    result = _state.GetGame();
                    goto ReturnResult;
                }
            }

            goto Start;

ReturnResult:
            if (result != null)
            {
                yield return result;

                if (stopParsing)
                {
                    yield break;
                }

                result = null;
                _state.ResetGame();
            }
            else
            {
                yield break;
            }
        }
    }

    private ReadOnlySpan<char> ReadSan()
    {
        var read = 0;

        while (char.IsAsciiLetterOrDigit(_reader.Peek(read))
            || _reader.Peek(read) == '='
            || (_reader.Peek(read) == '-' && _reader.Peek(read + 1) is '0' or 'O'))
        {
            read++;
        }

        if (_reader.Peek(read) == '+' || _reader.Peek(read) == '#')
            read++;

        var san = _reader.Data[..read];
        _reader.Advance(read);

        return san;
    }

    private ReadOnlySpan<char> ReadSanSuffix()
    {
        return (_reader.Current, _reader.Next) switch
        {
            ('-', '+') => "-+",
            ('+', '-') => "+-",
            ('?', '?') => "??",
            ('?', '!') => "?!",
            ('?', '±') => "?±",
            ('?', '⩲') => "?⩲",
            ('?', '⩱') => "?⩱",
            ('!', '?') => "!?",
            ('!', '!') => "!!",
            ('!', '±') => "!±",
            ('!', '⩲') => "!⩲",
            ('!', '⩱') => "!⩱",
            ('!', _) => "!",
            ('?', _) => "?",
            ('±', _) => "±",
            ('⩲', _) => "⩲",
            ('⩱', _) => "⩱",
            _ => throw new PgnParsingException(PgnErrorType.MovetextError, "Unrecognized SAN suffix"),
        };
    }

    private int ReadNumber()
    {
        var read = 0;

        while (char.IsDigit(_reader.Peek(read)))
            read++;

        if (!int.TryParse(_reader.Data[..read], out var moveNo))
            throw new PgnParsingException(PgnErrorType.MovetextError, "Invalid number token");

        _reader.Advance(read);

        if (
            _reader.Current != ' '
            && _reader.Current != '.'
            && _reader.Current != ')'
            && _reader.Current != '\r'
            && _reader.Current != '\n'
        )
        {
            throw new PgnParsingException(PgnErrorType.MovetextError, "Invalid character after the number token");
        }

        return moveNo;
    }

    private PgnTerminatorNode ReadTerminator()
    {
        _reader.Advance(1);

        if (_reader.StartsWith("-0"))
        {
            _reader.Advance(2);
            return new PgnTerminatorNode(GameResult.White);
        }

        if (_reader.StartsWith("/2-1/2"))
        {
            _reader.Advance(6);
            return new PgnTerminatorNode(GameResult.Tie);
        }

        throw new PgnParsingException(PgnErrorType.MovetextError, "Invalid game terminator");
    }

    private PgnNode ReadTerminatorOrCastling()
    {
        _reader.Advance(1);
        var castleLong = false;

        // '0-0'
        if (_reader.StartsWith("-0"))
        {
            _reader.Advance(2);
            PgnNode result;

            // '0-0-0'
            if (_reader.StartsWith("-0"))
            {
                castleLong = true;
                _reader.Advance(2);
            }

            // '0-0+', '0-0#', '0-0-0+' or ''0-0-0#'
            if (_reader.Current == '+' || _reader.Current == '#')
            {
                result = castleLong
                    ? new PgnMoveNode(_state.MoveNo, _state.Side, $"O-O-O{_reader.Current}")
                    : new PgnMoveNode(_state.MoveNo, _state.Side, $"O-O{_reader.Current}");
                _reader.Advance();
            }
            else
            {
                result = castleLong
                    ? new PgnMoveNode(_state.MoveNo, _state.Side, "O-O-O")
                    : new PgnMoveNode(_state.MoveNo, _state.Side, "O-O");
            }

            return result;
        }
        else if (_reader.StartsWith("-1"))
        {
            _reader.Advance(2);
            return new PgnTerminatorNode(GameResult.Black);
        }

        throw new PgnParsingException(PgnErrorType.MovetextError, "Invalid castling/game terminator");
    }

    private string ReadSingleLineComment()
    {
        Debug.Assert(_reader.Current == ';');
        _reader.Advance();
        var idx = _reader.IndexOfAny(s_newLineSymbols);

        if (idx == -1)
            return _reader.Data.ToString();

        var text = _reader.GetText(idx);
        _reader.Advance(idx + 1);

        return text;
    }

    private string ReadMultilineComment()
    {
        Debug.Assert(_reader.Current == '{');
        _reader.Advance();
        var idx = _reader.IndexOf('}');

        if (idx == -1)
            throw new PgnParsingException(PgnErrorType.MovetextError, "Unterminated multiline comment");

        var text = _reader.GetText(idx);
        _reader.Advance(idx + 1);

        return text;
    }

    private (string Key, string Value) ReadTagPair()
    {
        Debug.Assert(_reader.Current == '[');
        _reader.Advance();

        var key = ReadTagKey();
        SkipWhitespace();

        if (_reader.Current != '"')
            throw new PgnParsingException(PgnErrorType.TagError, "Unexpected symbol instead of tag value");

        var value = ReadTagValue();
        SkipWhitespace();

        if (_reader.Current != ']')
            throw new PgnParsingException(PgnErrorType.TagError, "Unclosed tag");

        _reader.Advance();

        return (key, value);
    }

    private string ReadTagKey()
    {
        SkipWhitespaceStrict();

        if (!char.IsAsciiLetterOrDigit(_reader.Current))
            throw new PgnParsingException(PgnErrorType.TagError, "Forbidden character in the tag key");

        var idx = _reader.IndexOfAny(s_tagKeyTerminators);

        if (idx < 0)
            throw new PgnParsingException(PgnErrorType.TagError, "Tag key can't be empty");

        if (idx > MaxTokenLength)
            throw new PgnParsingException(PgnErrorType.TagError, "Tag key is too long");

        var text = _reader.GetText(idx);
        _reader.Advance(idx);

        return text;
    }

    private string ReadTagValue()
    {
        Debug.Assert(_reader.Current == '"');
        _reader.Advance();

        // fast path
        var idxQuoteOrBackslash = _reader.IndexOfAny(s_tagValueTerminators);

        if (idxQuoteOrBackslash == -1)
            throw new PgnParsingException(PgnErrorType.TagError, "Unclosed tag value");

        if (_reader.Data[idxQuoteOrBackslash] == '"')
        {
            // Fast path for values without escape symbols
            var value = _reader.Data[..idxQuoteOrBackslash].ToString();
            _reader.Advance(idxQuoteOrBackslash + 1);
            return value;
        }

        var written = 0;
        var isEscaped = false;
        var isCompleted = false;

        while (_reader.Current != '\0' && written < MaxTokenLength)
        {
            if (_reader.Current == '\\')
            {
                if (isEscaped)
                {
                    _buffer[written++] = _reader.Current;
                }

                isEscaped = !isEscaped;
            }
            else if (_reader.Current == '"')
            {
                if (isEscaped)
                {
                    _buffer[written++] = _reader.Current;
                    isEscaped = false;
                }
                else
                {
                    isCompleted = true;
                    // Advance past the closing quote
                    _reader.Advance(1);
                    break;
                }
            }
            else if (_reader.Current == '\n')
            {
                throw new PgnParsingException(PgnErrorType.TagError, "Invalid character inside a string token");
            }
            else
            {
                if (isEscaped)
                    throw new PgnParsingException(PgnErrorType.TagError, "Invalid escape sequence inside a tag value");

                _buffer[written++] = _reader.Current;
            }

            _reader.Advance(1);
        }

        if (isCompleted)
            return _buffer.AsSpan(0, written).ToString();
        else if (written == MaxTokenLength)
            throw new PgnParsingException(PgnErrorType.TagError, "Tag value is too long");
        else if (written == 0)
            throw new PgnParsingException(PgnErrorType.TagError, "Tag value is empty");

        return string.Empty;
    }

    private bool TryRecoverFromError()
    {
        // Try to find either a new tag ('[') or a first move ('1.'), that follow a new line
        while (_reader.TryAdvanceTo('\n'))
        {
            _reader.Advance(1);
            if (_reader.Current == '['
                || (_reader.Current == '1' && _reader.Next == '.'))
            {
                return true;
            }
        }

        return false;
    }

    private void SkipWhitespace()
    {
        while (_reader.Current == ' ' || _reader.Current == '\r' || _reader.Current == '\n' || _reader.Current == '\t')
            _reader.Advance();
    }

    private void SkipWhitespaceStrict()
    {
        while (_reader.Current == ' ')
            _reader.Advance();
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                _reader.Dispose();
            }

            _disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
