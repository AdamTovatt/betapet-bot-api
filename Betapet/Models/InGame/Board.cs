using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Betapet.Models.InGame
{
    public class Board
    {
        public Tile[,] Tiles { get; set; }
        public List<Tile> LetterTilesOnBoard { get; set; }

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

                    if(tile.Type == TileType.Letter)
                        LetterTilesOnBoard.Add(tile);

                    Tiles[x, y] = tile;
                }
            }
        }
    }
}
