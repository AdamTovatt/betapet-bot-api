using Newtonsoft.Json;
using Betapet;

namespace BetapetBotApi.FrontendModels
{
    public class Game
    {
        [JsonProperty("board")]
        public Board Board { get; set; }

        [JsonProperty("player")]
        public Player Player { get; set; }

        [JsonProperty("opponent")]
        public Player Opponent { get; set; }

        private int GetFrontendType(Betapet.Models.InGame.TileType type, Betapet.Models.InGame.Tile tile)
        {
            if (type == Betapet.Models.InGame.TileType.Letter)
                return 0;
            if(type == Betapet.Models.InGame.TileType.MultiplyWord && tile.NumericValue == 2)
                return 1;
            if (type == Betapet.Models.InGame.TileType.MultiplyWord && tile.NumericValue == 3)
                return 2;
            if (type == Betapet.Models.InGame.TileType.MultiplyLetter && tile.NumericValue == 2)
                return 3;
            if (type == Betapet.Models.InGame.TileType.MultiplyLetter && tile.NumericValue == 3)
                return 4;
            if (type == Betapet.Models.InGame.TileType.Start)
                return 6;

            return -1;
        }

        public Game(Betapet.Models.Game game)
        {
            Square[][] squares = new Square[15][];

            for (int x = 0; x < 15; x++)
            {
                squares[x] = new Square[15];
                for (int y = 0; y < 15; y++)
                {
                    Betapet.Models.InGame.Tile tile = game.Board.Tiles[x, y];

                    if (tile == null)
                        tile = game.OriginalBoard.Tiles[x, y];

                    if (tile != null)
                    {
                        squares[x][y] = new Square()
                        {
                            Letter = new Letter() { StringValue = tile.StringValue, ScoreValue = tile.PointValue },
                            Multiplier = tile.NumericValue,
                            MultiplyWord = tile.Type == Betapet.Models.InGame.TileType.MultiplyWord,
                            X = tile.X,
                            Y = tile.Y,
                            Type = GetFrontendType(tile.Type, tile),
                        };
                    }
                }
            }

            List<Letter> playerHand = new List<Letter>();
            foreach(var tile in game.Hand)
            {
                playerHand.Add(new Letter() { StringValue = tile.StringValue, ScoreValue = tile.PointValue });
            }

            Board = new Board()
            {
                Squares = squares,
                PlayerState = new PlayerState()
                {
                    Hand = playerHand,
                    HandCount = playerHand.Count(),
                    Score = game.UserList[0].Score,
                    UserId = game.UserList[0].Id,
                },
                OpponentState = new PlayerState()
                {
                    Hand = null,
                    HandCount = 0,
                    Score = game.UserList[1].Score,
                    UserId = game.UserList[1].Id,
                }
            };

            Player = new Player()
            {
                Id = game.UserList[0].Id,
                NameFirst = "Förnamn",
                NameLast = "Efternamn",
                Handle = "handle",
            };

            Opponent = new Player()
            {
                Id = game.UserList[1].Id,
                NameFirst = "Förnamn",
                NameLast = "Efternamn",
                Handle = "handle",
            };
        }

        public Game(Board board, Player player, Player opponent)
        {
            Board = board;
            Player = player;
            Opponent = opponent;
        }
    }
}
