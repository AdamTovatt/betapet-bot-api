using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatBot.Services
{
    public class SdcaPredictionServiceRepository : IPredictionServiceRepository
    {
        public PredictionService GetPredictionServiceInstance()
        {
            return new SdcaPredictionService();
        }

        public PredictionTrainingService GetPredictionTrainingServiceInstance()
        {
            return new SdcaPredictionTrainingService();
        }
    }
}
