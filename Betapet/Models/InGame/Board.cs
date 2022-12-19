using System;
using System.Collections.Generic;
using System.Diagnostics.SymbolStore;
using System.Linq;
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

        /// <summary>
        /// Constructor for board, will create a board from a board data string
        /// </summary>
        /// <param name="boardData">A board data string, will be 15x15 and consist of numbers and letters</param>
        public Board(string boardData)
        {
            Tiles = new Tile[15, 15];
            LetterTilesOnBoard = new List<Tile>();

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
            Tile offsetTile = GetTileAtPosition(tile.X + 1, tile.Y);
            if (offsetTile != null && offsetTile.Type == TileType.Letter)
                return true;

            offsetTile = GetTileAtPosition(tile.X - 1, tile.Y);
            if (offsetTile != null && offsetTile.Type == TileType.Letter)
                return true;

            offsetTile = GetTileAtPosition(tile.X, tile.Y + 1);
            if (offsetTile != null && offsetTile.Type == TileType.Letter)
                return true;

            offsetTile = GetTileAtPosition(tile.X, tile.Y - 1);
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

                while(originTile.Y - verticalOffset > 0 && GetTile(originTile.X, originTile.Y - verticalOffset, move).Type == TileType.Letter) //check up
                {
                    result.Insert(0, cachedTile);
                    verticalOffset++;
                }

                verticalOffset = 1;

                while(originTile.Y + verticalOffset > 0 && GetTile(originTile.X, originTile.Y + verticalOffset, move).Type == TileType.Letter) //check down
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

            cachedTile = Tiles[x, y];
            return cachedTile;
        }
    }
}
