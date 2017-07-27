using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Safern.Hub
{
    public class HubConfiguration
    {
        [JsonProperty("HubName")]
        public string HubName { get; set; }

        [JsonProperty("HubEndPoint")]
        public string HubEndPoint { get; set; }

        [JsonProperty("ConnectionString")]
        public string ConnectionString { get; set; }

        [JsonProperty("SasKey")]        
        public string SasKey { get; set; }

        [JsonProperty("SasKeyName")]   
        public string SasKeyName { get; set; }

        [JsonProperty("DeviceId")]
        public string DeviceId { get; set; }

        [JsonProperty("DeviceKey")]
        public string DeviceKey { get; set; }

        [JsonProperty("HubUri")]        
        public string HubUri { get; set; }

        [JsonProperty("NeedsDeviceSetup")]
        public bool NeedsDeviceSetup { get; set; }

        public void SaveConfigToFile(string file)
        {
            if (string.IsNullOrEmpty(file))
            {
                throw new ArgumentNullException(nameof(file), "Argument can't be null or empty");
            }

            File.WriteAllText(file, JsonConvert.SerializeObject(this, Formatting.Indented));
        }

        public static HubConfiguration GetConfiguration(string settingsFile)
        {
            if (string.IsNullOrEmpty(settingsFile))
            {
                throw new ArgumentNullException(nameof(settingsFile), "Argument can't be null or empty");
            }

            if (!File.Exists(settingsFile))
            {
                throw new FileNotFoundException($"Settings file {settingsFile} was not found.");
            }

            string text = File.ReadAllText(settingsFile);
            return JsonConvert.DeserializeObject<HubConfiguration>(text);
        }
    }
}