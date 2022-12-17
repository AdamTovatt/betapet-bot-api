using Betapet.Models.InGame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Betapet.Helpers
{
    public static class ExtensionMethods
    {
        public static List<Tile> ToListOfTiles(this string value)
        {
            List<Tile> result = new List<Tile>();

            foreach(char c in value)
            {
                result.Add(Tile.FromCharacter(c));
            }

            return result;
        }

        public static string ToTileString(this List<Tile> tiles)
        {
            StringBuilder tileString = new StringBuilder();

            foreach (Tile tile in tiles)
            {
                tileString.Append(tile.StringValue);
            }

            return tileString.ToString();
        }

        public static void AddTile(this List<Tile> tiles, Tile tile, int count)
        {
            for (int i = 0; i < count; i++)
            {
                tiles.Add(new Tile(tile.StringValue));
            }
        }

        public static void AddTile(this List<Tile> tiles, string tile, int count)
        {
            for (int i = 0; i < count; i++)
            {
                tiles.Add(new Tile(tile));
            }
        }
    }
}
