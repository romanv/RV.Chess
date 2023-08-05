using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using FluentResults;
using RV.Chess.CBReader.Entities;
using RV.Chess.CBReader.Readers;

namespace RV.Chess.CBReader
{
    public class CBReader
    {
        private static readonly string[] _fileExtensions = new string[] { "cbh", "cbp" };
        private readonly string _dbDirectory;
        private readonly string _dbName;
        private readonly string _dbPathNoExtension;
        private readonly GameHeadersReader? _gameReader;
        private readonly PlayerDataReader? _playerReader;
        private readonly MovesReader _movesReader;
        private readonly AnnotationsReader _annotationsReader;
        private readonly TopGamesReader _topGamesReader;

        private CBReader(string dbDirectory, string dbName)
        {
            _dbDirectory = dbDirectory;
            _dbName = dbName;
            _dbPathNoExtension = Path.Combine(dbDirectory, dbName);

            _playerReader = new PlayerDataReader(_dbPathNoExtension);
            _annotationsReader = new AnnotationsReader(_dbPathNoExtension);
            _movesReader = new MovesReader(_dbPathNoExtension, _annotationsReader);
            _topGamesReader = new TopGamesReader(_dbPathNoExtension);

            if (_playerReader.IsError || _movesReader.IsError || _annotationsReader.IsError || _topGamesReader.IsError)
            {
                IsError = true;
                ErrorMessage = "Initialization error";
                return;
            }

            _gameReader = new GameHeadersReader(_dbPathNoExtension, _playerReader, _movesReader);

            if (_gameReader.IsError)
            {
                IsError = true;
                ErrorMessage = _gameReader.ErrorMessage;
            }
        }

        [MemberNotNullWhen(returnValue: false, nameof(_playerReader))]
        [MemberNotNullWhen(returnValue: false, nameof(_gameReader))]
        [MemberNotNullWhen(returnValue: false, nameof(_movesReader))]
        [MemberNotNullWhen(returnValue: false, nameof(_annotationsReader))]
        [MemberNotNullWhen(returnValue: false, nameof(_topGamesReader))]
        public bool IsError { get; private set; }

        public string ErrorMessage { get; private set; } = string.Empty;

        public static Result<CBReader> Open(string dbDirectory)
        {
            if (!Directory.Exists(dbDirectory))
            {
                return Result.Fail("Directory not found");
            }

            var dbFilePath = Directory.EnumerateFiles(dbDirectory, "*.cbh").FirstOrDefault();

            if (dbFilePath == null)
            {
                return Result.Fail("CBH file not found");
            }

            var dbFileName = Path.GetFileNameWithoutExtension(dbFilePath);

            var missingFile = _fileExtensions.FirstOrDefault(ext =>
                !File.Exists(Path.Combine(dbDirectory, $"{dbFileName}.{ext}")));

            return missingFile == null
                ? new CBReader(dbDirectory, dbFileName)
                : Result.Fail($"{dbFileName}.{missingFile} is missing");
        }

        public Result<IImmutableList<Result<CbhRecord>>> GetCbhRecords()
        {
            try
            {
                return IsError
                    ? Result.Fail(ErrorMessage)
                    : _gameReader.Read(0, int.MaxValue).ToImmutableList();
            }
            catch (Exception ex)
            {
                return Result.Fail(ex.Message);
            }
        }

        public Result<IImmutableList<Result<CbhRecord>>> GetGameHeaders()
        {
            try
            {
                return IsError
                    ? Result.Fail(ErrorMessage)
                    : _gameReader.Read(0, int.MaxValue).ToImmutableList();
            }
            catch (Exception ex)
            {
                return Result.Fail(ex.Message);
            }
        }

        public Result<IImmutableList<Result<CbhRecord>>> GetGameHeaders(uint count)
        {
            try
            {
                return IsError
                    ? Result.Fail(ErrorMessage)
                    : _gameReader.Read(0, count).ToImmutableList();
            }
            catch (Exception ex)
            {
                return Result.Fail(ex.Message);
            }
        }

        public Result<IImmutableList<Result<CbhRecord>>> GetGameHeaders(uint skip, uint count)
        {
            try
            {
                return IsError
                    ? Result.Fail(ErrorMessage)
                    : _gameReader.Read(skip, count).ToImmutableList();
            }
            catch (Exception ex)
            {
                return Result.Fail(ex.Message);
            }
        }

        public Result<CbhRecord> GetGameHeader(uint id)
        {
            if (IsError)
            {
                return Result.Fail(ErrorMessage);
            }

            return _gameReader.Read(id, 1).FirstOrDefault() ?? Result.Fail("Game not found");
        }

        public Result<GameMoves> GetMoves(uint movesOffset, uint annotationsOffset)
        {
            return IsError
                ? Result.Fail(ErrorMessage)
                : _movesReader.Read(movesOffset, annotationsOffset);
        }

        public Result<HashSet<uint>> GetTopGames(uint count = int.MaxValue)
        {
            return IsError
                ? Result.Fail(ErrorMessage)
                : _topGamesReader.Read(count);
        }
    }
}
