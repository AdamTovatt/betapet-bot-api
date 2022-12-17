using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Betapet.Models.InGame
{
    public class MoveEvaluation
    {
        public bool Possible { get; set; }
        public int Points { get; set; }

        public static MoveEvaluation ImpossibleMove { get { return new MoveEvaluation(false, 0); } }

        public MoveEvaluation(bool possible, int points)
        {
            Possible = possible;
            Points = points;
        }

    }
}
