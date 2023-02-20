namespace ChatBot.Models.Data
{
    public interface ITrainingDataProvider
    {
        public Task<string> GetTrainingDataAsync();
    }
}
