using RV.Chess.CBReader.Entities;
using RV.Chess.CBReader.Utils;

namespace RV.Chess.CBReader.Readers
{
    internal class PlayerDataReader : ReaderBase
    {
        internal override string FILE_EXTENSION => "cbp";
        internal readonly PlayerRecord UnknownPlayer = new(-1, "Unknown", string.Empty, 0);
        internal readonly Dictionary<int, PlayerRecord> _players;

        const uint FILE_HEADER_SIZE = 32;
        const int RECORD_SIZE = 67;

        internal PlayerDataReader(string dbPath) : base(dbPath)
        {
            _players = new() { { -1, UnknownPlayer } };
        }

        /*
           https://github.com/Yarin78/morphy/blob/master/morphy-cbh/docs/cbh-format/entities.md
           https://github.com/Yarin78/morphy/blob/master/morphy-cbh/docs/cbh-format/players.md
           Offset  Length
                0 	    4 	The id of the entity that is the left subtree of this node, or -1 if there is none.
                4 	    4 	The id of the entity that is the right subtree of this node, or -1 if there is none.
                8 	    1 	The height difference between the left and the right subtree.
                9 	    30 	Last name
                39 	    20 	First name
                59  	4 	Number of references to this player. In games were a player plays against themselves, this is counted twice.
                63 	    4 	The id of the first game in the database with this player.
        */
        internal PlayerRecord GetPlayer(int id)
        {
            if (_players.TryGetValue(id, out var cached))
            {
                return cached;
            }

            if (IsError)
            {
                return UnknownPlayer;
            }

            try
            {
                _fs.Seek(FILE_HEADER_SIZE + id * RECORD_SIZE, SeekOrigin.Begin);
                var record = _reader.ReadBytes(RECORD_SIZE).AsSpan();
                var player = new PlayerRecord(id,
                    record.Slice(9, 30).ToCBZeroTerminatedString(),
                    record.Slice(39, 20).ToCBZeroTerminatedString(),
                    record.Slice(63, 4).ToUIntBigEndian());
                _players[id] = player;

                return player;
            }
            catch
            {
                _players[id] = UnknownPlayer;
            }

            return UnknownPlayer;
        }
    }
}
