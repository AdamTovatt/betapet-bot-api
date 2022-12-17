using Betapet.Helpers;
using Betapet.Models.InGame;
using Newtonsoft.Json;
using Betapet.Helpers;
using System.Text.Json.Serialization;

namespace Betapet.Models
{
    public class Game
    {
        [JsonProperty("gameid")]
        public int Id { get; set; }

        [JsonProperty("start_time")]
        public DateTime StartTime { get; set; }

        [JsonProperty("board_type")]
        public int BoardType { get; set; }

        [JsonProperty("player_cnt")]
        public int PlayerCount { get; set; }

        [JsonProperty("wordlist")]
        public int WordList { get; set; }

        [JsonProperty("activity")]
        public int Activity { get; set; }

        [JsonProperty("activity_time")]
        public DateTime ActivityTime { get; set; }

        [JsonProperty("active")]
        public int Active { get; set; }

        [JsonProperty("status")]
        public int Status { get; set; }

        [JsonProperty("board_data")]
        public string BoardData { get; set; }

        [JsonProperty("board_data_org")]
        public string OriginalBoardData { get; set; }

        [JsonProperty("turn")]
        public int Turn { get; set; }

        [JsonProperty("last_chat_time")]
        public DateTime LastChatTime { get; set; }

        [JsonProperty("user_list")]
        public List<InGameUser> UserList { get; set; }

        [JsonProperty("tiles_left")]
        public int TilesLeft { get; set; }

        [JsonProperty("tiles_percent")]
        public int TilesPercent { get; set; }

        [JsonProperty("fails")]
        public int Fails { get; set; }

        [JsonProperty("swap_count")]
        public int SwapCount { get; set; }

        [JsonProperty("words_first")]
        public string FirstWord { get; set; }

        [JsonProperty("words")]
        public string Words { get; set; }

        [JsonProperty("bingo")]
        public bool Bingo { get; set; }

        [JsonProperty("mark")]
        public List<string> Mark { get; set; }

        /// <summary>
        /// A list of all tiles that can be used. That is the tiles currently in our hand + the tiles on the board
        /// </summary>
        public List<Tile> UseableTiles { get { if (_useableTiles == null) _useableTiles = Hand.AddTiles(PlayedTiles); return _useableTiles; } }
        private List<Tile> _useableTiles;

        /// <summary>
        /// The tiles that have been played to the board
        /// </summary>
        public List<Tile> PlayedTiles { get { return Board.LetterTilesOnBoard; } }

        /// <summary>
        /// The tiles that are hidden from us. Either they are in the opponents hand or they are in the tiles stash.
        /// We can't know unless the tiles left are 0, then we know that all the hidden tiles are in the opponents hand.
        /// </summary>
        public List<Tile> HiddenTiles { get { if (_hiddenTiles == null) _hiddenTiles = GetHiddenTiles(); return _hiddenTiles; } }
        private List<Tile> _hiddenTiles;

        /// <summary>
        /// Our currently available tiles to place
        /// </summary>
        public List<Tile> Hand { get { if (_hand == null) _hand = GetHand(); return _hand; } }
        private List<Tile> _hand;

        /// <summary>
        /// The board
        /// </summary>
        public Board Board { get { if (_board == null) _board = new Board(BoardData); return _board; } }
        private Board _board { get; set; }

        /// <summary>
        /// The original board, free from any placed tiles
        /// </summary>
        public Board OriginalBoard { get { if (_originalBoard == null) _originalBoard = new Board(OriginalBoardData); return _originalBoard; } }
        public Board _originalBoard;

        /// <summary>
        /// Tells wether or not it's our turn to play
        /// </summary>
        public bool OurTurn { get { if (_ourTurn == null) CheckOurTurn(); return (bool)_ourTurn; } }
        private bool? _ourTurn;

        public static Game FromJson(string json)
        {
            return JsonConvert.DeserializeObject<Game>(json);
        }

