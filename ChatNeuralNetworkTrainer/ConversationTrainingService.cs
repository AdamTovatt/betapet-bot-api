using Microsoft.ML;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatNeuralNetworkTrainer
{
    public class ConversationTrainingService
    {
        public void Train(IEnumerable<Conversation> trainingData, string modelSavePath)
        {
            var mlContext = new MLContext(seed: 0);

            // Configure ML pipeline
            var pipeline = LoadDataProcessPipeline(mlContext);
            var trainingPipeline = GetTrainingPipeline(mlContext, pipeline);
            var trainingDataView = mlContext.Data.LoadFromEnumerable(trainingData);

            // Generate training model.
            var trainingModel = trainingPipeline.Fit(trainingDataView);

            // Save training model to disk.
            mlContext.Model.Save(trainingModel, trainingDataView.Schema, modelSavePath);
        }

        private IEstimator<ITransformer> LoadDataProcessPipeline(MLContext mlContext)
        {
            // Configure data pipeline based on the features in TransactionData.
            // Description and TransactionType are the inputs and Category is the expected result.
            var dataProcessPipeline = mlContext
                .Transforms.Conversion.MapValueToKey(inputColumnName: nameof(Conversation.Response), outputColumnName: "Label")
                .Append(mlContext.Transforms.Text.FeaturizeText(inputColumnName: nameof(Conversation.Promt), outputColumnName: "TitleFeaturized"))
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
    }
}
