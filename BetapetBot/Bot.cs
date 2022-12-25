using Betapet;
using Betapet.Helpers;
using Betapet.Models;
using Betapet.Models.Communication;
using Betapet.Models.Communication.Responses;
using Betapet.Models.InGame;
using Npgsql;
using System.Diagnostics;

namespace BetapetBot
{
    public class Bot
    {
        private BetapetManager betapet;
        private Lexicon lexicon;

        public Bot(string username, string password, string deviceId, string connectionString)
        {
            betapet = new BetapetManager(username, password, deviceId);
            lexicon = new Lexicon(connectionString);
        }

        public async Task<List<string>> HandleAllMatches()
        {
            List<string> result = new List<string>();

            RequestResponse loginResponse = await betapet.LoginAsync();
            RequestResponse requestResponse = await betapet.GetGameAndUserListAsync();

            GamesAndUserListResponse gameResponse = (GamesAndUserListResponse)requestResponse.InnerResponse;

            foreach (Game game in gameResponse.Games)
            {
                if (game.OurTurn && !game.Finished)
                {
                    List<WordLine> wordLines = GetWordLines(game);
                    List<Move> moves = await GenerateMovesFromWordLinesAsync(game, wordLines);
                    moves = await CheckForWildcards(game, moves);

                    bool performedMove = false;
                    foreach (Move move in moves)
                    {
                        RequestResponse playRequestResponse = await betapet.PlayMoveAsync(move, game);
                        if (playRequestResponse != null && playRequestResponse.Success)
                        {
                            result.Add(string.Format("Played \"{0}\" for {1} points", move.Tiles.ToTileString(), move.Evaluation.Points));
                            performedMove = true;
                            break;
                        }
                    }

                    if (!performedMove)
                    {
                        result.Add("Cannot find move");
                        /*
                        SwapTilesResponse swapResponse = (SwapTilesResponse)(await betapet.SwapTilesAsync(game, game.Hand)).InnerResponse;
                        if (swapResponse.SwapCount > 0)
                        {
                            result.Add("Swapped " + swapResponse.SwapCount.ToString() + " tiles");
                        }
                        else
                        {
                            result.Add("Tried to swap tiles but failed");
                        }
                        */
                    }
                }
                else
                {
                    result.Add("Not our turn in game: " + game.Id);
                }
            }

            return result;
        }

        public async Task<string> GetMessage()
        {
            RequestResponse message = await betapet.LoginAsync();
            RequestResponse response = await betapet.GetFriendsAsync();
            RequestResponse games = await betapet.GetGameAndUserListAsync();

            Game game = ((GamesAndUserListResponse)games.InnerResponse).Games[0];

            List<string> words = await lexicon.GetPossibleWordsAsync("AIhäRDIGPTRKRZ");

            List<WordLine> wordLines = GetWordLines(game);

            List<Move> moves = await GenerateMovesFromWordLinesAsync(game, wordLines);

            foreach (Move move in moves)
            {
                MoveEvaluation evaluation = await EvaluateMoveAsync(move, game, game.Hand);
                //RequestResponse playRequestResponse = await betapet.PlayMoveAsync(move, game);
                //if (playRequestResponse != null && playRequestResponse.Success)
                //    return string.Format("Played \"{0}\" for {1} points. Server response: {2}", move.Tiles.ToTileString(), move.Evaluation.Points, ((PlayMoveResponse)playRequestResponse.InnerResponse).Code);
            }

            //SendChatResponse chatResponse = (SendChatResponse)(await betapet.SendChatMessage(game.Id, "du är noob")).InnerResponse;
            RequestResponse getChatResponse = await betapet.GetChatMessagesAsync(game);

            //PlayMoveResponse playResponse = (PlayMoveResponse)(await betapet.PlayMoveAsync(move3, game)).InnerResponse;

            return string.Format("authkey: {0}, userid: {1}", ((LoginResponse)message.InnerResponse).AuthKey, ((LoginResponse)message.InnerResponse).UserId);
        }

