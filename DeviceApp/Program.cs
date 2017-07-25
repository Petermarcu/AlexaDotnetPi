using System;
using System.Threading;
using Pi.IO;
using Pi.IO.GeneralPurpose;
using Pi.IO.Components.Converters.Mcp3008;

namespace DeviceApp
{
    class Program
    {
        static void Main(string[] args)
        {
            const ConnectorPin pir = ConnectorPin.P1Pin31;
            const ConnectorPin led = ConnectorPin.P1Pin29;

            var driver = new GpioConnectionDriver();

            const ConnectorPin adcClock = ConnectorPin.P1Pin23;
            const ConnectorPin adcMiso = ConnectorPin.P1Pin21;
            const ConnectorPin adcMosi = ConnectorPin.P1Pin19;
            const ConnectorPin adcCs = ConnectorPin.P1Pin24;

            var adcConnection = new Mcp3008SpiConnection(
                driver.Out(adcClock),
                driver.Out(adcCs),
                driver.In(adcMiso),
                driver.Out(adcMosi));

            var pirPin = driver.In(pir);
            var ledPin = driver.Out(led);

            bool ledState = false;
            ledPin.Write(ledState);

            while (true)
            {
                Thread.Sleep(500);

                // Read the analog inputs
                AnalogValue a0 = adcConnection.Read(Mcp3008Channel.Channel0);
                AnalogValue a1 = adcConnection.Read(Mcp3008Channel.Channel1);
                AnalogValue a2 = adcConnection.Read(Mcp3008Channel.Channel2);

                Console.WriteLine("Value 1: " + a0.Value);
                Console.WriteLine("Value 2: " + a1.Value);
                Console.WriteLine("Value 3: " + a2.Value);

                // Read the pir sensor
                var pirState = pirPin.Read();
                Console.WriteLine("Pir Pin Value: " + pirState.ToString());
                if (pirState && !ledState)
                {
                    ledState = true;
                    ledPin.Write(ledState);
                }
                else if (!pirState && ledState)
                {
                    ledState = false;
                    ledPin.Write(ledState);
                }
            }
        }
    }
}
