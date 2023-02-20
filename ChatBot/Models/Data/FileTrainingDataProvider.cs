using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatBot.Models.Data
{
    public class FileTrainingDataProvider : ITrainingDataProvider
    {
        private string path;

        public FileTrainingDataProvider(string path)
        {
            this.path = path;
        }

        public async Task<string> GetTrainingDataAsync()
        {
            return await File.ReadAllTextAsync(path);
        }
    }
}
