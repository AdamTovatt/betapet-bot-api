namespace ChatBot.Models.Data
{
    public interface ITrainingDataProvider
    {
        public Task<TrainingData> GetTrainingDataAsync();
    }
}
