using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TotteML;

namespace ChatNeuralNetworkTrainer
{
    public class TrainingSet
    {
        public double[] Input { get; set; }
        public Neuron[] Output { get; set; }
    }
}
