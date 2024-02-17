using System.Text;

namespace RV.Chess.PGN.Readers
{
    internal class SimplePgnFileReader : IPgnReader
    {
        const int BUFFER_SIZE = 8 * 1024 * 1024;
        const int MIN_FRAME_SIZE = 8 * 1024;
        private readonly char[] _buffer = new char[BUFFER_SIZE];
        private readonly char[] _resultTag = new char[] { '[', 'R', 'e', 's', 'u', 'l', 't', ' ', '"' };
        private FileStream? _fs;
        private StreamReader? _sr;
        private int _frameStart = 0;
        private int _frameEnd = 0;
        private int _position = 0;
        private bool _isAtEnd = false;
        private bool _disposedValue;

        private SimplePgnFileReader() { }

        public bool TryGetGameChunk(out PgnGameChunk chunk)
        {
            if (_frameEnd <= _frameStart && _isAtEnd)
            {
                chunk = default;
                return false;
            }

            if (_frameEnd - _frameStart < MIN_FRAME_SIZE && !_isAtEnd)
            {
                FillBuffer();
            }

            var frame = _buffer.AsSpan()[_frameStart.._frameEnd];
            var resultTagStart = MemoryExtensions.IndexOf(frame, _resultTag, StringComparison.Ordinal);

            if (resultTagStart < 0)
            {
                chunk = default;
                return false;
            }

            resultTagStart += 9; // skip the '[Result "' part
            var resultTagEnd = MemoryExtensions.IndexOf(frame[resultTagStart..], '"');

            if (resultTagEnd < 0)
            {
                chunk = default;
                return false;
            }

            resultTagEnd += resultTagStart;
            var terminatorValue = frame[resultTagStart..resultTagEnd];
            var terminatorTagStart = MemoryExtensions.IndexOf(frame[resultTagEnd..], terminatorValue);

            if (terminatorTagStart < 0)
            {
                chunk = default;
                return false;
            }

            terminatorTagStart += resultTagEnd;
            var game = frame[..(terminatorTagStart + terminatorValue.Length)].ToString();
            _frameStart += terminatorTagStart + terminatorValue.Length + 1;

            chunk = new PgnGameChunk
            {
                Col = 0,
                Row = 0,
                ChunkStartPos = (uint)_position,
                Text = game,
            };

            _position += terminatorTagStart + terminatorValue.Length;

            return true;
        }

        internal static SimplePgnFileReader Open(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException("Path cannot be empty");
            }

            if (!File.Exists(path))
            {
                throw new ArgumentException("File does not exist");
            }

            var reader = new SimplePgnFileReader
            {
                _fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
            };
            reader._sr = new StreamReader(reader._fs, Encoding.UTF8);
            reader.FillBuffer();
            return reader;
        }

        private void FillBuffer()
        {
            if (_sr == null)
            {
                return;
            }

            var frameSize = _frameEnd - _frameStart;
            var freeSpace = BUFFER_SIZE - frameSize;
            Buffer.BlockCopy(_buffer, _frameStart * sizeof(char), _buffer, 0, frameSize * sizeof(char));
            _frameStart = 0;
            var charsRead = _sr.ReadBlock(_buffer, frameSize, freeSpace);
            _frameEnd = frameSize + charsRead;

            if (charsRead < freeSpace)
            {
                _isAtEnd = true;
            }

            return;
        }

        public void Reset()
        {
            _fs?.Seek(0, SeekOrigin.Begin);
            _frameStart = 0;
            _frameEnd = 0;
            _position = 0;
            _isAtEnd = false;
            FillBuffer();
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

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
