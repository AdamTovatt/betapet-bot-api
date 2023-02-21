using ChatBot.Models.Data;
using Microsoft.ML;

namespace ChatBot.Services
{
    public class SdcaPredictionTrainingService : PredictionTrainingService
    {
        public override byte[] Train(IEnumerable<PromptResponsePair> trainingData)
        {
            var mlContext = new MLContext(seed: 0);

            // Configure ML pipeline
            var pipeline = LoadDataProcessPipeline(mlContext);
            var trainingPipeline = GetTrainingPipeline(mlContext, pipeline);
            var trainingDataView = mlContext.Data.LoadFromEnumerable(trainingData);

            // Generate training model.
            var trainingModel = trainingPipeline.Fit(trainingDataView);

            // Save training model to disk.
            using (MemoryStream memoryStream = new MemoryStream())
            {
                mlContext.Model.Save(trainingModel, trainingDataView.Schema, memoryStream);
                return memoryStream.ToArray();
            }
        }

        private IEstimator<ITransformer> LoadDataProcessPipeline(MLContext mlContext)
        {
            // Configure data pipeline based on the features in TransactionData.
            // Description and TransactionType are the inputs and Category is the expected result.
            var dataProcessPipeline = mlContext
                .Transforms.Conversion.MapValueToKey(inputColumnName: nameof(PromptResponsePair.Response), outputColumnName: "Label")
                .Append(mlContext.Transforms.Text.FeaturizeText(inputColumnName: nameof(PromptResponsePair.Prompt), outputColumnName: "TitleFeaturized"))
                // Merge two features into a single feature.
                .Append(mlContext.Transforms.Concatenate("Features", "TitleFeaturized"))
                .AppendCacheCheckpoint(mlContext);

            return dataProcessPipeline;
        }

        private IEstimator<ITransformer> GetTrainingPipeline(MLContext mlContext, IEstimator<ITransformer> pipeline)
        {
            // Use the multi-class SDCA algorithm to predict the label using features.
            // For StochasticDualCoordinateAscent the KeyToValue needs to be PredictedLabel.
            return pipeline
                .Append(GetScadaTrainer(mlContext))
                .Append(mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel"));
        }

        private IEstimator<ITransformer> GetScadaTrainer(MLContext mlContext)
        {
            return mlContext.MulticlassClassification.Trainers.SdcaMaximumEntropy("Label", "Features");
        }

        public override PredictionService CreateNewPredictionService()
        {
            return new SdcaPredictionService();
        }
    }
}
