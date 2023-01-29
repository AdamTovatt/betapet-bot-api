using Betapet.Models.InGame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetapetBot
{
    public static class ExtensionMethods
    {
        private static Random random = new Random();

        public static List<List<Tile>> RemoveDuplicates(this List<List<Tile>> words)
        {
            HashSet<string> checkedWords = new HashSet<string>();
            List<List<Tile>> result = new List<List<Tile>>();

            foreach (List<Tile> word in words)
            {
                string tileString = word.GetAsString();
                if (!checkedWords.Contains(tileString))
                {
                    result.Add(word);
                    checkedWords.Add(tileString);
                }
            }

            return result;
        }

        /// <summary>
        /// Will take random tiles from a list of tiles, not taking the same tile twice. Can exclude wild cards to avoid extreme calculation times and errors when scoring moves
        /// </summary>
        /// <param name="tiles">The tiles to take tiles from</param>
        /// <param name="count">The amount of tiles to take</param>
        /// <param name="excludeWildCards">If wildcards should be excluded, honestly they always should</param>
        /// <returns></returns>
        public static List<Tile> TakeRandomTiles(this List<Tile> tiles, int count, bool excludeWildCards = true)
        {
            List<Tile> result = new List<Tile>();

            if (count >= tiles.Count)
                result.AddRange(tiles);
            else
            {
                for (int i = 0; i < count; i++)
                {
                    Tile tileToAdd = tiles[random.Next(tiles.Count)];

                    while (result.Contains(tileToAdd))
                    {
                        tileToAdd = tiles[random.Next(tiles.Count)];
                    }

                    if (excludeWildCards && tileToAdd.StringValue == " ")
                        continue;

                    result.Add(tileToAdd);
                }
            }

            return result;
        }

        public static string GetAsString(this List<Tile> tiles)
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (Tile tile in tiles)
                stringBuilder.Append(string.Format("{0}{1}{2}", tile.StringValue, tile.X, tile.Y));
            return stringBuilder.ToString();
        }
    }
}
