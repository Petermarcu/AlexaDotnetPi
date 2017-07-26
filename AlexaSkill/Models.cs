using Newtonsoft.Json;

namespace AlexaSkill.Models
{
    public class Sensor
    {
        [JsonProperty("state")]
        public SensorState State { get; set; }

        public override string ToString()
        {
            return $"Right now, the sensor is {StringRepresentationOfState(State)}";
        }

        private string StringRepresentationOfState(SensorState state)
        {
            return (state == SensorState.On) ? "On" : "Off";
        }
    }

    public enum SensorState
    {
        On,
        Off
    }
}