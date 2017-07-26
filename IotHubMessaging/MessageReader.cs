using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.EventHubs;

namespace Safern.Hub.Reader
{
    public class MessageReader
    {
        private EventHubClient _eventHubClient;
        private readonly CancellationToken _ct;

        private readonly HubConfiguration _configuration;

        public delegate void MessageReceivedHandler(object sender, MessageEventArgs e);

        public event MessageReceivedHandler OnMessageReceived;

        public MessageReader(HubConfiguration configuration, CancellationToken ct)
        {
            _configuration = configuration;
            _ct = ct;
            Init();
        }

        private void Init()
        {

            var connection = new EventHubsConnectionStringBuilder(_configuration.ConnectionString)
            {
                EntityPath = _configuration.HubName,
                SasKey = _configuration.SasKey,
                SasKeyName = _configuration.SasKeyName
            };

            _eventHubClient = EventHubClient.CreateFromConnectionString(connection.ToString());
        }

        public async void RunAsync(string queue)
        {
            string messageFilter = $"\"queue\":\"{queue}\"";
            await Task.Run(() => { Run(messageFilter); });
        }

        private void Run(string messageFilter)
        {
            Console.WriteLine("Receiving Messages.");
            
            var partitions = _eventHubClient.GetRuntimeInformationAsync().GetAwaiter().GetResult().PartitionIds;

            var tasks = new List<Task>();
            foreach (string partition in partitions)
            {
                tasks.Add(ReadMessagesAsync(partition, messageFilter));
            }

            Task.WaitAll(tasks.ToArray());
        }

        private async Task ReadMessagesAsync(string partition, string messageFilter)
        {
            var eventHubReceiver = _eventHubClient.CreateReceiver("$Default", partition, DateTime.UtcNow);
            while (true)
            {
                if (_ct.IsCancellationRequested) break;
                var eventData = await eventHubReceiver.ReceiveAsync(10);

                if (eventData == null) continue;

                foreach (var item in eventData)
                {
                    string data = Encoding.UTF8.GetString(item.Body.Array);
                    var args = new MessageEventArgs(data);
                    if (data.Contains(messageFilter))
                    {
                        OnMessage(args);
                    }
                }
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