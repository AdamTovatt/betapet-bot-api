using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Betapet.Models.InGame
{
    public class Move
    {
        public int GameId { get; set; }
        public int Turn { get; set; }
        public List<Tile> Tiles { get; set; }
        
        public Move(int gameId, int turn)
        {
            GameId = gameId;
            Turn = turn;
            Tiles = new List<Tile>();
        }

        public void AddTile(Tile tile)
        {
            Tiles.Add(tile);
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
