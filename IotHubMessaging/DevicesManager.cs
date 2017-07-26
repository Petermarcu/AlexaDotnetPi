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

        public async Task<string> AddDeviceOrGetKeyAsync(string deviceName)
        {
            Device device;
            try
            {
                device = await _registryManager.AddDeviceAsync(new Device(deviceName));
            }
            catch (DeviceAlreadyExistsException)
            {
                device = await _registryManager.GetDeviceAsync(deviceName);
            }

            return device.Authentication.SymmetricKey.PrimaryKey;
        }

        private string GetConnectionString()
        {
            return $"HostName={_configuration.HubUri};SharedAccessKeyName={_configuration.SasKeyName};SharedAccessKey={_configuration.SasKey}";
        }
    }
}