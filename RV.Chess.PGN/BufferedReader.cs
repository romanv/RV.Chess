using System.Buffers;
using System.Diagnostics;
using System.Text;

namespace RV.Chess.PGN;

internal class BufferedReader : IDisposable
{
    private readonly Stream _stream;
    private readonly long _fullBufferSize;
    private readonly char[] _buffer;
    private readonly StreamReader _sr;

    private int _streamOffset = 0;
    private int _position = 0;
    private int _filledBufferSize;
    private bool _disposedValue;

    internal BufferedReader(Stream stream, int bufferSize)
    {
        _stream = stream;
        _sr = new StreamReader(_stream, Encoding.UTF8, bufferSize: bufferSize);
        _fullBufferSize = Math.Min(bufferSize, stream.Length);
        _buffer = new char[_fullBufferSize];
        _filledBufferSize = _sr.Read(_buffer, 0, (int)_fullBufferSize);
    }

    internal bool IsReadAll => _stream.Position >= _stream.Length;

    internal char Current => Peek(0);

    internal char Next => Peek(1);

    internal long Remaining => _filledBufferSize - _position + (_stream.Length - _stream.Position);

    internal ReadOnlySpan<char> Data => _buffer.AsSpan()[_position.._filledBufferSize];

    internal int ToStreamPosition(int idx) => _streamOffset + idx;

    internal string GetText(int length)
    {
        Debug.Assert(_position + length < _filledBufferSize);

        return Data[..length].ToString();
    }

    internal string GetText(int start, int length)
    {
        Debug.Assert(start + length < _filledBufferSize);

        return _buffer.AsSpan().Slice(start, length).ToString();
    }

    internal char Peek(long offset)
    {
        if (offset > _filledBufferSize)
        {
            return '\0';
        }

        if (_position + offset < _filledBufferSize)
        {
            return _buffer[_position + offset];
        }

        var remainingInBuffer = _filledBufferSize - _position;
        var remainingInStream = _stream.Length - _stream.Position;
        if (_position > 0 && remainingInBuffer + remainingInStream >= offset && AdvanceBuffer())
        {
            return _buffer[offset];
        }

        return '\0';
    }

    internal char Read()
    {
        var c = _buffer[_position];
        Advance();

        return c;
    }

    internal int IndexOf(char c)
    {
        var idx = Data.IndexOf(c);

        if (idx != -1)
            return idx;

        if (_position > 0)
        {
            AdvanceBuffer();
            return IndexOf(c);
        }

        return -1;
    }

    internal int IndexOfAny(SearchValues<char> chars)
    {
        var idx = Data.IndexOfAny(chars);

        if (idx != -1)
            return idx;

        if (_position > 0)
        {
            AdvanceBuffer();
            return IndexOfAny(chars);
        }

        return -1;
    }

    internal bool StartsWith(ReadOnlySpan<char> chars)
    {
        if (chars.Length <= Data.Length)
            return Data.StartsWith(chars);

        if (_position != 0 && Remaining > chars.Length)
        {
            AdvanceBuffer();
            return Data.StartsWith(chars);
        }

        return false;
    }

    internal void Advance()
    {
        _position++;

        if (_position >= _filledBufferSize)
            AdvanceBuffer();
    }

    internal void Advance(int offset)
    {
        if (offset > _fullBufferSize)
            throw new InvalidOperationException("Offset can't exceed the buffer size");

        _position += offset;

        if (_position >= _filledBufferSize)
            AdvanceBuffer();
    }

    internal bool TryAdvanceTo(char c)
    {
        while (true)
        {
            var idx = Data.IndexOf(c);

            if (idx != -1)
            {
                Advance(idx);
                return true;
            }
            else if (_position != 0 && !IsReadAll)
            {
                AdvanceBuffer();
            }
            else
            {
                return false;
            }
        }
    }

    internal void Reset()
    {
        _stream.Seek(0, SeekOrigin.Begin);
        _filledBufferSize = _sr.Read(_buffer, 0, (int)_fullBufferSize);
        _position = 0;
        _streamOffset = 0;
    }

    private bool AdvanceBuffer()
    {
        var rest = _buffer.AsSpan()[_position.._filledBufferSize];
        rest.CopyTo(_buffer);
        var freeSpace = _fullBufferSize - rest.Length;
        var read = _sr.ReadBlock(_buffer, rest.Length, (int)freeSpace);
        _streamOffset += _position;
        _position = 0;
        _filledBufferSize = rest.Length + read;

        return read > 0;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                _stream.Dispose();
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
