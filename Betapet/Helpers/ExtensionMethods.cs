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
        private static Random random = new Random();

        /// <summary>
        /// Will convert a string to a list of tiles by taking all the characters and converting them separately to a tile each
        /// </summary>
        /// <param name="value"></param>
        /// <returns>A list of tiles</returns>
        public static List<Tile> ToListOfTiles(this string value)
        {
            List<Tile> result = new List<Tile>();

            foreach (char c in value)
            {
                result.Add(Tile.FromCharacter(c));
            }

            return result;
        }

        /// <summary>
        /// Will convert a list of tiles to a string with the tile letters
        /// </summary>
        /// <param name="tiles"></param>
        /// <returns>A tile string that could look like follows: LETTERS</returns>
        public static string ToTileString(this List<Tile> tiles)
        {
            StringBuilder tileString = new StringBuilder();

            foreach (Tile tile in tiles)
            {
                if (tile.StringValue != " ")
                    tileString.Append(tile.StringValue);
            }

            return tileString.ToString();
        }

        /// <summary>
        /// Will take an amount of random tiles from a list of tiles. Will not take the same tile twice
        /// </summary>
        /// <param name="tiles">The list of tiles to take the random tiles from</param>
        /// <param name="amountToTake">The amount of tiles to take</param>
        /// <returns></returns>
        public static List<Tile> TakeRandomTiles(this List<Tile> tiles, int amountToTake)
        {
            List<Tile> result = new List<Tile>();

            if (amountToTake >= tiles.Count)
            {
                result.AddRange(tiles);
            }
            else
            {
                int tilesTaken = 0;
                HashSet<Tile> takenTiles = new HashSet<Tile>();

                while (tilesTaken < amountToTake)
                {
                    Tile tileToTake = tiles[random.Next(tiles.Count)];

                    while (takenTiles.Contains(tileToTake))
                        tileToTake = tiles[random.Next(tiles.Count)];

                    result.Add(tileToTake);
                    takenTiles.Add(tileToTake);

                    tilesTaken++;
                }
            }

            return result;
        }

        /// <summary>
        /// Will add a tile to a list of tiles, modifying the list in the process
        /// </summary>
        /// <param name="tiles"></param>
        /// <param name="tile">The tile to add. Will add a copy of this tile! Will not add the actual reference to the tile provided but will instead copy it</param>
        /// <param name="count">The amount of tiles of the specified tile to add</param>
        public static void AddTile(this List<Tile> tiles, Tile tile, int count)
        {
            for (int i = 0; i < count; i++)
            {
                tiles.Add(new Tile(tile.StringValue));
            }
        }

        /// <summary>
        /// Will add a tile to a list of tiles, modifying the list in the process
        /// </summary>
        /// <param name="tiles"></param>
        /// <param name="tile">The tile to add</param>
        /// <param name="count">The amount of tiles of the specified tile to add</param>
        public static void AddTile(this List<Tile> tiles, string tile, int count)
        {
            for (int i = 0; i < count; i++)
            {
                tiles.Add(new Tile(tile));
            }
        }

        /// <summary>
        /// Adds a list of tiles to another list of tiles. Will return a completely new list of the resulting tiles, leaving both original lists untouched.
        /// </summary>
        /// <param name="tiles">The list of tiles to add to, will be unchanged</param>
        /// <param name="tilesToAdd">The list of tiles to add, will be unchanged</param>
        /// <returns>A new list of tiles</returns>
        public static List<Tile> AddTiles(this List<Tile> tiles, List<Tile> tilesToAdd)
        {
            List<Tile> result = new List<Tile>();

            foreach (Tile tile in tiles)
                result.AddTile(tile, 1);

            foreach (Tile tile in tilesToAdd)
                result.AddTile(tile, 1);

            return result;
        }

        /// <summary>
        /// Will remove a tile from a list, modifying the list in the process. Uses the string value for equality comparison
        /// </summary>
        /// <param name="tiles"></param>
        /// <param name="tile"></param>
        /// <returns>A modified list without the specified tile</returns>
        public static List<Tile> RemoveTile(this List<Tile> tiles, Tile tile)
        {
            int removeIndex = tiles.FindIndex(x => x.StringValue == tile.StringValue);

            if (removeIndex != -1)
            {
                tiles.RemoveAt(removeIndex);
            }

            return tiles;
        }

        /// <summary>
        /// Will remove a list of tiles from another list of tiles, modifying the original list in the process
        /// </summary>
        /// <param name="tiles"></param>
        /// <param name="tilesToRemove">The tiles to remove</param>
        /// <returns>A modified list</returns>
        public static List<Tile> RemoveTiles(this List<Tile> tiles, List<Tile> tilesToRemove)
        {
            foreach (Tile tile in tilesToRemove)
            {
                tiles.RemoveTile(tile);
            }

            return tiles;
        }

        /// <summary>
        /// Returns wether or not a list of tiles contains all the tiles in another list of tiles
        /// </summary>
        /// <param name="tiles">The list to use as base</param>
        /// <param name="tilesToCheckFor">The list of tiles to check for</param>
        /// <returns>If the base list contains all tiles needed to create the list to check for</returns>
        public static bool ContainsTiles(this List<Tile> tiles, List<Tile> tilesToCheckFor)
        {
            Dictionary<string, int> letterCount = new Dictionary<string, int>();

            foreach (Tile tile in tilesToCheckFor)
            {
                if (!letterCount.ContainsKey(tile.StringValue.ToUpper()))
                    letterCount.Add(tile.StringValue.ToUpper(), 1);
                else
                    letterCount[tile.StringValue.ToUpper()] += 1;
            }

            foreach (Tile tile in tiles)
            {
                if (letterCount.ContainsKey(tile.StringValue.ToUpper()))
                    letterCount[tile.StringValue.ToUpper()] -= 1;

                if (letterCount.TryGetValue(tile.StringValue.ToUpper(), out int value) && value == 0)
                {
                    letterCount.Remove(tile.StringValue.ToUpper());
                    if (letterCount.Values.All(x => x == 0))
                    {
                        return true;
                    }
                }
            }

            if (letterCount.Values.All(x => x == 0))
                return true;

            int missingValues = 0;

            foreach (string key in letterCount.Keys)
            {
                missingValues += letterCount[key];
            }

            if (missingValues > 2)
                return false;

            if (tiles.Count(x => x.StringValue == ".") >= missingValues)
                return true;

            return false;
        }
    }
}
