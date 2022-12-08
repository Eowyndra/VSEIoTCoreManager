// ----------------------------------------------------------------------------
// Filename: DeviceIoTCore.cs
// Copyright (c) 2022 ifm diagnostic GmbH - All rights reserved.
// ----------------------------------------------------------------------------
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace VSEIoTCoreServer.WebApp.Models
{
    using System.Diagnostics;
    using Microsoft.Extensions.Options;
    using VSEIoTCoreServer.CommonUtils;
    using VSEIoTCoreServer.CommonUtils.ExtensionMethods;
    using VSEIoTCoreServer.DAL.Models.Enums;
    using VSEIoTCoreServer.WebApp.ViewModels;

    public class DeviceIoTCore : IDisposable
    {
        private readonly ILogger<DeviceIoTCore> _logger;
        private readonly IoTCoreOptions _iotCoreOptions;
        private readonly DeviceConfigurationViewModel _configuration;
        private readonly StatusViewModel _status = new ();
        private Process? _process;
        private DeviceStatus _latestDeviceStatus;
        private DateTime _connectingSince = new ();

        // Timer could later be changed by a Pub/Sub Connection to the IoT Core e.g. via a MQTT connection --> if supported; that way we wouldn't need to poll periodically
        private Timer? _statusUpdater;

        public DeviceIoTCore(ILoggerFactory loggerFactory, IOptions<IoTCoreOptions> iotCoreOptions, DeviceConfigurationViewModel configuration)
        {
            var factory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            _logger = factory.CreateLogger<DeviceIoTCore>();

            var options = iotCoreOptions ?? throw new ArgumentNullException(nameof(iotCoreOptions));
            _iotCoreOptions = options.Value;

            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

            _status.IoTStatus = IoTStatus.Stopped;
            _status.DeviceStatus = DeviceStatus.Disconnected;
        }

        public delegate void DeviceStatusEventHandler(DeviceIoTCore sender, DeviceStatus status);
        public event DeviceStatusEventHandler? StatusChanged;

        public void Dispose()
        {
            _statusUpdater?.Dispose();
        }

        public async Task Start()
        {
            await Task.Run(() =>
            {
                // Start VSEIoTCore specified by deviceId
                var startInfo = new ProcessStartInfo(_iotCoreOptions.AdapterLocation);
                startInfo.Arguments = "--vse-ip " + _configuration.VseIpAddress +
                    " --vse-port " + _configuration.VsePort +
                    " --iotcore-uri " + _iotCoreOptions.IoTCoreURI + ":" + _configuration.IoTCorePort +
                    " --iotcore-id " + _configuration.Id;
                _logger.LogInformation($"Starting VSEIoTCore for device {_configuration.Id}");

                try
                {
                    if (startInfo != null)
                    {
                        _process = Process.Start(startInfo);
                    }

                    _logger.LogInformation($"Successfully started VSEIoTCore for device {_configuration.Id}");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error starting VSEIotCore: {ex.Message}");
                    throw;
                }
            });

            _statusUpdater = new Timer(CheckDeviceStatus, null, 1000, 5000);
        }

        public async Task Stop()
        {
            await Task.Run(() =>
            {
                _logger.LogInformation($"Stopping VSEIoTCore for device {_configuration.Id}");

                try
                {
                    _process?.Kill();
                    _process?.WaitForExit();
                    _process?.Dispose();
                    _process = null;
                    _status.DeviceStatus = DeviceStatus.Disconnected;
                    _status.IoTStatus = IoTStatus.Stopped;
                    _logger.LogInformation($"Successfully stopped VSEIoTCore for device {_configuration.Id}");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error stopping VSEIoTCore: {ex.Message}");
                    throw;
                }
            });

            if (_statusUpdater != null)
            {
                await _statusUpdater.DisposeAsync();
            }
        }

        public StatusViewModel GetStatus()
        {
            var status = new StatusViewModel();
            status.IoTStatus = _process == null ? IoTStatus.Stopped : IoTStatus.Started;
            status.DeviceStatus = _status.DeviceStatus;
            if (status.DeviceStatus == DeviceStatus.Connecting)
            {
                if ((DateTime.Now - _connectingSince).TotalSeconds > 10)
                {
                    status.DeviceStatus = DeviceStatus.Timeout;
                }
            }

            return status;
        }

        public DeviceConfigurationViewModel GetDeviceConfiguration()
        {
            return _configuration;
        }

        private static async Task<string> GetDeviceType(string ioTCoreURI, int ioTCorePort)
        {
            var deviceType = await IoTCoreUtils.WaitForDeviceType(ioTCoreURI, ioTCorePort);
            return deviceType;
        }

        private async void CheckDeviceStatus(object? state)
        {
            var iotAddress = _iotCoreOptions.IoTCoreURI + ":" + _configuration.IoTCorePort;

            try
            {
                using var client = new Client(iotAddress);
                var result = await client.SendRequestAndAwaitResponseAsync(IoTCoreRoutes.Device().Status().GetData());
                var deviceStatusMessage = IoTCoreUtils.CreateResponseMessage(result);
                _latestDeviceStatus = deviceStatusMessage.Data.GetDeviceStatus();
            }
            catch (HttpRequestException)
            {
                // IoTCore has not been started and/or is not reachable, set status to disconnected
                _latestDeviceStatus = DeviceStatus.Disconnected;
            }
            catch (Exception e)
            {
                _logger.LogError("Error getting device status: " + e.Message);
                throw;
            }

            if (_status.DeviceStatus != _latestDeviceStatus)
            {
                _status.DeviceStatus = _latestDeviceStatus;
                StatusChanged?.Invoke(this, _status.DeviceStatus);

                // If status is 'Connecting' remember the time
                if (_status.DeviceStatus == DeviceStatus.Connecting)
                {
                    _connectingSince = DateTime.Now;
                }

                // If status is 'Connected' also check the DeviceType
                if (_status.DeviceStatus == DeviceStatus.Connected)
                {
                    var deviceType = await GetDeviceType(_iotCoreOptions.IoTCoreURI, _configuration.IoTCorePort);
                    if (!string.IsNullOrEmpty(deviceType))
                    {
                        _configuration.VseType = deviceType;
                    }
                }
            }
        }
    }
}
