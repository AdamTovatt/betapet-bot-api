namespace ChatBot.Models.Data
{
    /// <summary>
    /// A class that will provide training data from a file, implements the ITrainingDataProvider interface
    /// </summary>
    public class FileTrainingDataProvider : ITrainingDataProvider
    {
        private string path;

        /// <summary>
        /// Will create a new provider with the specified path to read from
        /// </summary>
        /// <param name="path">The path to read from</param>
        public FileTrainingDataProvider(string path)
        {
            this.path = path;
        }

        /// <summary>
        /// Will return training data
        /// </summary>
        /// <returns>Training data</returns>
        public async Task<string> GetTrainingDataAsync()
        {
            return await File.ReadAllTextAsync(path);
        }
    }
}
