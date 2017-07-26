using System;
using System.Threading;
using Pi.IO;
using Pi.IO.GeneralPurpose;
using Pi.IO.Components.Converters.Mcp3008;

namespace PiClient
{
    public class Device
    {
        //Pin constants
        const ConnectorPin pir = ConnectorPin.P1Pin31;
        const ConnectorPin led = ConnectorPin.P1Pin29;

        const ConnectorPin adcClock = ConnectorPin.P1Pin23;
        const ConnectorPin adcMiso = ConnectorPin.P1Pin21;
        const ConnectorPin adcMosi = ConnectorPin.P1Pin19;
        const ConnectorPin adcCs = ConnectorPin.P1Pin24;

        //Driver
        GpioConnectionDriver driver;
        Mcp3008SpiConnection adcConnection;

        //Pins
        GpioInputBinaryPin pirPin;
        GpioOutputBinaryPin ledPin;

        public Device()
        {
            //gpio
            driver = new GpioConnectionDriver();

            //adc pin
            adcConnection = new Mcp3008SpiConnection(
                driver.Out(adcClock),
                driver.Out(adcCs),
                driver.In(adcMiso),
                driver.Out(adcMosi));

            //sensor
            pirPin = driver.In(pir);

            //ledpin
            ledPin = driver.Out(led);

            //Set LED off to start
            bool ledState = false;
            ledPin.Write(ledState);
        }

        public bool ReadPirPin()
        {
            var pirState = pirPin.Read();
            //Console.WriteLine("Pir Pin Value: " + pirState.ToString());
            return pirState;
        }

        public void SetLEDPin(bool ledState)
        {
            ledPin.Write(ledState);
            Console.WriteLine("Setting LED Pin Value: " + ledState.ToString());            
        }

        public AnalogValue ReadADCA0()
        {
            // Read the analog input
            AnalogValue a0 = adcConnection.Read(Mcp3008Channel.Channel0);
            Console.WriteLine("ADC Value 0: " + a0.Value);
            return a0;
        }

        public AnalogValue ReadADCA1()
        {
            // Read the analog input
            AnalogValue a1 = adcConnection.Read(Mcp3008Channel.Channel1);
            Console.WriteLine("ADC Value 1: " + a1.Value);
            return a1;
        }

        public AnalogValue ReadADCA2()
        {
            // Read the analog input
            AnalogValue a2 = adcConnection.Read(Mcp3008Channel.Channel2);
            Console.WriteLine("ADC Value 2: " + a2.Value);
            return a2;
        }           
    }
}
