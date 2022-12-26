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

        public bool IsConnected(Board board)
        {
            int tilesLength = Tiles.Count;
            for (int i = 0; i < tilesLength; i++)
            {
                if (board.GetHasConnectedTiles(Tiles[tilesLength - i - 1]))
                    return true;
            }
            return false;
        }

        public void AddTile(Tile tile)
        {
            Tiles.Add(tile);
        }

        public void AddTile(string letter, int x, int y)
        {
            Tiles.Add(new Tile(letter, x, y));
        }

        public void AddTile(string letter, int x, int y, bool isFromWordLine)
        {
            Tiles.Add(new Tile(letter, x, y) { IsFromWordLine = isFromWordLine });
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();

            if(Evaluation != null)
                stringBuilder.Append(Evaluation.ToString() + " ");

            foreach(Tile tile in Tiles)
            {
                stringBuilder.Append(tile.WildCard ? tile.StringValue.ToLower() : tile.StringValue);
                stringBuilder.Append(',');
                stringBuilder.Append(tile.X);
                stringBuilder.Append(',');
                stringBuilder.Append(tile.Y);
                stringBuilder.Append(',');
            }

            stringBuilder.Remove(stringBuilder.Length - 1, 1);

            return stringBuilder.ToString();
        }

        public string ToMoveString()
        {
            StringBuilder stringBuilder = new StringBuilder();

            foreach (Tile tile in Tiles)
            {
                if (tile.IsFromWordLine)
                    continue;

                stringBuilder.Append(tile.WildCard ? tile.StringValue.ToLower() : tile.StringValue);
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
