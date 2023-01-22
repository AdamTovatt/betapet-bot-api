using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Betapet.Models.InGame
{
    public enum TileType
    {
        Empty, MultiplyLetter, MultiplyWord, Letter, Start
    }

    public class Tile
    {
        public TileType Type { get; set; }
        public int NumericValue { get; set; }
        public string StringValue { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public bool WildCard { get; set; }
        public bool IsFromWordLine { get; set; }
        public int PointValue { get { if (_pointValue == -1 && !string.IsNullOrEmpty(StringValue)) _pointValue = PointLookup.GetPointValue(StringValue); return _pointValue; } }
        private int _pointValue = -1;

        public char OriginalCharacter { get; private set; }

        public Tile(TileType type, int numericValue)
        {
            Type = type;
            NumericValue = numericValue;
        }

        public Tile(string letter)
        {
            Type = TileType.Letter;
            StringValue = letter;
        }

        public Tile(string letter, int x, int y)
        {
            X = x;
            Y = y;
            StringValue = letter;
            Type = TileType.Letter;
        }

        public static Tile FromCharacter(char character)
        {
            switch (character)
            {
                case '1':
                    return new Tile(TileType.MultiplyWord, 2) { OriginalCharacter = character };
                case '2':
                    return new Tile(TileType.MultiplyWord, 3) { OriginalCharacter = character };
                case '3':
                    return new Tile(TileType.MultiplyLetter, 2) { OriginalCharacter = character };
                case '4':
                    return new Tile(TileType.MultiplyLetter, 3) { OriginalCharacter = character };
                case '5':
                    return new Tile(TileType.Empty, 0) { OriginalCharacter = character };
                case '6':
                    return new Tile(TileType.Start, 0) { OriginalCharacter = character };
                default:
                    return new Tile(character.ToString()) { OriginalCharacter = character };
            }
        }

        public override string ToString()
        {
            if (Type == TileType.MultiplyLetter)
                return string.Format("Letter * {0}", NumericValue);
            if (Type == TileType.MultiplyWord)
                return string.Format("Word * {0}", NumericValue);
            if (Type == TileType.Empty)
                return "(Empty)";
            else
                return StringValue;
        }

        public string GetHash()
        {
            return string.Format("{0},{1}:{2}", X, Y, StringValue);
        }
    }

    public static class PointLookup
    {
        private static Dictionary<string, int> lookup = new Dictionary<string, int>()
        {
            { "A", 1 },
            { "B", 4 },
            { "C", 8 },
            { "D", 2 },
            { "E", 1 },
            { "F", 4 },
            { "G", 2 },
            { "H", 3 },
            { "I", 2 },
            { "J", 7 },
            { "K", 3 },
            { "L", 2 },
            { "M", 3 },
            { "N", 1 },
            { "O", 2 },
            { "P", 3 },
            { "R", 1 },
            { "S", 1 },
            { "T", 1 },
            { "U", 4 },
            { "V", 3 },
            { "X", 8 },
            { "Y", 7 },
            { "Z", 10 },
            { "Å", 4 },
            { "Ä", 4 },
            { "Ö", 4 },
            { " ", 0 },
        };      

        public static int GetPointValue(string letter)
        {
            if (!lookup.ContainsKey(letter))
                return 0;

            return lookup[letter];
        }
    }
}
