using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatBot.Services
{
    public interface IPredictionServiceRepository
    {
        public Task<PredictionService> GetPredictionServiceAsync(string name);
    }
}
