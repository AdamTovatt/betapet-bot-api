using Betapet.Helpers;
using Betapet.Models;
using Betapet.Models.InGame;
using BetapetBot;

namespace BetapetTests
{
    [TestClass]
    public class MoveEvaluationTests
    {
        [TestMethod]
        public void TestMoveEvaluation1()
        {
            BotConfiguration configuration = BotConfiguration.Load();
            Bot bot = new Bot(configuration.Username, configuration.Password, configuration.DeviceId, configuration.ConnectionString);

            const string gameJson = "{\"gameid\":16336507,\"start_time\":\"2022-12-26T16:56:36\",\"board_type\":3,\"player_cnt\":2,\"wordlist\":2,\"activity\":3,\"activity_time\":\"2022-12-26T17:40:32\",\"active\":1,\"status\":1,\"board_data\":\"553525155555255555555555545555155355555532555555545555555535435555555555555255555555355255543555555255555555555565515554555555555555515425555555513514355545354555525555125551552543555555125515115555551555555555535555555314555\",\"board_data_org\":\"553525155555255555555555545555155355555532555555545555555535435555555555555255555555355255543555555255555555555565515554555555555555515425555555513514355545354555525555125551552543555555125515115555551555555555535555555314555\",\"turn\":1,\"last_chat_time\":\"2022-12-26T16:56:36\",\"user_list\":[{\"userid\":748338,\"hand\":\"ARÅYKSE\",\"score\":40,\"chat\":0,\"bingos\":0,\"hand_cnt\":7},{\"userid\":814356,\"hand\":null,\"score\":0,\"chat\":0,\"bingos\":0,\"hand_cnt\":7}],\"tiles_left\":82,\"tiles_percent\":4,\"fails\":0,\"swap_count\":0,\"words_first\":\"\",\"words\":\"\",\"bingo\":false,\"mark\":[\"112\",\"113\",\"114\",\"115\"]}";
            Game game = Game.FromJson(gameJson);      

            Move move = new Move();
            move.AddTile("R", 7, 7);
            move.AddTile("Y", 8, 7);
            move.AddTile("S", 9, 7);
            move.AddTile("A", 10, 7);

            MoveEvaluation evaluation = null;

            Assert.IsTrue(evaluation.Points == 40);
        }

        [TestMethod]
        public void TestMoveEvaluation2()
        {
            BotConfiguration configuration = BotConfiguration.Load();

            const string gameJson = "{\"gameid\":16336507,\"start_time\":\"2022-12-26T16:56:36\",\"board_type\":3,\"player_cnt\":2,\"wordlist\":2,\"activity\":3,\"activity_time\":\"2022-12-26T18:05:13\",\"active\":1,\"status\":1,\"board_data\":\"5535251555552555555555555455551553555555325555555455555555354355555555555552555555553552555435555552G55555555555RYSA55545555555555S55154255555555S35143555453545A5525555125551552543555555125515115555551555555555535555555314555\",\"board_data_org\":\"553525155555255555555555545555155355555532555555545555555535435555555555555255555555355255543555555255555555555565515554555555555555515425555555513514355545354555525555125551552543555555125515115555551555555555535555555314555\",\"turn\":3,\"last_chat_time\":\"2022-12-26T16:56:36\",\"user_list\":[{\"userid\":748338,\"hand\":\"ÅKOTLEI\",\"score\":101,\"chat\":0,\"bingos\":0,\"hand_cnt\":7},{\"userid\":814356,\"hand\":null,\"score\":12,\"chat\":0,\"bingos\":0,\"hand_cnt\":7}],\"tiles_left\":74,\"tiles_percent\":12,\"fails\":0,\"swap_count\":0,\"words_first\":\"ÅKTE (61p)\",\"words\":\"ÅKTE, GASSAT (54+7=61p)\",\"bingo\":false,\"mark\":[\"173\",\"174\",\"175\",\"176\",\"100\",\"115\",\"130\",\"145\",\"160\"]}";
            Game game = Game.FromJson(gameJson);

            Move move = new Move();
            move.AddTile("Å", 8, 11);
            move.AddTile("K", 9, 11);
            move.AddTile("T", 10, 11);
            move.AddTile("E", 11, 11);

            MoveEvaluation evaluation = null;

            Assert.IsTrue(evaluation.Points == 40);
        }
    }
}