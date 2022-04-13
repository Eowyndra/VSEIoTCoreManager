// ----------------------------------------------------------------------------
// Filename: IoTCoreService.cs
// Copyright (c) 2022 ifm diagnostic GmbH - All rights reserved.
// ----------------------------------------------------------------------------
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace VSEIoTCoreServer.WebApp.Services
{
    using System.Collections.Concurrent;
    using System.Diagnostics;
    using AutoMapper;
    using Microsoft.Extensions.Options;
    using VSEIoTCoreServer.CommonUtils;
    using VSEIoTCoreServer.DAL.Models.Enums;
    using VSEIoTCoreServer.WebApp.ExtensionMethods;
    using VSEIoTCoreServer.WebApp.ViewModels;

    public class IoTCoreService : IIoTCoreService
    {
        private static readonly ConcurrentDictionary<int, Process> _iotCoreProcessForDeviceId = new ();
        private readonly IMapper _mapper;
        private readonly ILogger<IoTCoreService> _logger;
        private readonly IDeviceConfigurationService _deviceConfigurationService;
        private readonly IoTCoreOptions _iotCoreOptions;

        public IoTCoreService(
            IMapper mapper,
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
                _logger.LogInformation($"Starting VSEIoTCore for device {device.Id}");

                try
                {
                    var process = Process.Start(startInfo);
                    if (process != null)
                    {
                        _iotCoreProcessForDeviceId[device.Id] = process;
                    }

                    _logger.LogInformation($"Successfully started VSEIoTCore for device {device.Id}");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error starting VSEIotCore: {ex.Message}");
                    throw;
                }
            });

            // If device has no device type, try to get device type from IoTCore and update entity in database
            if (string.IsNullOrEmpty(device.VseType))
            {
                var deviceType = await GetDeviceType(_iotCoreOptions.IoTCoreURI, device.IoTCorePort);
                if (!string.IsNullOrEmpty(deviceType))
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
                if (!_iotCoreProcessForDeviceId.TryGetValue(deviceId, out var process))
                {
                    return;
                }

                _logger.LogInformation($"Stopping VSEIoTCore for device {deviceId}");

                try
                {
                    process?.Kill();
                    process?.WaitForExit();

                    if (process != null && process.HasExited)
                    {
                        _iotCoreProcessForDeviceId.TryRemove(deviceId, out _);
                        _logger.LogInformation($"Successfully stopped VSEIoTCore for device {deviceId}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error stopping VSEIoTCore: {ex.Message}");
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
                _logger.LogInformation($"Device {deviceId} not found!");
                return null;
            }

            // Check if the VSEIoTCore process is started
            device.IoTStatus = GetIoTStatus(deviceId);

            // Check if the device is reachable
            device.DeviceStatus = await GetDeviceStatus(_iotCoreOptions.IoTCoreURI, device.IoTCorePort);

            return _mapper.Map<StatusViewModel>(device);
        }

        public async Task<DeviceStatus> GetDeviceStatus(string iotCoreUrl, int iotCorePort)
        {
            var deviceStatus = DeviceStatus.Disconnected;

            try
            {
                using var client = new Client(iotCoreUrl + ":" + iotCorePort);
                var result = await client.SendRequestAndAwaitResponseAsync(IoTCoreRoutes.Device().Status().GetData());
                var deviceStatusMessage = IoTCoreUtils.CreateResponseMessage(result);
                deviceStatus = deviceStatusMessage.Data.GetDeviceStatus();
            }
            catch (HttpRequestException)
            {
                // IoTCore has not been started and/or is not reachable, set status to disconnected
                deviceStatus = DeviceStatus.Disconnected;
            }
            catch (Exception e)
            {
                _logger.LogError("Error getting device status: " + e.Message);
                throw;
            }

            return deviceStatus;
        }

        public IoTStatus GetIoTStatus(int deviceId)
        {
            return _iotCoreProcessForDeviceId.ContainsKey(deviceId) ? IoTStatus.Running : IoTStatus.Stopped;
        }

        private static async Task<string> GetDeviceType(string ioTCoreURI, int ioTCorePort)
        {
            var deviceType = string.Empty;
            var started = await IoTCoreUtils.WaitUntilVSEIoTCoreStarted(ioTCoreURI, ioTCorePort);
            if (started)
            {
                // Get device type from iotcore
                using var client = new Client(ioTCoreURI + ":" + ioTCorePort);
                var result = await client.SendRequestAndAwaitResponseAsync(IoTCoreRoutes.Device().Information().Device().Type().GetData());
                var message = IoTCoreUtils.CreateResponseMessage(result);
                var devType = message.Data.GetDeviceType();
                deviceType = devType ?? string.Empty;
            }

            return deviceType;
        }
    }
}
