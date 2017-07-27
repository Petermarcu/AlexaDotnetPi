using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;

namespace Safern.Hub.Reader
{
    public class MessageReader
    {
        private readonly CancellationToken _ct;
        private readonly DeviceClient _deviceClient;
        private readonly HubConfiguration _configuration;

        public delegate void MessageReceivedHandler(object sender, MessageEventArgs e);

        public event MessageReceivedHandler OnMessageReceived;

        public MessageReader(HubConfiguration configuration, CancellationToken ct)
        {
            _configuration = configuration;
            _ct = ct;
            _deviceClient = DeviceClient.Create(_configuration.HubUri, new DeviceAuthenticationWithRegistrySymmetricKey(_configuration.DeviceId, _configuration.DeviceKey), TransportType.Mqtt);
        }

        public async void RunAsync()
        {
            await ReadMessagesAsync();
        }

        private async Task ReadMessagesAsync()
        {
            while (true)
            {
                if (_ct.IsCancellationRequested) break;
                var eventData = await _deviceClient.ReceiveAsync();

                if (eventData == null) continue;

                string data = Encoding.ASCII.GetString(eventData.GetBytes());
                var args = new MessageEventArgs(data);
                OnMessage(args);

                await _deviceClient.CompleteAsync(eventData);
            }
        }

        protected virtual void OnMessage(MessageEventArgs e)
        {
            MessageReceivedHandler handler = OnMessageReceived;
            if (handler != null)
            {
                handler(this, e);
            }
        }
    }
}