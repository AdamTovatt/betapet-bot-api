using Betapet.Helpers;
using Betapet.Models;
using Betapet.Models.InGame;

namespace BetapetTests
{
    [TestClass]
    public class MoveEvaluationTests
    {
        [TestMethod]
        public void TestMoveEvaluation1()
        {
            Game game = new Game()
            {
                BoardData = "4552555555525545155555155555155545535553554552553553535535525555155JULS55555535535U53553555553554S4H535555155355TOAN5515555355454D5V55555355355LEKA35555551553551R55525535535355A552554553555355455515555515555515455255555552554",
                OriginalBoardData = "455255555552554515555515555515554553555355455255355353553552555515535515555553553555355355555355454553555515535565535515555355454553555553553555355355555515535515555255355353553552554553555355455515555515555515455255555552554",
                UserList = new List<InGameUser>() { new InGameUser() { Hand = "TMÅELUF" } }
            };

            Move move1 = new Move();
            move1.AddTile("M", 6, 5);
            move1.AddTile("Å", 6, 6);

            MoveEvaluation evaluation1 = game.EvaluateMove(move1);

            Move move2 = new Move();
            move2.AddTile("E", 10, 8);
            move2.AddTile("L", 11, 8);

            MoveEvaluation evaluation2 = game.EvaluateMove(move2);

            Move move3 = new Move();
            move3.AddTile("Å", 10, 10);
            move3.AddTile("T", 10, 11);

            MoveEvaluation evaluation3 = game.EvaluateMove(move3);

            Move move4 = new Move();
            move4.AddTile("U", 10, 10);
            move4.AddTile("F", 10, 11);

            MoveEvaluation evaluation4 = game.EvaluateMove(move4);

            Assert.IsTrue(evaluation1.Points == 35 && evaluation2.Possible == false && evaluation3.Points == 28 && evaluation4.Points == 37);
        }
    }
}