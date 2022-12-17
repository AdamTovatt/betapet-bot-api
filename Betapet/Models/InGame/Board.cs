using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Betapet.Models.InGame
{
    public class Board
    {
        Tile[,] tiles;

        public Board(string boardData)
        {
            tiles = new Tile[15, 15];

            for (int y = 0; y < 15; y++)
            {
                for (int x = 0; x < 15; x++)
                {
                    Tile tile = Tile.FromCharacter(boardData[x + y * 15]);
                    tile.X = x;
                    tile.Y = y;

                    tiles[x, y] = tile;
                }
            }
        }
    }
}
