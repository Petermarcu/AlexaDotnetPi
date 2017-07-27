using System;
using System.Text;
using Microsoft.Azure.Devices;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Safern.Hub.Sender
{
    public class MessageSender
    {
        private readonly ServiceClient _serviceClient; 

        private readonly HubConfiguration _configuration;

        public MessageSender(HubConfiguration configuration)
        {
            _configuration = configuration;
            _serviceClient = ServiceClient.CreateFromConnectionString(GetConnectionString());
        }

        public async void SendMessageAsync(string message)
        {
            var messageObject = new Message(Encoding.UTF8.GetBytes(message));
            
            await _serviceClient.SendAsync(_configuration.DeviceId, messageObject);

            Console.WriteLine($"{DateTime.Now} > Sending message: {message}");
        }

        private string GetConnectionString()
        {
            return $"HostName={_configuration.HubUri};SharedAccessKeyName={_configuration.SasKeyName};SharedAccessKey={_configuration.SasKey}";
        }
    }
}
