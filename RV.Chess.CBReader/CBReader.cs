using System.Diagnostics.CodeAnalysis;
using FluentResults;
using RV.Chess.Board.Game;
using RV.Chess.Board.Utils;
using RV.Chess.CBReader.Entities;
using RV.Chess.CBReader.Readers;

namespace RV.Chess.CBReader
{
    public class CBReader
    {
        private static readonly string[] _fileExtensions = new string[] { "cbh", "cbp" };

        private readonly GameHeadersReader? _headersReader;
        private readonly PlayerDataReader? _playerReader;
        private readonly MovesReader? _movesReader;
        private readonly AnnotationsReader? _annotationsReader;
        private readonly TopGamesReader? _topGamesReader;
        private readonly PositionSearchBoosterReader? _positionSearchBoosterReader;
        private readonly EntitySearchIndexBoosterReader? _entitySearchIndexBoosterReader;
        private readonly TournamentReader? _tournamentReader;

        private CBReader(string dbDirectory, string dbName)
        {
            var dbPathNoExtension = Path.Combine(dbDirectory, dbName);

            try
            {
                _playerReader = new PlayerDataReader(dbPathNoExtension);
                _annotationsReader = new AnnotationsReader(dbPathNoExtension);
                _movesReader = new MovesReader(dbPathNoExtension, _annotationsReader);
                _topGamesReader = new TopGamesReader(dbPathNoExtension);
                _positionSearchBoosterReader = new PositionSearchBoosterReader(dbPathNoExtension);
                var gameIdsBoosterReader = new GameIdsBoosterReader(dbPathNoExtension);
                _entitySearchIndexBoosterReader =
                    new EntitySearchIndexBoosterReader(dbPathNoExtension, gameIdsBoosterReader);
                _headersReader = new GameHeadersReader(dbPathNoExtension, _playerReader, _movesReader);
                _tournamentReader = new TournamentReader(dbPathNoExtension);
            }
            catch (Exception ex)
            {
                IsError = true;
                ErrorMessage = ex.Message;
            }
        }

        [MemberNotNullWhen(returnValue: false, nameof(_headersReader))]
        [MemberNotNullWhen(returnValue: false, nameof(_playerReader))]
        [MemberNotNullWhen(returnValue: false, nameof(_movesReader))]
        [MemberNotNullWhen(returnValue: false, nameof(_annotationsReader))]
        [MemberNotNullWhen(returnValue: false, nameof(_topGamesReader))]
        [MemberNotNullWhen(returnValue: false, nameof(_positionSearchBoosterReader))]
        [MemberNotNullWhen(returnValue: false, nameof(_entitySearchIndexBoosterReader))]
        [MemberNotNullWhen(returnValue: false, nameof(_tournamentReader))]
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

            var missingFile = Array.Find(_fileExtensions, ext =>
                !File.Exists(Path.Combine(dbDirectory, $"{dbFileName}.{ext}")));

            return missingFile == null
                ? new CBReader(dbDirectory, dbFileName)
                : Result.Fail($"{dbFileName}.{missingFile} is missing");
        }

        public Result<IEnumerable<Result<PlayerRecord>>> GetAllPlayers()
        {
            if (IsError)
            {
                return Result.Fail(ErrorMessage);
            }

            try
            {
                return Result.Ok(_playerReader.GetAll());
            }
            catch (Exception ex)
            {
                return Result.Fail(ex.Message);
            }
        }

        public Result<IEnumerable<Result<uint>>> GetGamesByPlayer(uint id)
        {
            if (IsError)
            {
                return Result.Fail(ErrorMessage);
            }

            try
            {
                return Result.Ok(_entitySearchIndexBoosterReader.GetGamesByPlayer(id));
            }
            catch (Exception ex)
            {
                return Result.Fail(ex.Message);
            }
        }

        public Result<IEnumerable<Result<CbhRecord>>> GetGameHeaders()
        {
            if (IsError)
            {
                return Result.Fail(ErrorMessage);
            }

            try
            {
                return Result.Ok(_headersReader.Read(0, int.MaxValue));
            }
            catch (Exception ex)
            {
                return Result.Fail(ex.Message);
            }
        }

        public Result<IEnumerable<Result<CbhRecord>>> GetGameHeaders(uint count)
        {
            if (IsError)
            {
                return Result.Fail(ErrorMessage);
            }

            try
            {
                return Result.Ok(_headersReader.Read(0, count));
            }
            catch (Exception ex)
            {
                return Result.Fail(ex.Message);
            }
        }

        public Result<IEnumerable<Result<CbhRecord>>> GetGameHeaders(uint skip, uint count)
        {
            if (IsError)
            {
                return Result.Fail(ErrorMessage);
            }

            try
            {
                return Result.Ok(_headersReader.Read(skip, count));
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

            try
            {
                return _headersReader.Read(id, 1).First();
            }
            catch (Exception ex)
            {
                return Result.Fail(ex.Message);
            }
        }

        public Result<GameMoves> GetMoves(CbGameMetadata gm)
        {
            if (IsError)
            {
                return Result.Fail(ErrorMessage);
            }

            try
            {
                return _movesReader.Read(gm.MovesDataStartOffset, gm.AnnotationsDataStartOffset);
            }
            catch (Exception ex)
            {
                return Result.Fail(ex.Message);
            }
        }

        public Result<GameMoves> GetMoves(uint movesOffset, uint annotationsOffset)
        {
            if (IsError)
            {
                return Result.Fail(ErrorMessage);
            }

            try
            {
                return _movesReader.Read(movesOffset, annotationsOffset);
            }
            catch (Exception ex)
            {
                return Result.Fail(ex.Message);
            }
        }

        public Result<IEnumerable<Result<uint>>> GetTopGames(uint count = int.MaxValue)
        {
            if (IsError)
            {
                return Result.Fail(ErrorMessage);
            }

            try
            {
                return Result.Ok(_topGamesReader.Read(count));
            }
            catch (Exception ex)
            {
                return Result.Fail(ex.Message);
            }
        }

        public Result<IEnumerable<Result<uint>>> FindGamesWithSearchBooster(string fen)
        {
            if (IsError)
            {
                return Result.Fail(ErrorMessage);
            }

            try
            {
                return FindGamesWithSearchBooster(new Chessgame(fen));
            }
            catch (Exception ex)
            {
                return Result.Fail(ex.Message);
            }
        }

        public Result<IEnumerable<Result<uint>>> FindGamesWithSearchBooster(Chessgame game)
        {
            if (IsError)
            {
                return Result.Fail(ErrorMessage);
            }

            try
            {
                var mask = CBPositionSearchBoosterEncoder.Encode(game);
                return Result.Ok(_positionSearchBoosterReader.Find(mask));
            }
            catch (Exception ex)
            {
                return Result.Fail(ex.Message);
            }
        }

        public Result<IEnumerable<Result<TournamentRecord>>> GetAllTournaments()
        {
            if (IsError)
            {
                return Result.Fail(ErrorMessage);
            }

            try
            {
                return Result.Ok(_tournamentReader.GetAll());
            }
            catch (Exception ex)
            {
                return Result.Fail(ex.Message);
            }
        }
    }
}
