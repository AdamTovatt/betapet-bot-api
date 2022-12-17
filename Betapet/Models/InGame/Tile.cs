using System;
using System.Collections.Generic;
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

        public static Tile FromCharacter(char character)
        {
            switch (character)
            {
                case '1':
                    return new Tile(TileType.MultiplyWord, 2);
                case '2':
                    return new Tile(TileType.MultiplyWord, 3);
                case '3':
                    return new Tile(TileType.MultiplyLetter, 2);
                case '4':
                    return new Tile(TileType.MultiplyLetter, 3);
                case '5':
                    return new Tile(TileType.Empty, 0);
                case '6':
                    return new Tile(TileType.Start, 0);
                default:
                    return new Tile(character.ToString());
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
    }
}
