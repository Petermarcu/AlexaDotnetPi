using System;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Safern.Hub;
using Safern.Hub.Reader;
using Safern.Hub.Sender;
using Pi.IO;
using Pi.IO.GeneralPurpose;
using Safern.Hub.Devices;

namespace PiClient
{
    class Program
    {
        static void Main(string[] args)
        {
            bool isRunning = true;

            HubConfiguration configuration = HubConfiguration.GetConfiguration("settings.json");

            if (configuration.NeedsDeviceSetup)
            {                
                RunDeviceSetup(ref configuration);
            }

            CancellationTokenSource cts = new CancellationTokenSource();

            MessageReader messageReader = new MessageReader(configuration, cts.Token);

            System.Console.CancelKeyPress += (s, e) =>
            {
                isRunning = false;
                cts.Cancel();
                Console.WriteLine("Exiting.");
            };

            messageReader.OnMessageReceived += (s, e) =>
            {
                var obj = JsonConvert.DeserializeObject<dynamic>(e.Data);
                Console.WriteLine();
                Console.WriteLine($"{DateTime.Now} > Message Received: ");
                Console.WriteLine($"     Message: {obj.message}");
                Console.WriteLine($"     Queue: {obj.queue}");
                Console.WriteLine();
            };

            messageReader.RunAsync("evenQueue");

            MessageSender messageSender = new MessageSender(configuration);

            //Instanciate device
            Device pi = new Device();
            bool ledState = false;

            //Loop 
            while (isRunning)
            {
                //Read pir and led state
                bool pirState = pi.ReadPirPin();

                //Super simple state machine
                if (pirState && !ledState)
                {
                    ledState = true;
                    pi.SetLEDPin(ledState);
                }
                else if (!pirState && ledState)
                {
                    ledState = false;
                    pi.SetLEDPin(ledState);
                }                

                Console.WriteLine("Sent Message");
                messageSender.SendMessageAsync($"LED state: {ledState}", "evenQueue");
                Thread.Sleep(1000);
            }
        }

        private static void RunDeviceSetup(ref HubConfiguration configuration)
        {
            DevicesManager deviceManager = new DevicesManager(configuration);
            
            Console.WriteLine("Hello to AlexaDotnetPi in order to setup your smart hub we need to setup your device name.");
            Console.WriteLine();
            Console.WriteLine("In order to do so, we need to set it to be your account id that got created for you when you linked the skill with your Alexa.");
            Console.WriteLine();
            Console.WriteLine("To get it please go to our-website.com, login and in the top page you'll see the account id. Copy that id and insert it here:\n");
            string deviceName = Console.ReadLine();

            if (string.IsNullOrEmpty(deviceName))
                throw new ArgumentNullException(nameof(deviceName), "The device name can't be empty");

            configuration.DeviceId = deviceName;
            configuration.DeviceKey = deviceManager.AddDeviceOrGetKeyAsync(deviceName).Result;
            configuration.NeedsDeviceSetup = false;
            configuration.SaveConfigToFile("settings.json");
        }
    }
}