        public async Task<List<string>> GetPossibleWords(string letters)
        {
            return await lexicon.GetPossibleWordsAsync(letters);
        }

        public async Task GetPossibleWords(List<string> wordOfLetters)
        {
            using (NpgsqlConnection connection = await lexicon.GetConnectionAsync())
            {
                foreach (string letters in wordOfLetters)
                {
                    await lexicon.GetPossibleWordsAsync(letters, connection);
                }
            }
        }

        public async Task<List<Move>> CheckForWildcards(Game game, List<Move> moves)
        {
            List<Move> result = new List<Move>();

            string tiles = game.Hand.ToTileString();
            foreach (Move move in moves)
            {
                Move checkResult = await CheckForWildCards(tiles, move);
                if (checkResult != null)
                    result.Add(checkResult);
            }

            return result;
        }

        private async Task<Move> CheckForWildCards(string tiles, Move move)
        {
            foreach (Tile tile in move.Tiles)
            {
                if (!tiles.Any(c => c == tile.StringValue[0]))
                {
                    tile.WildCard = true;
                    int indexOfWildCard = tiles.IndexOf('.');

                    if (indexOfWildCard == -1)
                        return null;

                    tiles = tiles.Remove(indexOfWildCard, 1);
                }
                else
                    tiles = tiles.Remove(tiles.IndexOf(tile.StringValue[0]), 1);
            }

            return move;
        }

