using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Betapet.Models.InGame
{
    public enum TileType
    {
        Empty, MultiplyLetter, MultiplyWord, Letter
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
        }
    }
}
