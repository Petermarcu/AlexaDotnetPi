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
using AlexaSkill.Models;

namespace PiClient
{
    class Program
    {
        static PIState piState;

        static void Main(string[] args)
        {
            //Instanciate device
            Device pi = new Device();
            piState = new PIState();

            //Flag for main loop
            bool isRunning = true;

            //Config
            HubConfiguration configuration = HubConfiguration.GetConfiguration("settings.json");
            if (configuration.NeedsDeviceSetup)
            {                
                RunDeviceSetup(ref configuration);
            }

            //Setup sender            
            MessageSender messageSender = new MessageSender(configuration);

            //Setup reader
            CancellationTokenSource cts = new CancellationTokenSource();
            MessageReader messageReader = new MessageReader(configuration, cts.Token);

            //Add delegate to stop
            System.Console.CancelKeyPress += (s, e) =>
            {
                isRunning = false;
                cts.Cancel();
                Console.WriteLine("Exiting.");
            };

            //Delegate to read incoming messages
            messageReader.OnMessageReceived += (s, e) =>
            {
                Message message = JsonConvert.DeserializeObject<Message>(e.Data);
                
                if(message.messageType==MessageType.REQUEST)
                {
                    //For SET
                    if(message.requestType==RequestType.SET)
                    {
                        Console.WriteLine($"{DateTime.Now} Request to SET to : {message.state} (current {piState.ledState})");
                        if(piState.ledState==message.state)
                        {
                            //send error state
                            Console.WriteLine($"{DateTime.Now} Sending Error state (already {message.state}");
                            message.messageType=MessageType.RESPONSE;
                            message.returnCode=1;
                            messageSender.SendMessageAsync(JsonConvert.SerializeObject(message));
                            return;
                        }

                        //Set state 
                        piState.ledState=message.state;
                        pi.SetLEDPin(piState.ledState);

                        //Now send response
                        SendLEDState(messageSender,piState);
                    }

                    //For GET
                    if(message.requestType==RequestType.GET)
                    {
                        Console.WriteLine($"{DateTime.Now} Request to GET current LED state");
                        SendLEDState(messageSender,piState);
                    }
                }
            };

            Console.WriteLine("Starting...");

            //Start reading
            messageReader.RunAsync();

            //Loop for motion sensor
            bool stateChanged=false;
            while (isRunning)
            {
                //Read pir and led state
                if(pi.ReadPirPin() != piState.pirState)
                {
                    piState.pirState = pi.ReadPirPin();
                    stateChanged=true;
                }

                //Super simple state machine to turn LED on or off
                if (piState.pirState && stateChanged)
                {
                    piState.ledState = true;
                    pi.SetLEDPin(piState.ledState);
                    Console.WriteLine($"{DateTime.Now} Motion Dected.  Turning LED on");
                }
                else if (!piState.pirState && stateChanged)
                {
                    piState.ledState = false;
                    pi.SetLEDPin(piState.ledState);
                    Console.WriteLine($"{DateTime.Now} Motion no longer detected.  Turning LED off");
                }                

                //Send response only if state changed
                if(stateChanged)
                {
                    stateChanged=false;
                    SendLEDState(messageSender,piState);
                }

                //Wait around for a bit
                Thread.Sleep(1000);
            }
        }

        private static void SendLEDState(MessageSender messageSender,PIState piState)
        {
            Console.WriteLine($"{DateTime.Now} Sending LED state {piState.ledState}");
            Message newMsg = new Message
            {
                messageType=MessageType.RESPONSE,
                state=piState.ledState,
                returnCode=0
            };
            messageSender.SendMessageAsync(JsonConvert.SerializeObject(newMsg));
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

    class PIState
    {
        public bool ledState {get; set;}
        public bool pirState {get; set;}
    }
}