        public async Task<List<Move>> GenerateMovesFromWordLinesAsync(Game game, List<WordLine> wordLines)
        {
            List<Move> moves = new List<Move>();
            int worstScore = 0;

            string hand = game.Hand.ToTileString();
            using (NpgsqlConnection connection = await lexicon.GetConnectionAsync())
            {
                foreach (WordLine wordLine in wordLines)
                {
                    List<string> possibleWords = await lexicon.GetPossibleWordsAsync(wordLine.Letters.AddTiles(game.Hand).ToTileString());

                    foreach (string candidateWord in possibleWords)
                    {
                        for (int startPositionOffset = 0; startPositionOffset < 15; startPositionOffset++)
                        {
                            int shiftedX = wordLine.StartPosition.X + (wordLine.Direction == Direction.Horizontal ? startPositionOffset : 0);
                            int shiftedY = wordLine.StartPosition.Y + (wordLine.Direction == Direction.Vertical ? startPositionOffset : 0);

                            Move move = CreateMoveFromPosition(new Position(shiftedX, shiftedY), candidateWord, wordLine, game.Board, hand, game.Turn == 0);
                            if (move != null && !(game.Turn == 0 && (move.Tiles[0].X != 7 || move.Tiles[0].Y != 7)))
                            {
                                MoveEvaluation evaluation = await EvaluateMoveAsync(move, game, wordLine.Letters, connection);

                                if (evaluation.Possible)
                                {
                                    move.Evaluation = evaluation;

                                    if (worstScore == 0)
                                    {
                                        moves.Add(move);
                                        worstScore = evaluation.Points;
                                    }
                                    else
                                    {
                                        for (int i = 0; i < moves.Count; i++)
                                        {
                                            if (moves[i].Evaluation.Points < evaluation.Points)
                                            {
                                                worstScore = moves[i].Evaluation.Points;
                                                moves.Insert(i, move);
                                                break;
                                            }
                                        }
                                    }

                                    if (moves.Count > 1000)
                                    {
                                        moves.RemoveRange(990, 9);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            moves = moves.OrderByDescending(move => move.Evaluation.Points).ToList();

            return moves;
        }

        private Move CreateMoveFromPosition(Position position, string word, WordLine wordLine, Board board, string hand, bool firstMove)
        {
            if ((wordLine.Direction == Direction.Horizontal ? position.X : position.Y) + word.Length > 15 || position.X < 0 || position.Y < 0)
                return null;

            List<char> missingHandLettes = null;
            for (int i = 0; i < word.Length; i++)
            {
                if (hand.Contains(word[i]))
                    hand.Remove(hand.IndexOf(word[i]), 1);
                else
                {
                    if (missingHandLettes == null)
                        missingHandLettes = new List<char>();

                    missingHandLettes.Add(word[i]);
                }
            }

            bool anyTileConnected = false;
            Move move = new Move();
            bool hasMissingHandLetters = missingHandLettes != null && missingHandLettes.Count > 0;

            for (int offset = 0; offset < word.Length; offset++)
            {
                int x = position.X + (wordLine.Direction == Direction.Horizontal ? offset : 0);
                int y = position.Y + (wordLine.Direction == Direction.Vertical ? offset : 0);

                if (!anyTileConnected && (board.TilesConnected[x, y] || firstMove))
                    anyTileConnected = true;

                if (hasMissingHandLetters && missingHandLettes.Contains(word[offset]))
                {
                    if (!wordLine.Letters.Any(tile => tile.StringValue == word[offset].ToString() && tile.X == x && tile.Y == y))
                        return null;
                }
                move.AddTile(word[offset].ToString(), x, y);
            }

            if (!anyTileConnected)
                return null;

            return move;
        }

        public List<WordLine> GetWordLines(Game game)
        {
            if (game.Turn == 0)
                return new List<WordLine>() { new WordLine(Direction.Horizontal, new Position(7, 7)), new WordLine(Direction.Vertical, new Position(7, 7)) };

            List<WordLine> wordLines = new List<WordLine>();

            List<Position> startPositionsX = new List<Position>();
            List<Position> startPositionsY = new List<Position>();
            bool preX = false;
            bool preY = false;

            Position lastAddedPositionX = null;
            Position lastAddedPositionY = null;

            for (int x = 0; x < 15; x++)
            {
                bool found = false;
                for (int y = 0; y < 15; y++)
                {
                    Tile currentTile = game.Board.Tiles[x, y];
                    if (currentTile.Type == TileType.Letter)
                    {
                        int startY = currentTile.Y - game.Hand.Count;
                        if (startY < 0)
                            startY = 0;

                        lastAddedPositionX = new Position(currentTile.X, startY);
                        startPositionsX.Add(lastAddedPositionX);

                        if (!preX)
                        {
                            startPositionsX.Add(new Position(currentTile.X - 1, startY));
                            preX = true;
                        }

                        found = true;
                        break;
                    }
                }

                if (!found && preX)
                {
                    if (lastAddedPositionX != null)
                        startPositionsX.Add(new Position(lastAddedPositionX.X + 1, lastAddedPositionX.Y));

                    break;
                }
            }

            foreach (Position position in startPositionsX)
            {
                wordLines.Add(new WordLine(Direction.Vertical, position));
            }

            for (int y = 0; y < 15; y++)
            {
                bool found = false;
                for (int x = 0; x < 15; x++)
                {
                    Tile currentTile = game.Board.Tiles[x, y];
                    if (currentTile.Type == TileType.Letter)
                    {
                        int startX = currentTile.X - game.Hand.Count;
                        if (startX < 0)
                            startX = 0;

                        lastAddedPositionY = new Position(startX, currentTile.Y);
                        startPositionsY.Add(lastAddedPositionY);

                        if (!preY)
                        {
                            startPositionsY.Add(new Position(startX, currentTile.Y - 1));
                            preY = true;
                        }

                        found = true;
                        break;
                    }
                }

                if (!found && preY)
                {
                    if (lastAddedPositionY != null)
                        startPositionsY.Add(new Position(lastAddedPositionY.X, lastAddedPositionY.Y + 1));

                    break;
                }
            }

            foreach (Position position in startPositionsY)
            {
                wordLines.Add(new WordLine(Direction.Horizontal, position));
            }

            foreach (WordLine wordLine in wordLines)
            {
                for (int i = 0; i < 15; i++)
                {
                    int positionToCheck = wordLine.Direction == Direction.Horizontal ? wordLine.StartPosition.X : wordLine.StartPosition.Y;
                    positionToCheck += i;
                    if (positionToCheck > 14)
                        break;

                    Tile currentTile;

                    if (wordLine.Direction == Direction.Horizontal)
                        currentTile = game.Board.GetTileAtPosition(positionToCheck, wordLine.StartPosition.Y);
                    else
                        currentTile = game.Board.GetTileAtPosition(wordLine.StartPosition.X, positionToCheck);

                    if (currentTile != null && currentTile.Type == TileType.Letter)
                        wordLine.Letters.Add(currentTile);
                }
            }

            return wordLines;
        }

        public async Task<MoveEvaluation> EvaluateMoveAsync(Move move, Game game, List<Tile> additionalTiles, NpgsqlConnection sqlConnection = null)
        {
            if (move.Tiles.Count == 0)
                return MoveEvaluation.ImpossibleMove;

            if (!game.Hand.AddTiles(additionalTiles).ContainsTiles(move.Tiles))
                return MoveEvaluation.ImpossibleMove;

            foreach (Tile tile in move.Tiles)
            {
                if (tile.X > 14 || tile.X < 0 || tile.Y > 14 || tile.Y < 0)
                    return MoveEvaluation.ImpossibleMove;

                Tile boardTile = game.Board.GetTileAtPosition(tile.X, tile.Y);
                if (boardTile.Type == TileType.Letter && boardTile.StringValue != tile.StringValue)
                    return MoveEvaluation.ImpossibleMove;
            }

            List<Tile> tilesToRemove = new List<Tile>();
            foreach (Tile tile in move.Tiles)
            {
                if (game.Board.TileIsLetter(game.Board.GetTileAtPosition(tile.X, tile.Y)))
                    tilesToRemove.Add(tile);
            }

            Direction moveDirection = Direction.None;

            if (move.Tiles.Count > 1)
            {
                moveDirection = move.Tiles[0].X > move.Tiles[1].X ? Direction.Horizontal : Direction.Vertical;
            }

            if (game.Turn != 0 && !move.IsConnected(game.Board))
                return MoveEvaluation.ImpossibleMove;

            List<List<Tile>> words = new List<List<Tile>>();

            foreach (Tile tile in move.Tiles)
            {
                if (moveDirection != Direction.None)
                    words.Add(game.Board.ScanForTiles(move, tile, moveDirection == Direction.Horizontal ? Direction.Vertical : Direction.Horizontal));
            }

            if (moveDirection != Direction.None)
                words.Add(game.Board.ScanForTiles(move, move.Tiles[0], moveDirection));

            if (!words.Any(x => x.Count > 1))
                return MoveEvaluation.ImpossibleMove;

            List<int> pointsPerWord = new List<int>();
            UniqueTileCollection multiplyTiles = new UniqueTileCollection();

            NpgsqlConnection connection = null;

            try
            {
                connection = sqlConnection ?? await lexicon.GetConnectionAsync();

                foreach (List<Tile> word in words)
                {
                    if (!await lexicon.GetWordExistsAsync(word.ToTileString(), connection))
                        return MoveEvaluation.ImpossibleMove;

                    int wordPoints = 0;
                    foreach (Tile tile in word)
                    {
                        wordPoints += tile.PointValue * game.Board.GetPositionLetterMultiplier(tile.X, tile.Y);

                        if (game.Board.GetPositionWordMultiplier(tile.X, tile.Y) > 1)
                            multiplyTiles.AddTile(game.Board.Tiles[tile.X, tile.Y]);
                    }

                    foreach (Tile tile in multiplyTiles.Tiles)
                        wordPoints *= tile.NumericValue;

                    pointsPerWord.Add(wordPoints);
                    multiplyTiles.Clear();
                }
            }
            catch
            {
                throw;
            }
            finally
            {
                if (sqlConnection == null && connection != null)
                {
                    await connection.CloseAsync();
                    await connection.DisposeAsync();
                }
            }

            return new MoveEvaluation(true, pointsPerWord.Sum()) { TilesFromBoard = tilesToRemove };
        }
    }
}