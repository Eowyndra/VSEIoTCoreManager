using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Diagnostics;
using VSEIoTCoreServer.DAL.Models.Enums;
using VSEIoTCoreServer.WebApp.ExtensionMethods;
using VSEIoTCoreServer.CommonUtils;
using VSEIoTCoreServer.WebApp.ViewModels;
using AutoMapper;

namespace VSEIoTCoreServer.WebApp.Services
{
    public class IoTCoreService : IIoTCoreService
    {
        private readonly IMapper _mapper;
        private readonly ILogger<IoTCoreService> _logger;
        private readonly IDeviceConfigurationService _deviceConfigurationService;
        private readonly IoTCoreOptions _iotCoreOptions;
        private static ConcurrentDictionary<int, Process> _iotCoreProcessForDeviceId = new ConcurrentDictionary<int, Process>();
 

        public IoTCoreService(IMapper mapper, 
            IDeviceConfigurationService deviceConfigurationService,
            ILoggerFactory loggerFactory,
            IOptions<IoTCoreOptions> iotCoreOptions)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _deviceConfigurationService = deviceConfigurationService ?? throw new ArgumentNullException(nameof(deviceConfigurationService));
            var factory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            _logger = factory.CreateLogger<IoTCoreService>();
            var options = iotCoreOptions ?? throw new ArgumentNullException(nameof(iotCoreOptions));
            _iotCoreOptions = options.Value;
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

            // If device has no device type, try to get device type from IoTCore and update entity in database
            if (device.VseType == null || device.VseType == "")
            {
                var deviceType = await GetDeviceType(_iotCoreOptions.IoTCoreURI, device.IoTCorePort);
                if (deviceType != "")
                {
                    device.VseType = deviceType;
                    await _deviceConfigurationService.UpdateDevice(device);
                }
            }
        }

        public async Task Stop(int deviceId)
        {
            await Task.Run(() =>
            {
                // Stop VSEIoTCore specified by deviceId
                Process process;

                if (!_iotCoreProcessForDeviceId.TryGetValue(deviceId, out process)) return;
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

        public async Task<StatusViewModel?> Status(int deviceId)
        {
            // Get device model from database
            var device = await _deviceConfigurationService.GetById(deviceId);
            if (device == null)
            {
                _logger.LogInformation($"Device with Id={deviceId} not found!");
                return null;
            }

            // IoTStatus: has the VSEIoTCore process been started?
            device.IoTStatus = _iotCoreProcessForDeviceId.ContainsKey(deviceId) ? IoTStatus.Running : IoTStatus.Stopped;

            // DeviceStatus: is the device reachable via IoTCore URI?
            try
            {
                using (var client = new Client(_iotCoreOptions.IoTCoreURI + ":" + device.IoTCorePort))
                {
                    var result = await client.SendRequestAndAwaitResponseAsync(IoTCoreRoutes.Device().Status().GetData());
                    var deviceStatusMessage = IoTCoreUtils.CreateResponseMessage(result);
                    device.DeviceStatus = deviceStatusMessage.Data.GetDeviceStatus();
                }
            }
            catch (HttpRequestException)
            {
                // IoTCore has not been started and/or is not reachable, set status to disconnected
                device.DeviceStatus = DeviceStatus.Disconnected;
            }
            catch (Exception e)
            {
                _logger.LogError("Error getting VSEIoTCore status: " + e.Message);
                throw;
            }

            var statusViewModel = _mapper.Map<StatusViewModel>(device); 
            return statusViewModel;
        }

        private async Task<string> GetDeviceType(string ioTCoreURI, int ioTCorePort)
        {
            var deviceType = "";
            var started = await IoTCoreUtils.WaitUntilVSEIoTCoreStarted(ioTCoreURI, ioTCorePort);
            if (started)
            {
                //get device type from iotcore
                using (var client = new Client(ioTCoreURI + ":" + ioTCorePort))
                {
                    var result = await client.SendRequestAndAwaitResponseAsync(IoTCoreRoutes.Device().Information().Device().Type().GetData());
                    var message = IoTCoreUtils.CreateResponseMessage(result);
                    deviceType = message.Data.GetDeviceType();
                }
            }
            return deviceType;
        }
    }
}
