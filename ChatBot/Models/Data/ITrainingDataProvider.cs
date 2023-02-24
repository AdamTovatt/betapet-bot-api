namespace ChatBot.Models.Data
{
    /// <summary>
    /// Interface for training data providers. Usefull for creating your own training data providers, maybe you have a database that you want to read the training data from? Then you shuold create a training data provider for that and implement this interface in that
    /// </summary>
    public interface ITrainingDataProvider
    {
        /// <summary>
        /// The method for getting training data
        /// </summary>
        /// <returns></returns>
        public Task<string> GetTrainingDataAsync();
    }
}
