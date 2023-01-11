namespace BetapetBotApi.FrontendModels
{
    public class GameSummary
    {
        public int Completion { get; set; }
        public int OurScore { get; set; }
        public int TheirScore { get; set; }
        public Opponent Opponent { get; set; }
        public DateTime LastPlayTime { get; set; }
        public bool OurTurn { get; set; }
        public bool Active { get; set; }
        public int RatingChange { get; set; }

        public GameSummary(Betapet.Models.Game game, Betapet.BetapetManager betapet)
        {
            Completion = game.TilesPercent;

            Betapet.Models.User them = betapet.GetUserInfo(game.TheirUser.Id);

            Opponent = new Opponent()
            {
                CurrentAge = DateTime.Now.Year - them.Age,
                MatchesPlayed = them.Lost + them.Won + them.Dropped,
                Name = them.Handle,
                Rating = them.Rating
            };

            Betapet.Models.User us = betapet.GetUserInfo(betapet.UserId);

            OurScore = game.OurUser.Score;
            TheirScore = game.TheirUser.Score;
            LastPlayTime = game.ActivityTime;
            OurTurn = game.OurTurn;
            Active = !game.Finished;

            if (OurScore != 0 && TheirScore != 0)
                RatingChange = (int)Math.Round((((OurScore * OurScore) / (double)(OurScore * OurScore + TheirScore * TheirScore)) - (us.Rating / (double)(us.Rating + Opponent.Rating))) * us.Rating * (0.1 + 0.2 * Math.Pow(Math.E, -0.1 * (us.Won + us.Lost))), 0);
        }
    }
}
