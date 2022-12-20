using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Betapet.Models.InGame
{
    public class MoveEvaluation
    {
        private static MoveEvaluation impossibleMove = new MoveEvaluation(false, 0);

        public bool Possible { get; set; }
        public int Points { get; set; }

        public static MoveEvaluation ImpossibleMove { get { return impossibleMove; } }

        public MoveEvaluation(bool possible, int points)
        {
            Possible = possible;
            Points = points;
        }

        public override string ToString()
        {
            if (!Possible)
                return "Impossible move";
            else
                return "Would yield: " + Points.ToString();
        }
    }
}
