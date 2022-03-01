using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace VSEIoTCoreServer.Services
{
    public class IoTCoreService : IIoTCoreService
    {
        private static ConcurrentDictionary<int, Process> _iotCoreProcessForDeviceId = new ConcurrentDictionary<int, Process>();
        private readonly IDeviceConfigurationService _deviceConfigurationService;
        private readonly IoTCoreOptions _iotCoreOptions;
        private readonly ILogger<IoTCoreService> _logger;

        public IoTCoreService(IDeviceConfigurationService deviceConfigurationService, ILoggerFactory loggerFactory, IOptions<IoTCoreOptions> iotCoreOptions)
        {
            _deviceConfigurationService = deviceConfigurationService ?? throw new ArgumentNullException(nameof(deviceConfigurationService));
            var options = iotCoreOptions ?? throw new ArgumentNullException(nameof(iotCoreOptions));
            _iotCoreOptions = options.Value; 
            var factory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            _logger = factory.CreateLogger<IoTCoreService>();
        }

        public async Task Start(int deviceId)
        {
            var device = await _deviceConfigurationService.GetById(deviceId);

            await Task.Run(() =>
            {
                // Start VSEIoTCore specified by deviceId
                var startInfo = new ProcessStartInfo(_iotCoreOptions.AdapterLocation);
                startInfo.Arguments = "--vse-ip " + device.VseIpAddress +
                    " --vse-port " + device.VsePort +
                    " --iotcore-uri " + _iotCoreOptions.IoTCoreURI + ":" + device.IoTCorePort +
                    " --iotcore-id " + device.Id;
                _logger.LogInformation($"Starting VSEIoTCore for device {device.Id}... (IP-Address: {device.VseIpAddress}:{device.VsePort} on URI: {_iotCoreOptions.IoTCoreURI}:{device.IoTCorePort})");

                try
                {
                    var process = Process.Start(startInfo);
                    if (process != null)
                    {
                        _iotCoreProcessForDeviceId[device.Id] = process;
                    }
                    _logger.LogInformation($"Successfully started VSEIoTCore for device {device.Id}... (IP-Address: {device.VseIpAddress}:{device.VsePort} on URI: {_iotCoreOptions.IoTCoreURI}:{device.IoTCorePort})");
                }
                catch (Exception e)
                {
                    _logger.LogError("Error starting VSEIotCore: " + e.Message);
                    throw;
                }
            });
        }

        public async Task Stop(int deviceId)
        {
            await Task.Run(() =>
            {
                // Stop VSEIoTCore specified by deviceId
                Process process;
                
                if(!_iotCoreProcessForDeviceId.TryGetValue(deviceId, out process)) return;
                _logger.LogInformation($"Stopping VSEIoTCore for device with ID: {deviceId} ...");
                
                try
                {
                    process?.Kill();
                    process?.WaitForExit();

                    if (process.HasExited)
                    {
                        _iotCoreProcessForDeviceId.TryRemove(deviceId, out _);
                        _logger.LogInformation($"Successfully stopped VSEIoTCore for device with ID: {deviceId}.");
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError("Error stopping VSEIoTCore: " + e.Message);
                    throw;
                }
            });
        }
    }
}
