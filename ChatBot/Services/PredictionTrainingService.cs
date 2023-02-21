using ChatBot.Models.Data;

namespace ChatBot.Services
{
    public abstract class PredictionTrainingService
    {
        public abstract byte[] Train(IEnumerable<PromptResponsePair> trainingData);

        public abstract PredictionService CreateNewPredictionService();
    }
}
