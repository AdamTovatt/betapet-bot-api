using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetapetBot
{
    public class LexiconSubset
    {
        public char Character { get; set; }
        public Dictionary<int, LexiconSubset> Subsets { get; set; }

        public LexiconSubset(char charater)
        {
            Character = charater;
        }
    }
}
