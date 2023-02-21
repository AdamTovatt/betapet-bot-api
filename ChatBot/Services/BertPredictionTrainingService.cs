using ChatBot.Models.Data;
using Microsoft.ML;
using Microsoft.ML.TorchSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatBot.Services
{
    public class BertPredictionTrainingService : PredictionTrainingService
    {
        public override byte[] Train(IEnumerable<PromptResponsePair> trainingData)
        {
            MLContext mlContext = new MLContext(0);

            //Define your training pipeline
            var pipeline =
                    mlContext.Transforms.Conversion.MapValueToKey("Label", nameof(PromptResponsePair.Response))
                        .Append(mlContext.MulticlassClassification.Trainers.TextClassification(sentence1ColumnName: nameof(PromptResponsePair.Prompt)))
                        .Append(mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel"));

            var trainingDataView = mlContext.Data.LoadFromEnumerable(trainingData);

            // Train the model
            var model = pipeline.Fit(trainingDataView);

            using (MemoryStream memoryStream = new MemoryStream())
            {
                mlContext.Model.Save(model, trainingDataView.Schema, memoryStream);
                return memoryStream.ToArray();
            }
        }

        public override PredictionService CreateNewPredictionService()
        {
            return new BertPredictionService();
        }
    }
}
