using ChatBot.Models.Data;
using ChatBot.Models.Prediction;
using Microsoft.ML;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatBot.Services
{
    public abstract class PredictionService
    {
        protected readonly MLContext _mlContext;
        protected PredictionEngine<PromptResponsePair, ConversationPrediction>? _predEngine;

        public MLContext MlContext { get { return _mlContext; } }
        private ITransformer? loadedModel;

        public PredictionService()
        {
            _mlContext = new MLContext(seed: 0);
        }

        public byte[] GetBytes()
        {
            if (_predEngine == null)
                throw new Exception("Can't get bytes of a prediction service that hasn't been loaded yet");

            using (MemoryStream memoryStream = new MemoryStream())
            {
                _mlContext.Model.Save(loadedModel, _predEngine.OutputSchema, memoryStream);
                return memoryStream.ToArray();
            }
        }

        public void LoadModel(byte[] bytes)
        {
            using (MemoryStream stream = new MemoryStream(bytes))
                loadedModel = _mlContext.Model.Load(stream, out var modelInputSchema);

            _predEngine = _mlContext.Model.CreatePredictionEngine<PromptResponsePair, ConversationPrediction>(loadedModel);
        }

        public abstract List<ConversationResponse> PredictResponse(PromptResponsePair conversation);
    }
}
