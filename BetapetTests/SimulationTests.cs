using Betapet.Models;
using Betapet.Models.InGame;
using BetapetBot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetapetTests
{
    [TestClass]
    public class SimulationTests
    {
        [TestMethod]
        public async Task TestMoveEvaluation1()
        {
            BotConfiguration configuration = BotConfiguration.Load();
            Bot bot = new Bot(configuration.Username, configuration.Password, configuration.DeviceId, configuration.ConnectionString);

            const string gameJson = "{\"gameid\":16328809,\"start_time\":\"2022-12-23T20:06:36\",\"board_type\":2,\"player_cnt\":2,\"wordlist\":1,\"activity\":3,\"activity_time\":\"2022-12-27T00:56:15\",\"active\":1,\"status\":1,\"board_data\":\"455L5555555I55S51DI5551555NOCK55UT5355535G45E2BOA553535SEG5D5r5515535BANER5TAX55355FÄ55N555S53554MÄRR35555S5535VE55ÖDSLA5AH3D5ÅT45KOK5555A5RYTA535MO5555SPAR5NU515T55V5ARG5HOJT5STUMI5455FÖL5T55E55vRÅ555L1555551545SEDEL55552554\",\"board_data_org\":\"455255555552554515555515555515554553555355455255355353553552555515535515555553553555355355555355454553555515535565535515555355454553555553553555355355555515535515555255355353553552554553555355455515555515555515455255555552554\",\"turn\":36,\"last_chat_time\":\"2022-12-26T22:37:59\",\"user_list\":[{\"userid\":748338,\"hand\":\"ZI\",\"score\":326,\"chat\":0,\"bingos\":0,\"hand_cnt\":2},{\"userid\":814453,\"hand\":null,\"score\":371,\"chat\":0,\"bingos\":0,\"hand_cnt\":4}],\"tiles_left\":0,\"tiles_percent\":94,\"fails\":0,\"swap_count\":0,\"words_first\":\"GEN (5p)\",\"words\":\"GEN (5p)\",\"bingo\":false,\"mark\":[\"57\",\"72\",\"87\"]}";
            Game game = Game.FromJson(gameJson);

            Assert.IsTrue(game.BoardData == game.Board.ToBoardData());
        }
    }
}
