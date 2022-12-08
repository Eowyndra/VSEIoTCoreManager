// ----------------------------------------------------------------------------
// Filename: IoTCoreServer.cs
// Copyright (c) 2022 ifm diagnostic GmbH - All rights reserved.
// ----------------------------------------------------------------------------
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace VSEIoTCoreServer.WebApp.Models
{
    using System.Collections.Concurrent;
    using Microsoft.Extensions.Options;
    using VSEIoTCoreServer.DAL.Models.Enums;
    using VSEIoTCoreServer.LibraryRuntime;
    using VSEIoTCoreServer.WebApp.Services;
    using VSEIoTCoreServer.WebApp.ViewModels;

    public class IoTCoreServer : IoTCoreRuntime, IIoTCoreServer, IDisposable
    {
        // private readonly IGlobalConfigurationService _globalConfigService;
        private readonly IServiceProvider _serviceProvider;
        private readonly IOptions<IoTCoreOptions> _options;
        private readonly IoTCoreOptions _iotCoreOptions;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger<IoTCoreServer> _logger;
        private readonly List<DeviceIoTCore> _vseDevices = new ();
        private readonly ConcurrentBag<DeviceIoTCore> _mirroredDevices = new ();
        private readonly GlobalIoTCoreStatusViewModel _status = new ();

        private readonly Timer? _statusUpdater;

        public IoTCoreServer(IServiceProvider serviceProvider, ILoggerFactory loggerFactory, IOptions<IoTCoreOptions> iotCoreOptions)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            _logger = _loggerFactory.CreateLogger<IoTCoreServer>();

            _options = iotCoreOptions ?? throw new ArgumentNullException(nameof(iotCoreOptions));
            _iotCoreOptions = _options.Value;

            _status.Status = GlobalIoTCoreStatus.Stopped;

            _statusUpdater = new Timer(CheckGlobalStatus, null, 1000, 1000);
        }

        public Task AddRange(List<DeviceConfigurationViewModel> devices)
        {
            if (devices == null || devices.Count < 1)
            {
                _logger.LogError("List of devices is null or empty");
                throw new ArgumentException("null or empty", nameof(devices));
            }

            foreach (var device in devices)
            {
                Add(device);
            }

            return Task.CompletedTask;
        }

        public Task Add(DeviceConfigurationViewModel device)
        {
            var addedDevice = new DeviceIoTCore(_loggerFactory, _options, device);
            _vseDevices.Add(addedDevice);

            addedDevice.StatusChanged += OnDeviceStatusChanged;

            if (_status.Status > GlobalIoTCoreStatus.Starting)
            {
                _ = Task.Run(() => addedDevice.Start());
            }

            return Task.CompletedTask;
        }

        public async Task Start()
        {
            if (_status.Status != GlobalIoTCoreStatus.Stopped)
            {
                throw new InvalidOperationException("Already started");
            }

            _status.Status = GlobalIoTCoreStatus.Starting;

            // Start the global IoTCore instance
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var globalConfigService = scope.ServiceProvider.GetRequiredService<IGlobalConfigurationService>();
                var config = await globalConfigService.GetConfig();

                _logger.LogInformation("Starting global IoTCore server");
                base.Start(_iotCoreOptions.IoTCoreURI, config.GlobalIoTCorePort);
                _logger.LogInformation("Global IoTCore server started");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error starting global IoTCore server: {ex.Message}");
                throw;
            }

            // Start all configured devices
            foreach (var device in _vseDevices)
            {
                await device.Start();
            }
        }

        async Task IIoTCoreServer.Stop()
        {
            if (_status.Status < GlobalIoTCoreStatus.Starting)
            {
                throw new InvalidOperationException("Not started");
            }

            _status.Status = GlobalIoTCoreStatus.Stopping;

            // Stop all configured devices
            foreach (var device in _vseDevices)
            {
                await device.Stop();
            }

            // Stop the global IoTCore instance
            try
            {
                _logger.LogInformation("Stopping global IoTCore server");
                base.Stop();
                _logger.LogInformation("Global IoTCore server stopped");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error stopping global IoTCore server: {ex.Message}");
                throw;
            }

            _mirroredDevices.Clear();
        }

        public Task<GlobalIoTCoreStatusViewModel> GetStatus()
        {
            return Task.FromResult(_status);
        }

        public Task<List<DeviceIoTCore>> GetDevices()
        {
            return Task.FromResult(_vseDevices);
        }

        public void Dispose()
        {
            _statusUpdater?.Dispose();
            _mirroredDevices?.Clear();
        }

        private async Task MirrorDevice(DeviceIoTCore device)
        {
            await Task.Run(() =>
            {
                var deviceConfig = device.GetDeviceConfiguration();
                try
                {
                    _logger.LogInformation($"Adding VSEIoTCore {_iotCoreOptions.IoTCoreURI}:{deviceConfig.IoTCorePort} to global IoTCore");
                    base.AddMirror(_iotCoreOptions.IoTCoreURI, deviceConfig.IoTCorePort);
                    _logger.LogInformation($"Successfully added VSEIoTCore {_iotCoreOptions.IoTCoreURI}:{deviceConfig.IoTCorePort} to global IoTCore");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error mirroring VSEIoTCores into global IoTCore: {ex.Message}");
                    throw;
                }
            });
        }

        private void OnDeviceStatusChanged(DeviceIoTCore? sender, DeviceStatus e)
        {
            if (e == DeviceStatus.Connected)
            {
                var alreadyMirrored = _mirroredDevices.Contains(sender);
                var globalStarted = _status.Status > GlobalIoTCoreStatus.Starting;

                if (sender != null && globalStarted && !alreadyMirrored)
                {
                    _mirroredDevices.Add(sender);
                    _ = Task.Run(async () => await MirrorDevice(sender));
                }
            }
        }

        private void CheckGlobalStatus(object? state)
        {
            var anyDeviceConnecting = false;
            var anyDevicePending = false;
            var anyDeviceTimeout = false;
            var anyDeviceDisconnected = false;
            var anyDeviceConnected = false;

            var anyIoTCoreStopped = false;
            var anyIoTCoreStarted = false;

            foreach (var device in _vseDevices)
            {
                var deviceStatus = device.GetStatus().DeviceStatus;
                anyDeviceConnecting |= deviceStatus == DeviceStatus.Connecting;
                anyDevicePending |= deviceStatus == DeviceStatus.Pending;
                anyDeviceTimeout |= deviceStatus == DeviceStatus.Timeout;
                anyDeviceDisconnected |= deviceStatus == DeviceStatus.Disconnected;
                anyDeviceConnected |= deviceStatus == DeviceStatus.Connected;

                var iotStatus = device.GetStatus().IoTStatus;
                anyIoTCoreStopped |= iotStatus == IoTStatus.Stopped;
                anyIoTCoreStarted |= iotStatus == IoTStatus.Started;
            }

            if ((_status.Status == GlobalIoTCoreStatus.Stopped || _status.Status == GlobalIoTCoreStatus.Starting) && (anyDeviceConnecting || anyDevicePending))
            {
                _status.Status = GlobalIoTCoreStatus.Starting;
            }

            if ((_status.Status == GlobalIoTCoreStatus.Starting || _status.Status == GlobalIoTCoreStatus.Started) &&
                (anyDeviceTimeout || anyDeviceDisconnected || anyIoTCoreStopped))
            {
                _status.Status = GlobalIoTCoreStatus.PartlyRunning;
            }

            if ((_status.Status == GlobalIoTCoreStatus.Starting || _status.Status == GlobalIoTCoreStatus.PartlyRunning) &&
                !anyDeviceConnecting && !anyDevicePending && !anyDeviceTimeout && !anyDeviceDisconnected && !anyIoTCoreStopped)
            {
                _status.Status = GlobalIoTCoreStatus.Started;
            }

            if (_status.Status == GlobalIoTCoreStatus.Stopping && !anyDeviceConnected && !anyIoTCoreStarted && !anyDevicePending && !anyDeviceConnecting)
            {
                _status.Status = GlobalIoTCoreStatus.Stopped;
            }
        }
    }
}
