using System;
using System.Diagnostics.CodeAnalysis;

namespace RV.Chess.PGN.Readers
{
    internal class StringPgnReader : IPgnReader
    {
        private bool _disposedValue;
        private int _previousChunkStart = 0;
        private string _s = string.Empty;

        public int Col { get; private set; }

        public int Row { get; private set; }

        public bool TryGetGameChunk([NotNullWhen(true)] out PgnGameChunk chunk)
        {
            var cursor = 0;
            var isInsideStringToken = false;
            var isInsideComment = false;
            var isNextEscaped = false;
            var isInsideTag = false;

            while (true)
            {
                Col++;

                if (cursor < _s.Length)
                {
                    if (_s[cursor] == '[' && !isInsideComment && !isInsideStringToken)
                    {
                        isInsideTag = true;
                    }
                    else if (_s[cursor] == ']' && isInsideTag && !isNextEscaped && !isInsideStringToken)
                    {
                        isInsideTag = false;
                        // Some pgn's have malformed tags like [Event "City /"], which leads to
                        // the rest of the pgn being treated as the unclosed string token.
                        // Forcibly closing string token when tag pair ends helps with that.
                        isInsideStringToken = false;
                    }
                    else if (_s[cursor] == '"')
                    {
                        if (isNextEscaped)
                        {
                            isNextEscaped = false;
                        }
                        else if (isInsideStringToken)
                        {
                            isInsideStringToken = false;
                        }
                        else
                        {
                            isInsideStringToken = true;
                        }
                    }
                    else if (_s[cursor] == '{' && !isNextEscaped && !isInsideStringToken)
                    {
                        isInsideComment = true;
                    }
                    else if (_s[cursor] == '}' && !isNextEscaped)
                    {
                        isInsideComment = false;
                    }
                    else if (_s[cursor] == '\\' && !isNextEscaped)
                    {
                        isNextEscaped = true;
                    }
                    else if (_s[cursor] == '1' && !isInsideStringToken && !isInsideComment)
                    {
                        // look behind if we are in a '0-1' tag
                        if (cursor - 2 >= _previousChunkStart && _s[cursor - 1] == '-' && _s[cursor - 2] == '0')
                        {
                            chunk = new PgnGameChunk
                            {
                                Text = _s.AsSpan().Slice(_previousChunkStart, cursor - _previousChunkStart + 1),
                                Col = Col,
                                Row = Row,
                            };
                            _previousChunkStart = cursor + 1;
                            return true;
                        }
                    }
                    else if (_s[cursor] == '0' && !isInsideStringToken && !isInsideComment)
                    {
                        // look behind if we are in a '1-0' tag
                        if (cursor - 2 >= _previousChunkStart && _s[cursor - 1] == '-' && _s[cursor - 2] == '1')
                        {
                            chunk = new PgnGameChunk
                            {
                                Text = _s.AsSpan().Slice(_previousChunkStart, cursor - _previousChunkStart + 1),
                                Col = Col,
                                Row = Row,
                            };
                            _previousChunkStart = cursor + 1;
                            return true;
                        }
                    }
                    else if (_s[cursor] == '2' && !isInsideStringToken && !isInsideComment)
                    {
                        // look behind if we are in a '1/2-1/2' tag
                        if (cursor - 6 >= _previousChunkStart
                            && _s[cursor - 1] == '/'
                            && _s[cursor - 2] == '1'
                            && _s[cursor - 3] == '-'
                            && _s[cursor - 4] == '2'
                            && _s[cursor - 5] == '/'
                            && _s[cursor - 6] == '1')
                        {
                            chunk = new PgnGameChunk
                            {
                                Text = _s.AsSpan().Slice(_previousChunkStart, cursor - _previousChunkStart + 1),
                                Col = Col,
                                Row = Row,
                            };
                            _previousChunkStart = cursor + 1;
                            return true;
                        }
                    }
                    else if (_s[cursor] == '*' && !isInsideStringToken && !isInsideComment)
                    {
                        chunk = new PgnGameChunk
                        {
                            Text = _s.AsSpan().Slice(_previousChunkStart, cursor - _previousChunkStart + 1),
                            Col = Col,
                            Row = Row,
                        };
                        _previousChunkStart = cursor + 1;
                        return true;
                    }
                    else if (_s[cursor] == '\n')
                    {
                        Row++;
                        Col = 0;
                    }
                    else
                    {
                        isNextEscaped = false;
                    }

                    cursor++;
                }
                else
                {
                    chunk = default;
                    return false;
                }
            }
        }

        internal static StringPgnReader Open(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                throw new ArgumentException("Pgn text cannot be empty");
            }

            var reader = new StringPgnReader()
            {
                _s = s,
            };

            return reader;
        }

        public void Reset()
        {
            _previousChunkStart = 0;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
