using Betapet.Models.InGame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace Betapet.Helpers
{
    public static class ExtensionMethods
    {
        public static List<Tile> ToListOfTiles(this string value)
        {
            List<Tile> result = new List<Tile>();

            foreach (char c in value)
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

        public static List<Tile> RemoveTile(this List<Tile> tiles, Tile tile)
        {
            int removeIndex = tiles.FindIndex(x => x.StringValue == tile.StringValue);

            if (removeIndex != -1)
            {
                tiles.RemoveAt(removeIndex);
            }

            return tiles;
        }

        public static List<Tile> RemoveTiles(this List<Tile> tiles, List<Tile> tilesToRemove)
        {
            foreach (Tile tile in tilesToRemove)
            {
                tiles.RemoveTile(tile);
            }

            return tiles;
        }
    }
}
