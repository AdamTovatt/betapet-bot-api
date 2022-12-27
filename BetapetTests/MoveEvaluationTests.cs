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
        public async Task TestMoveEvaluation1()
        {
            BotConfiguration configuration = BotConfiguration.Load();
            Bot bot = new Bot(configuration.Username, configuration.Password, configuration.DeviceId, configuration.ConnectionString);

            const string gameJson = "{\"gameid\":16328809,\"start_time\":\"2022-12-23T20:06:36\",\"board_type\":2,\"player_cnt\":2,\"wordlist\":1,\"activity\":3,\"activity_time\":\"2022-12-27T00:56:15\",\"active\":1,\"status\":1,\"board_data\":\"455L5555555I55S51DI5551555NOCK55UT5355535G45E2BOA553535SEG5D5r5515535BANER5TAX55355FÄ55N555S53554MÄRR35555S5535VE55ÖDSLA5AH3D5ÅT45KOK5555A5RYTA535MO5555SPAR5NU515T55V5ARG5HOJT5STUMI5455FÖL5T55E55vRÅ555L1555551545SEDEL55552554\",\"board_data_org\":\"455255555552554515555515555515554553555355455255355353553552555515535515555553553555355355555355454553555515535565535515555355454553555553553555355355555515535515555255355353553552554553555355455515555515555515455255555552554\",\"turn\":36,\"last_chat_time\":\"2022-12-26T22:37:59\",\"user_list\":[{\"userid\":748338,\"hand\":\"ZI\",\"score\":326,\"chat\":0,\"bingos\":0,\"hand_cnt\":2},{\"userid\":814453,\"hand\":null,\"score\":371,\"chat\":0,\"bingos\":0,\"hand_cnt\":4}],\"tiles_left\":0,\"tiles_percent\":94,\"fails\":0,\"swap_count\":0,\"words_first\":\"GEN (5p)\",\"words\":\"GEN (5p)\",\"bingo\":false,\"mark\":[\"57\",\"72\",\"87\"]}";
            Game game = Game.FromJson(gameJson);      

            Move move = new Move();
            move.AddTile("T", 0, 5);
            move.AddTile("I", 0, 6);

            MoveEvaluation evaluation = await bot.EvaluateMoveAsync(move, game, new List<Tile>() { new Tile("T", 0, 5) });

            Assert.IsTrue(evaluation.Points == 6);
        }

        [TestMethod]
        public async Task TestMoveEvaluation2()
        {
            BotConfiguration configuration = BotConfiguration.Load();
            Bot bot = new Bot(configuration.Username, configuration.Password, configuration.DeviceId, configuration.ConnectionString);

            const string gameJson = "{\"gameid\":16336507,\"start_time\":\"2022-12-26T16:56:36\",\"board_type\":3,\"player_cnt\":2,\"wordlist\":2,\"activity\":3,\"activity_time\":\"2022-12-26T18:05:13\",\"active\":1,\"status\":1,\"board_data\":\"5535251555552555555555555455551553555555325555555455555555354355555555555552555555553552555435555552G55555555555RYSA55545555555555S55154255555555S35143555453545A5525555125551552543555555125515115555551555555555535555555314555\",\"board_data_org\":\"553525155555255555555555545555155355555532555555545555555535435555555555555255555555355255543555555255555555555565515554555555555555515425555555513514355545354555525555125551552543555555125515115555551555555555535555555314555\",\"turn\":2,\"last_chat_time\":\"2022-12-26T16:56:36\",\"user_list\":[{\"userid\":748338,\"hand\":\"ÅKOTLEI\",\"score\":101,\"chat\":0,\"bingos\":0,\"hand_cnt\":7},{\"userid\":814356,\"hand\":null,\"score\":12,\"chat\":0,\"bingos\":0,\"hand_cnt\":7}],\"tiles_left\":74,\"tiles_percent\":12,\"fails\":0,\"swap_count\":0,\"words_first\":\"ÅKTE (61p)\",\"words\":\"ÅKTE, GASSAT (54+7=61p)\",\"bingo\":false,\"mark\":[\"173\",\"174\",\"175\",\"176\",\"100\",\"115\",\"130\",\"145\",\"160\"]}";
            Game game = Game.FromJson(gameJson);

            Move move = new Move();
            move.AddTile("Å", 8, 11);
            move.AddTile("K", 9, 11);
            move.AddTile("T", 10, 11);
            move.AddTile("E", 11, 11);

            MoveEvaluation evaluation = await bot.EvaluateMoveAsync(move, game, new List<Tile>()); //list is empty since this is the first move, no additional characters exist

            Assert.IsTrue(evaluation.Points == 40);
        }
    }
}