using System;
using System.Text;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Safern.Hub.Sender
{
    public class MessageSender
    {
        private readonly DeviceClient _deviceClient;

        private readonly HubConfiguration _configuration;

        public MessageSender(HubConfiguration configuration)
        {
            _configuration = configuration;
            _deviceClient = DeviceClient.Create(_configuration.HubUri, new DeviceAuthenticationWithRegistrySymmetricKey(_configuration.DeviceId, _configuration.DeviceKey), TransportType.Mqtt);
        }

        public async void SendMessageAsync(string message, string queue)
        {
            JObject jsonObject = JObject.Parse(message);
            jsonObject.Add(new JProperty("queue", queue));
            var messageObject = new Message(Encoding.UTF8.GetBytes(jsonObject.ToString(Formatting.None)));
            
            await _deviceClient.SendEventAsync(messageObject);

            Console.WriteLine($"{DateTime.Now} > Sending message: {jsonObject.ToString(Formatting.None)}");
        }
    }
}
