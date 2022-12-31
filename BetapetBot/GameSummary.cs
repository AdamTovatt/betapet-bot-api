using Betapet;
using Betapet.Models;
using Betapet.Models.InGame;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetapetBot
{
    public class GameSummary
    {
        [JsonProperty("winning")]
        public bool Winning { get; set; }

        [JsonProperty("ourScore")]
        public int OurScore { get; set; }
        
        [JsonProperty("theirScore")]
        public int TheirScore { get; set; }

        [JsonProperty("matchCompletion")]
        public int MatchCompletion { get; set; }

        [JsonProperty("lastAction")]
        public string LastAction { get; set; }

        [JsonProperty("opponentName")] 
        public string OpponentName { get; set; }

        public GameSummary(Game game, BetapetManager betapet)
        {
            InGameUser us = game.UserList.Where(x => x.Id == betapet.UserId).FirstOrDefault();
            if (us != null)
                OurScore = us.Score;

            InGameUser them = game.UserList.Where(x => x.Id != betapet.UserId).FirstOrDefault();
            if (them != null)
                TheirScore = them.Score;

            if (OurScore != 0 && TheirScore != 0)
                Winning = OurScore > TheirScore;

            MatchCompletion = game.TilesPercent;

            User theirUser = betapet.GetUserInfo(them.Id);
            if (theirUser != null)
            {
                OpponentName = theirUser.Handle;
            }

            if(game.Finished)
            {
                LastAction = Winning ? "We won" : "We lost";
            }
            else if(!game.OurTurn)
            {
                LastAction = "Waiting for opponent to make a move";
            }
        }
    }
}
