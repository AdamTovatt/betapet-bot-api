using ReadSecretsConsole;
using TotteML;

namespace ChatNeuralNetworkTrainer
{
    internal class Program
    {
        static void Main(string[] args)
        {
            NeuralNetwork neuralNetwork = new NeuralNetwork();

            SecretAppsettingReader secrets = new SecretAppsettingReader();
            SecretValues secretValues = secrets.ReadSection<SecretValues>("Configuration");
            Console.WriteLine(secretValues.DatabaseUrl);
            Console.ReadLine();
        }

        private NeuralNetwork CreateNetwork(int textLength, int possibleLettersCount, int hiddenLayers, double hiddenLayerScale, int possibleOutputs)
        {
            int[] size = new int[hiddenLayers + 2];
            size[0] = textLength * possibleLettersCount;

            for (int i = 1; i < hiddenLayers + 1; i++)
            {
                size[i] = (int)(size[0] * hiddenLayerScale);
            }

            size[hiddenLayers + 1] = possibleOutputs;

            return new NeuralNetwork(size, 0.5);
        }
    }
}