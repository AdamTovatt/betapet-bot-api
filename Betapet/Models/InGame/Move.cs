using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Betapet.Models.InGame
{
    public class Move
    {
        public List<Tile> Tiles { get; set; }
        public MoveEvaluation Evaluation { get; set; }

        public Move()
        {
            Tiles = new List<Tile>();
        }

        public void AddTile(Tile tile)
        {
            Tiles.Add(tile);
        }

        public void AddTile(string letter, int x, int y)
        {
            Tiles.Add(new Tile(letter, x, y));
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();

            foreach(Tile tile in Tiles)
            {
                stringBuilder.Append(tile.StringValue);
                stringBuilder.Append(',');
                stringBuilder.Append(tile.X);
                stringBuilder.Append(',');
                stringBuilder.Append(tile.Y);
                stringBuilder.Append(',');
            }

            stringBuilder.Remove(stringBuilder.Length - 1, 1);

            return stringBuilder.ToString();
        }
    }
}
