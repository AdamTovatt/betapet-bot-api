using System;
using System.Collections.Generic;
using System.Diagnostics.SymbolStore;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Betapet.Models.InGame
{
    public enum Direction
    {
        Horizontal, Vertical, None
    }

    public class Board
    {
        public Tile[,] Tiles { get; set; }
        public List<Tile> LetterTilesOnBoard { get; set; }
        public bool[,] TilesConnected { get; set; }

        /// <summary>
        /// Constructor for board, will create a board from a board data string
        /// </summary>
        /// <param name="boardData">A board data string, will be 15x15 and consist of numbers and letters</param>
        public Board(string boardData)
        {
            Tiles = new Tile[15, 15];
            TilesConnected = new bool[15, 15];
            LetterTilesOnBoard = new List<Tile>();

            if (boardData != null)
            {
                for (int y = 0; y < 15; y++)
                {
                    for (int x = 0; x < 15; x++)
                    {
                        Tile tile = Tile.FromCharacter(boardData[x + y * 15]);
                        tile.X = x;
                        tile.Y = y;

                        if (tile.Type == TileType.Letter)
                            LetterTilesOnBoard.Add(tile);

                        Tiles[x, y] = tile;
                    }
                }

                for (int y = 0; y < 15; y++)
                {
                    for (int x = 0; x < 15; x++)
                    {
                        TilesConnected[x, y] = GetPositionIsConneted(x, y);
                    }
                }
            }
        }

        public string ToBoardData()
        {
            StringBuilder boardData = new StringBuilder(new string('5', 225)); // board data is 15x15 long

            for (int y = 0; y < 15; y++)
            {
                for (int x = 0; x < 15; x++)
                {
                    boardData[x + y * 15] = Tiles[x, y].OriginalCharacter;
                }
            }

            return boardData.ToString();
        }

        public bool HasLetterAtPosition(int x, int y, string letter)
        {
            Tile tile = GetTileAtPosition(x, y);
            if (tile == null)
                return false;
            if (tile.Type != TileType.Letter)
                return false;
            if (tile.StringValue == letter)
                return true;
            return false;
        }

        /// <summary>
        /// Will return the letter multiplier for a certain position on the board
        /// </summary>
        /// <param name="x">Please use values between 0 and 14</param>
        /// <param name="y">Please use values between 0 and 14</param>
        /// <returns></returns>
        public int GetPositionLetterMultiplier(int x, int y)
        {
            try
            {
                Tile tile = Tiles[x, y];

                if (tile.Type != TileType.MultiplyLetter)
                    return 1;

                return tile.NumericValue;
            }
            catch
            {
                throw new Exception("Values for x and y need to be between 0 and 14! They are currently: x: " + x + " y: " + y);
            }
        }

        /// <summary>
        /// Will return the word multiplier for a certain position on the board
        /// </summary>
        /// <param name="x">Please use values between 0 and 14</param>
        /// <param name="y">Please use values between 0 and 14</param>
        /// <returns></returns>
        public int GetPositionWordMultiplier(int x, int y)
        {
            try
            {
                Tile tile = Tiles[x, y];

                if (tile.Type != TileType.MultiplyWord)
                    return 1;

                return tile.NumericValue;
            }
            catch
            {
                throw new Exception("Values for x and y need to be between 0 and 14! They are currently: x: " + x + " y: " + y);
            }
        }

        public bool GetHasConnectedTiles(Tile tile)
        {
            return TilesConnected[tile.X, tile.Y];
        }

        public bool GetHasConnectedTiles(int x, int y)
        {
            return TilesConnected[x, y];
        }

        private bool GetPositionIsConneted(int x, int y)
        {
            Tile offsetTile = GetTileAtPosition(x + 1, y);
            if (offsetTile != null && offsetTile.Type == TileType.Letter)
                return true;

            offsetTile = GetTileAtPosition(x - 1, y);
            if (offsetTile != null && offsetTile.Type == TileType.Letter)
                return true;

            offsetTile = GetTileAtPosition(x, y + 1);
            if (offsetTile != null && offsetTile.Type == TileType.Letter)
                return true;

            offsetTile = GetTileAtPosition(x, y - 1);
            if (offsetTile != null && offsetTile.Type == TileType.Letter)
                return true;

            return false;
        }

        public Tile GetTileAtPosition(int x, int y)
        {
            if (x > 14 || x < 0 || y > 14 || y < 0)
                return null;

            return Tiles[x, y];
        }

        public bool TileIsLetter(Tile tile)
        {
            if (tile == null)
                return false;
            if (tile.Type == TileType.Letter)
                return true;
            return false;
        }

        /// <summary>
        /// Will scan for tiles with a move
        /// </summary>
        /// <param name="move">The move to simulate</param>
        /// <param name="originTile">The tile to scan from</param>
        /// <param name="scanDirection">The direction to scan in</param>
        /// <returns>A list of tiles that were found to create a word from the origin tile</returns>
        /// <exception cref="Exception"></exception>
        public List<Tile> ScanForTiles(Move move, Tile originTile, Direction scanDirection)
        {
            List<Tile> result = new List<Tile>() { originTile };

            if (scanDirection == Direction.Horizontal)
            {
                int horizontalOffset = 1;

                while (originTile.X - horizontalOffset > 0 && GetTile(originTile.X - horizontalOffset, originTile.Y, move).Type == TileType.Letter) //check left
                {
                    result.Insert(0, cachedTile);
                    horizontalOffset++;
                }

                horizontalOffset = 1;

                while (originTile.X + horizontalOffset < 15 && GetTile(originTile.X + horizontalOffset, originTile.Y, move).Type == TileType.Letter) //check right
                {
                    result.Add(cachedTile);
                    horizontalOffset++;
                }

                return result;
            }
            else if (scanDirection == Direction.Vertical)
            {
                int verticalOffset = 1;

                while (originTile.Y - verticalOffset > 0 && GetTile(originTile.X, originTile.Y - verticalOffset, move).Type == TileType.Letter) //check up
                {
                    result.Insert(0, cachedTile);
                    verticalOffset++;
                }

                verticalOffset = 1;

                while (originTile.Y + verticalOffset > 0 && GetTile(originTile.X, originTile.Y + verticalOffset, move).Type == TileType.Letter) //check down
                {
                    result.Add(cachedTile);
                    verticalOffset++;
                }

                return result;
            }

            throw new Exception("Scan in both directions is not allowed");
        }

        Tile cachedTile;
        private Tile GetTile(int x, int y, Move move)
        {
            Tile moveTile = move.Tiles.Find(tile => tile.X == x && tile.Y == y);

            if (moveTile != null && moveTile.Type == TileType.Letter)
            {
                cachedTile = moveTile;
                return cachedTile;
            }

            cachedTile = GetTileAtPosition(x, y);
            if (cachedTile == null)
                cachedTile = new Tile(TileType.Empty, -1);
            return cachedTile;
        }
    }
}