        private List<Tile> GetHand()
        {
            return UserList.Where(x => x.Hand != null).First().Hand.ToListOfTiles();
        }

        private void CheckOurTurn()
        {
            if (UserList == null || UserList.Count < 2)
                throw new Exception("Invalid user list in game");

            int ourIndex = 0;

            if (UserList[0].Hand == null)
                ourIndex = 1;

            _ourTurn = Active == ourIndex;
        }

        private List<Tile> GetHiddenTiles()
        {
            return GetStartingTiles().RemoveTiles(Hand).RemoveTiles(Board.LetterTilesOnBoard);
        }

        private List<Tile> GetStartingTiles()
        {
            List<Tile> tiles = new List<Tile>();

            tiles.AddTile("A", 9);
            tiles.AddTile("B", 2);
            tiles.AddTile("C", 1);
            tiles.AddTile("D", 5);
            tiles.AddTile("E", 8);
            tiles.AddTile("F", 2);
            tiles.AddTile("G", 3);
            tiles.AddTile("H", 2);
            tiles.AddTile("I", 5);
            tiles.AddTile("J", 1);
            tiles.AddTile("K", 3);
            tiles.AddTile("L", 5);
            tiles.AddTile("M", 3);
            tiles.AddTile("N", 5);
            tiles.AddTile("O", 5);
            tiles.AddTile("P", 2);
            tiles.AddTile("R", 7);
            tiles.AddTile("S", 8);
            tiles.AddTile("T", 8);
            tiles.AddTile("U", 3);
            tiles.AddTile("V", 2);
            tiles.AddTile("X", 1);
            tiles.AddTile("Y", 1);
            tiles.AddTile("Z", 1);
            tiles.AddTile("Å", 2);
            tiles.AddTile("Ä", 2);
            tiles.AddTile("Ö", 2);
            tiles.AddTile(" ", 2);

            return tiles;
        }

        public MoveEvaluation EvaluateMove(Move move)
        {
            if (move.Tiles.Count == 0)
                return MoveEvaluation.ImpossibleMove;

            if (!Hand.ContainsTiles(move.Tiles))
                return MoveEvaluation.ImpossibleMove;

            foreach(Tile tile in move.Tiles)
            {
                if (Board.Tiles[tile.X, tile.Y].Type == TileType.Letter)
                    return MoveEvaluation.ImpossibleMove;
            }

            List<List<Tile>> words = new List<List<Tile>>();

            Direction moveDirection = Direction.None;
            
            if(move.Tiles.Count > 1)
            {
                moveDirection = move.Tiles[0].X > move.Tiles[1].X ? Direction.Horizontal : Direction.Vertical;
            }

            foreach (Tile tile in move.Tiles)
            {
                words.Add(Board.ScanForTiles(move, tile, moveDirection == Direction.Horizontal ? Direction.Vertical : Direction.Horizontal));
            }

            words.Add(Board.ScanForTiles(move, move.Tiles[0], moveDirection));

            if(!words.Any(x => x.Count > 1))
                return MoveEvaluation.ImpossibleMove;

            List<int> pointsPerWord = new List<int>(); 
            UniqueTileCollection multiplyTiles = new UniqueTileCollection();

            foreach (List<Tile> word in words)
            {
                int wordPoints = 0;
                foreach (Tile tile in word)
                {
                    wordPoints += tile.PointValue * Board.GetPositionLetterMultiplier(tile.X, tile.Y);

                    if (Board.GetPositionWordMultiplier(tile.X, tile.Y) > 1)
                        multiplyTiles.AddTile(Board.Tiles[tile.X, tile.Y]);
                }

                foreach (Tile tile in multiplyTiles.Tiles)
                    wordPoints *= tile.NumericValue;

                pointsPerWord.Add(wordPoints);
                multiplyTiles.Clear();
            }

            return new MoveEvaluation(true, pointsPerWord.Sum());
        }
    }
}
