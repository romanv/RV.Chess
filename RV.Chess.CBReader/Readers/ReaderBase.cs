using System.Diagnostics.CodeAnalysis;

namespace RV.Chess.CBReader.Readers
{
    abstract internal class ReaderBase : IDisposable
    {
        internal protected readonly string _fileName;
        internal protected readonly FileStream? _fs;
        internal protected readonly BinaryReader? _reader;
        private bool _disposedValue;

        private protected ReaderBase(string fileName)
        {
            _fileName = $"{fileName}.{FILE_EXTENSION}";

            try
            {
                _fs = File.OpenRead(_fileName);
                _reader = new BinaryReader(_fs);
            }
            catch (Exception ex)
            {
                IsError = true;
                ErrorMessage = ex.Message;
                throw;
            }
        }

        internal abstract string FILE_EXTENSION { get; }

        [MemberNotNullWhen(returnValue: false, nameof(_fs))]
        [MemberNotNullWhen(returnValue: false, nameof(_reader))]
        internal bool IsError { get; private set; }

        internal string ErrorMessage { get; private set; } = string.Empty;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing && !IsError)
                {
                    _reader.Dispose();
                    _fs.Dispose();
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
