namespace BetapetBotApi.FrontendModels
{
    public class GameSummary
    {
        public int Completion { get; set; }
        public int OurScore { get; set; }
        public int TheirScore { get; set; }
        public Opponent Opponent { get; set; }
        public DateTime LastPlayTime { get; set; }

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

            OurScore = game.OurUser.Score;
            TheirScore = game.TheirUser.Score;
            LastPlayTime = game.ActivityTime;
        }
    }
}
