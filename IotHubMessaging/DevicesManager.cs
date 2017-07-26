using System;
using System.Threading.Tasks;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Common.Exceptions;

namespace Safern.Hub.Devices
{
    public class DevicesManager
    {
        private readonly HubConfiguration _configuration;
        private readonly RegistryManager _registryManager;

        public DevicesManager(HubConfiguration configuration)
        {
            _configuration = configuration;
            _registryManager = RegistryManager.CreateFromConnectionString(GetConnectionString());
        }

        public async Task<string> AddDeviceAsync(string deviceName)
        {
            try
            {
                var device = await _registryManager.AddDeviceAsync(new Device(deviceName));
                return device.Authentication.SymmetricKey.PrimaryKey;
            }
            catch (DeviceAlreadyExistsException)
            {
                throw new ArgumentException($"The device: {deviceName} already exists, if you want to get its key call GetDeviceKey");
            }
        }

        public async Task<string> GetDeviceKey(string deviceName)
        {
            var device = await _registryManager.GetDeviceAsync(deviceName);
            if (device == null)
            {
                throw new ArgumentException($"The device: {deviceName} doesn't exist, please add it through AddDeviceAsync");
            }

            return device.Authentication.SymmetricKey.PrimaryKey;
        }

        private string GetConnectionString()
        {
            return $"HostName={_configuration.HubUri};SharedAccessKeyName={_configuration.SasKeyName};SharedAccessKey={_configuration.SasKey}";
        }
    }
}