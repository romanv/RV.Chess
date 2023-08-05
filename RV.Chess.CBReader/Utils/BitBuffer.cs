namespace RV.Chess.CBReader.Utils
{
    internal class BitBuffer
    {
        private readonly byte[] _bytes;
        private uint _currByteIdx = 0;
        private int _currBit = 8;

        internal BitBuffer(byte[] bytes)
        {
            _bytes = bytes;
        }

        public byte CurrByte => _bytes[_currByteIdx];

        public bool CanRead => _currByteIdx < _bytes.Length;

        internal int GetBit()
        {
            _currBit--;

            if (_currBit < 0)
            {
                _currBit = 7;
                _currByteIdx++;
            }

            return (CurrByte & (1 << _currBit)) > 0 ? 1 : 0;
        }
    }
}
