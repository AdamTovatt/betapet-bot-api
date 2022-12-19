using Betapet.Models.InGame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetapetBot
{
    public class WordLine
    {
        public Position StartPosition { get; set; }
        public List<Tile> Letters { get; set; }
        public Direction Direction { get; set; }

        public override string ToString()
        {
            return string.Format("{0}, {1}", StartPosition.ToString(), Direction.ToString());
        }

        public WordLine(Direction direction, Position startPosition)
        {
            Direction = direction;
            StartPosition = startPosition;
            Letters = new List<Tile>();
        }
    }
}
