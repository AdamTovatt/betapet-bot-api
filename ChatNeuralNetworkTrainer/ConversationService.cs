using Microsoft.ML;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatNeuralNetworkTrainer
{
    public class ConversationService
    {
        private readonly MLContext _mlContext;
        private PredictionEngine<Conversation, ConversationPrediction> _predEngine;

        public ConversationService()
        {
            _mlContext = new MLContext(seed: 0);
        }

        public void LoadModel(string modelPath)
        {
            ITransformer loadedModel;
            using (var stream = new FileStream(modelPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                loadedModel = _mlContext.Model.Load(stream, out var modelInputSchema);
            _predEngine = _mlContext.Model.CreatePredictionEngine<Conversation, ConversationPrediction>(loadedModel);
        }

        public string PredictResponse(Conversation transaction)
        {
            var prediction = new ConversationPrediction();
            _predEngine.Predict(transaction, ref prediction);
            return prediction?.Response;
        }
    }
}
