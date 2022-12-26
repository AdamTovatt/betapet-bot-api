using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BetapetTests
{
    public class BotConfiguration
    {
        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("password")]
        public string Password { get; set; }

        [JsonProperty("connectionString")]
        public string ConnectionString { get; set; }

        [JsonProperty("deviceId")]
        public string DeviceId { get; set; }

        public static BotConfiguration Load()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            string resourceName = assembly.GetManifestResourceNames().Single(str => str.EndsWith("BotConfiguration.json"));

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                return JsonConvert.DeserializeObject<BotConfiguration>(reader.ReadToEnd());
            }
        }
    }
}
