using System.Buffers.Binary;
using System.Collections.Immutable;
using FluentResults;
using RV.Chess.Board.Game;
using RV.Chess.CBReader.Entities;
using RV.Chess.CBReader.MoveDecoding;
using RV.Chess.CBReader.Utils;
using RV.Chess.Shared.Types;

namespace RV.Chess.CBReader.Readers
{
    internal class MovesReader : ReaderBase
    {
        internal override string FILE_EXTENSION => "cbg";

        const bool IGNORE_NON_ENGLISH_TEXTS = true;
        const bool IGNORE_EMPTY_HTML_BODY = true;
        const bool STIP_INLINE_IMAGES = true;

        private readonly uint _fileHeaderSize = 0;
        private readonly AnnotationsReader _annotationsReader;
        private const int GAME_RECORD_HEADER_SIZE = 4;

        public MovesReader(string dbPath, AnnotationsReader annotationsReader) : base(dbPath)
        {
            _annotationsReader = annotationsReader;

            if (!IsError)
            {
                // header size of a CBG file can differ between versions
                try
                {
                    _fileHeaderSize = _reader.ReadBytes(2).AsSpan().ToUIntBigEndian();
                }
                catch
                {
                    _fileHeaderSize = 0;
                }
            }
        }

        internal Result<IImmutableList<GuidingTextSection>> GetText(uint offset)
        {
            if (IsError)
            {
                return ImmutableList<GuidingTextSection>.Empty;
            }

            try
            {
                _fs.Seek(offset, SeekOrigin.Begin);
                var metadata = _reader.ReadBytes(8).AsSpan();
                var isGuidingText = (metadata[0] & 0b1000_0000) > 0;

                if (isGuidingText)
                {
                    var titlesCount = metadata.Slice(6, 2).ToUIntLittleEndian();
                    var titles = new Dictionary<TextLanguage, string>();

                    for (var i = 0; i < titlesCount; i++)
                    {
                        var titleLanguage = (TextLanguage)BinaryPrimitives.ReadUInt16LittleEndian(_reader.ReadBytes(2));
                        var titleLength = BinaryPrimitives.ReadUInt16LittleEndian(_reader.ReadBytes(2));
                        var title = _reader.ReadBytes(titleLength).AsSpan().ToCBZeroTerminatedString();
                        titles.Add(titleLanguage, title);
                    }

                    var textsCount = _reader.ReadBytes(3).AsSpan().Slice(1, 2).ToUIntLittleEndian();
                    var texts = new List<GuidingTextSection>();
                    var formatVersion = metadata.Slice(4, 2).ToUIntLittleEndian();

                    for (var i = 0; i < textsCount; i++)
                    {
                        (var language, var text) = ReadGuidingTextSegment(_reader, formatVersion);

                        var ignoredEmptyBody = IGNORE_EMPTY_HTML_BODY && formatVersion == 3 && IsEmptyHtmlBody(text);

                        if (!ignoredEmptyBody && (!IGNORE_NON_ENGLISH_TEXTS || language == TextLanguage.English))
                        {
                            texts.Add(new GuidingTextSection(
                                language,
                                titles.TryGetValue(language, out var lang) ? lang : string.Empty,
                                STIP_INLINE_IMAGES ? StripInlinedImages(text) : text
                            ));
                        }
                    }

                    return texts.ToImmutableList();
                }
            }
            catch
            {
            }

            return ImmutableList<GuidingTextSection>.Empty;
        }

        internal Result<GameMoves> Read(uint movesOffset, uint annotationsOffset)
        {
            if (IsError)
            {
                return Result.Fail(ErrorMessage);
            }

            try
            {
                _fs.Seek(movesOffset, SeekOrigin.Begin);
                var gameRecordHeader = _reader.ReadBytes(GAME_RECORD_HEADER_SIZE).AsSpan();
                var isDefaultStartPos = (gameRecordHeader[0] & 0b1000000) == 0;
                var encModeAndChessVar = gameRecordHeader[0] & 0b111111;

                // https://github.com/Yarin78/morphy/blob/master/morphy-cbh/docs/cbh-format/moves.md#encoding_modes
                // ignore everything but the most popular mode for now
                if (encModeAndChessVar != 0)
                {
                    return Result.Fail($"Move encoding mode #{encModeAndChessVar} is not supported");
                }

                var recordSize = (int)gameRecordHeader.Slice(1, 3).ToUIntBigEndian() - GAME_RECORD_HEADER_SIZE;
                (var game, var decodingState) = (new GameMoves(), new MoveDecodingState());

                if (!isDefaultStartPos)
                {
                    var newSetup = BuildGameSetup(_reader.ReadBytes(28).AsSpan());

                    if (newSetup.IsSuccess)
                    {
                        (game, decodingState) = newSetup.Value;
                    }
                    else
                    {
                        return Result.Fail(newSetup.Errors.FirstOrDefault()?.Message);
                    }
                }

                var annotations = _annotationsReader.GetAnnotations(annotationsOffset);
                if (annotations.IsFailed)
                {
                    return Result.Fail(annotations.Errors.FirstOrDefault()?.Message);
                }

                var moves = _reader.ReadBytes(recordSize).AsSpan();

                return MoveDecoder.Decode(moves, game, decodingState, annotations.Value);
            }
            catch (Exception ex)
            {
                return Result.Fail($"Move decoding error: {ex.Message}");
            }
        }

