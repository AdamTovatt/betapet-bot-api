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
        public static List<List<Tile>> RemoveDuplicates(this List<List<Tile>> words)
        {
            HashSet<string> checkedWords = new HashSet<string>();
            List<List<Tile>> result = new List<List<Tile>>();

            foreach(List<Tile> word in words)
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

        public static string GetAsString(this List<Tile> tiles)
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (Tile tile in tiles)
                stringBuilder.Append(string.Format("{0}{1}{2}", tile.StringValue, tile.X, tile.Y));
            return stringBuilder.ToString();
        }
    }
}
