using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace RV.Chess.PGN.Readers
{
    internal class StrictPgnFileReader : IPgnReader
    {
        const int BUFFER_SIZE = 8 * 1024 * 1024;
        private readonly char[] _buffer = new char[BUFFER_SIZE];
        private FileStream? _fs;
        private StreamReader? _sr;
        private int _bufferEnd = 0;
        private int _col = 1;
        private int _previousChunkStart = 0;
        private int _row = 1;
        private bool _disposedValue;

        private StrictPgnFileReader() { }

        public bool TryGetGameChunk([NotNullWhen(true)] out PgnGameChunk chunk)
        {
            chunk = default;

            if (_sr == null)
            {
                return false;
            }

            // attempt to find the game terminator tag inside the buffer
            var cursor = _previousChunkStart;
            var isInsideStringToken = false;
            var isInsideComment = false;
            var isNextEscaped = false;
            var isInsideTag = false;
            var startRow = _row;
            var startCol = _col;

            while (true)
            {
                _col++;

                if (cursor < _bufferEnd)
                {
                    if (_buffer[cursor] == '[' && !isInsideComment && !isInsideStringToken)
                    {
                        isInsideTag = true;
                    }
                    else if (_buffer[cursor] == ']' && isInsideTag && !isNextEscaped && !isInsideStringToken)
                    {
                        isInsideTag = false;
                        // Some pgn's have malformed tags like [Event "City /"], which leads to
                        // the rest of the pgn being treated as the unclosed string token.
                        // Forcibly closing string token when tag pair ends helps with that.
                        isInsideStringToken = false;
                    }
                    else if (_buffer[cursor] == '"')
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
                    else if (_buffer[cursor] == '{' && !isNextEscaped && !isInsideStringToken)
                    {
                        isInsideComment = true;
                    }
                    else if (_buffer[cursor] == '}' && !isNextEscaped)
                    {
                        isInsideComment = false;
                    }
                    else if (_buffer[cursor] == '\\' && !isNextEscaped)
                    {
                        isNextEscaped = true;
                    }
                    else if (_buffer[cursor] == '1' && !isInsideStringToken && !isInsideComment)
                    {
                        // look behind if we are in a '0-1' tag
                        if (cursor - 2 >= _previousChunkStart && _buffer[cursor - 1] == '-' && _buffer[cursor - 2] == '0')
                        {
                            chunk = new PgnGameChunk
                            {
                                Text = _buffer.AsSpan().Slice(_previousChunkStart, cursor - _previousChunkStart + 1),
                                Col = startCol,
                                Row = startRow,
                            };
                            _previousChunkStart = cursor + 1;
                            return true;
                        }
                    }
                    else if (_buffer[cursor] == '0' && !isInsideStringToken && !isInsideComment)
                    {
                        // look behind if we are in a '1-0' tag
                        if (cursor - 2 >= _previousChunkStart && _buffer[cursor - 1] == '-' && _buffer[cursor - 2] == '1')
                        {
                            chunk = new PgnGameChunk
                            {
                                Text = _buffer.AsSpan().Slice(_previousChunkStart, cursor - _previousChunkStart + 1),
                                Col = startCol,
                                Row = startRow,
                            };
                            _previousChunkStart = cursor + 1;
                            return true;
                        }
                    }
                    else if (_buffer[cursor] == '2' && !isInsideStringToken && !isInsideComment)
                    {
                        // look behind if we are in a '1/2-1/2' tag
                        if (cursor - 6 >= _previousChunkStart
                            && _buffer[cursor - 1] == '/'
                            && _buffer[cursor - 2] == '1'
                            && _buffer[cursor - 3] == '-'
                            && _buffer[cursor - 4] == '2'
                            && _buffer[cursor - 5] == '/'
                            && _buffer[cursor - 6] == '1')
                        {
                            chunk = new PgnGameChunk
                            {
                                Text = _buffer.AsSpan().Slice(_previousChunkStart, cursor - _previousChunkStart + 1),
                                Col = startCol,
                                Row = startRow,
                            };
                            _previousChunkStart = cursor + 1;
                            return true;
                        }
                    }
                    else if (_buffer[cursor] == '*' && !isInsideStringToken && !isInsideComment)
                    {
                        chunk = new PgnGameChunk
                        {
                            Text = _buffer.AsSpan().Slice(_previousChunkStart, cursor - _previousChunkStart + 1),
                            Col = startCol,
                            Row = startRow,
                        };
                        _previousChunkStart = cursor + 1;
                        return true;
                    }
                    else if (_buffer[cursor] == '\n')
                    {
                        _row++;
                        _col = 0;
                    }
                    else
                    {
                        isNextEscaped = false;
                    }

                    cursor++;
                }

                if (cursor == _bufferEnd)
                {
                    // if we are at the end and still found nothing, check if we can reallocate
                    // the tail end of the buffer to the start and append new chunk of the file to it
                    if (_previousChunkStart > 0)
                    {
                        var remainingChunkSize = _bufferEnd - _previousChunkStart;
                        Buffer.BlockCopy(_buffer, _previousChunkStart * sizeof(char),
                            _buffer, 0, remainingChunkSize * sizeof(char));
                        var charsRead = _sr.ReadBlock(_buffer, remainingChunkSize, BUFFER_SIZE - remainingChunkSize);

                        if (charsRead > 0)
                        {
                            _bufferEnd = remainingChunkSize + charsRead;
                            _previousChunkStart = 0;
                            cursor = remainingChunkSize;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
            }
        }

        internal static StrictPgnFileReader Open(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException("Path cannot be empty");
            }

            if (!File.Exists(path))
            {
                throw new ArgumentException("File does not exist");
            }

            var reader = new StrictPgnFileReader
            {
                _fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
            };
            reader._sr = new StreamReader(reader._fs, Encoding.UTF8);
            reader._bufferEnd = reader._sr.ReadBlock(reader._buffer, 0, BUFFER_SIZE);
            return reader;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _sr?.Dispose();
                    _fs?.Dispose();
                }

                _disposedValue = true;
            }
        }

        public void Reset()
        {
            _fs?.Seek(0, SeekOrigin.Begin);
            _bufferEnd = 0;
            _col = 1;
            _previousChunkStart = 0;
            _row = 1;
            _bufferEnd = _sr?.ReadBlock(_buffer, 0, BUFFER_SIZE) ?? 0;
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
