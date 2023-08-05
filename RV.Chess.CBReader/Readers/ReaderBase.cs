using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace RV.Chess.CBReader.Readers
{
    abstract class ReaderBase : IDisposable
    {
        protected readonly string _fileName;
        internal readonly FileStream? _fs;
        internal readonly BinaryReader? _reader;

        internal ReaderBase(string fileName)
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
            }
        }

        internal abstract string FILE_EXTENSION { get; }

        [MemberNotNullWhen(returnValue: false, nameof(_fs))]
        [MemberNotNullWhen(returnValue: false, nameof(_reader))]
        internal bool IsError { get; private set; }

        internal string ErrorMessage { get; private set; } = string.Empty;

        public void Dispose()
        {
            if (!IsError)
            {
                _reader.Dispose();
                _fs.Dispose();
            }
        }
    }
}