        private static bool IsEmptyHtmlBody(string text)
        {
            var tl = text.ToLower();
            var bodyStart = tl.IndexOf("<body");

            if (bodyStart < 0)
                return false;

            var bodyContentStart = tl.IndexOf(">", bodyStart + 5) + 1;

            if (bodyContentStart < 1)
                return false;

            var bodyContentEnd = tl.IndexOf("</body", bodyContentStart);

            if (bodyContentEnd < 0 || bodyContentEnd == bodyContentStart)
                return false;

            return text[bodyContentStart..bodyContentEnd].Trim().Length > 0;
        }

        private static (TextLanguage, string) ReadGuidingTextSegment(BinaryReader reader, uint version)
        {
            if (version == 1 || version == 2)
            {
                var size = version == 1 ? 2 : 4;
                var language = (TextLanguage)BinaryPrimitives.ReadUInt16LittleEndian(reader.ReadBytes(2));
                var textLength = size == 2
                    ? BinaryPrimitives.ReadInt16LittleEndian(reader.ReadBytes(size))
                    : BinaryPrimitives.ReadInt32LittleEndian(reader.ReadBytes(size));
                var text = reader.ReadBytes(textLength).AsSpan().ToCBString();
                var formattingSectionLength = size == 2
                    ? BinaryPrimitives.ReadInt16LittleEndian(reader.ReadBytes(size))
                    : BinaryPrimitives.ReadInt32LittleEndian(reader.ReadBytes(size));
                reader.ReadBytes(formattingSectionLength);

                return (language, text);
            }
            else if (version == 3)
            {
                var language = (TextLanguage)BinaryPrimitives.ReadUInt16LittleEndian(reader.ReadBytes(2));
                var textLength = BinaryPrimitives.ReadInt32LittleEndian(reader.ReadBytes(4));
                var text = reader.ReadBytes(textLength).AsSpan().ToCBString();
                reader.ReadBytes(4);

                return (language, text);
            }
            else
            {
                throw new NotImplementedException($"Guiding text format #{version} is not supported");
            }
        }

        private static string StripInlinedImages(string source)
        {
            var startPos = source.IndexOf("src=\"data:image/png");
            var result = source;

            while (startPos > -1)
            {
                var imgTagStartPos = result.LastIndexOf("<img", startPos);

                if (imgTagStartPos > -1)
                {
                    var endPos = result.IndexOf("\">", imgTagStartPos) + 2;

                    if (endPos > imgTagStartPos)
                    {
                        result = result.Remove(imgTagStartPos, endPos - imgTagStartPos);
                    }
                }

                startPos = result.IndexOf("src=\"data:image/png");
            }

            return result;
        }

        private static Result<(GameMoves, MoveDecodingState)> BuildGameSetup(Span<byte> setupHeader)
        {
            var sideToMove = (setupHeader[1] & 0b10000) > 0 ? Side.Black : Side.White;
            var enPassantFile = setupHeader[1] & 0b1111;
            var castlingRights = setupHeader[2] & 0b1111;
            var nextMoveNo = setupHeader[3] == 0 ? 1 : setupHeader[3];
            var piecesPart = setupHeader.Slice(4, 24);

            var decodingState = MoveDecodingState.GetCleanState();
            var chessgame = new Chessgame();
            chessgame.ClearBoard();
            chessgame.SetSide(sideToMove);
            chessgame.SetMoveNo(nextMoveNo);
            chessgame.SetCastling(GetCastlingRights(castlingRights));

            // 0 means there is no en passant, 1 is rank A etc.
            if (enPassantFile > 0)
            {
                var enPassantSquare = ((enPassantFile - 1) * 8) + sideToMove.Opposite() == Side.White ? 16 : 40;
                decodingState.SetEnPassant(enPassantSquare);
            }

            var rank = 0;
            var file = 0;
            var buf = new BitBuffer(piecesPart.ToArray());

            // pieces go in columns from a1-a8, b2-b8 etc
            while (rank < 8 && file < 8)
            {
                var isPiece = buf.GetBit();

                if (isPiece == 1)
                {
                    var side = buf.GetBit() == 0 ? Side.White : Side.Black;
                    var piece = ((buf.GetBit() << 2) | (buf.GetBit() << 1) | buf.GetBit()) switch
                    {
                        0b001 => PieceType.King,
                        0b010 => PieceType.Queen,
                        0b011 => PieceType.Knight,
                        0b100 => PieceType.Bishop,
                        0b101 => PieceType.Rook,
                        0b110 => PieceType.Pawn,
                        _ => PieceType.None,
                    };

                    if (piece == PieceType.None)
                    {
                        return Result.Fail("Invalid piece code during setup");
                    }

                    var square = rank * 8 + file;
                    decodingState.AddPiece(piece, side, square);
                    chessgame.AddPiece(piece, side, square);
                }

                rank++;

                if (rank > 7)
                {
                    file++;
                    rank = 0;
                }
            }

            decodingState.SetCastlingRights((CastlingRights)castlingRights);

            var cbgGame = new GameMoves
            {
                NextMoveNo = nextMoveNo,
                StartingFen = chessgame.Fen,
                StartingSide = sideToMove,
                StartingCastling = decodingState.CastlingRights,
            };

            return (cbgGame, decodingState);
        }

        private static CastlingRights GetCastlingRights(int rights)
        {
            var cr = CastlingRights.None;

            if ((rights & 0b1) > 0)
            {
                cr |= CastlingRights.WhiteQueenside;
            }

            if ((rights & 0b10) > 0)
            {
                cr |= CastlingRights.WhiteKingside;
            }

            if ((rights & 0b100) > 0)
            {
                cr |= CastlingRights.BlackQueenside;
            }

            if ((rights & 0b1000) > 0)
            {
                cr |= CastlingRights.BlackKingside;
            }

            return cr;
        }
    }
}
