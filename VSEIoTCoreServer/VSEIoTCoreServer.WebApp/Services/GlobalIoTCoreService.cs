// ----------------------------------------------------------------------------
// Filename: GlobalIoTCoreService.cs
// Copyright (c) 2022 ifm diagnostic GmbH - All rights reserved.
// ----------------------------------------------------------------------------
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace VSEIoTCoreServer.WebApp.Services
{
    using Microsoft.Extensions.Options;
    using VSEIoTCoreServer.CommonUtils;
    using VSEIoTCoreServer.DAL.Models.Enums;
    using VSEIoTCoreServer.LibraryRuntime;
    using VSEIoTCoreServer.WebApp.ViewModels;

    public class GlobalIoTCoreService : IGlobalIoTCoreService
    {
        private static readonly GlobalIoTCoreStatusViewModel _globalIoTCoreStatus = new ();
        private readonly IDeviceConfigurationService _deviceConfigurationService;
        private readonly IIoTCoreService _iotCoreService;
        private readonly IIoTCoreRuntime _iotCoreRuntime;
        private readonly IoTCoreOptions _iotCoreOptions;
        private readonly ILogger<GlobalIoTCoreService> _logger;

        public GlobalIoTCoreService(
            IDeviceConfigurationService deviceConfigurationService,
            IIoTCoreService iotCoreService,
            IIoTCoreRuntime iotCoreRuntime,
            ILoggerFactory loggerFactory,
            IOptions<IoTCoreOptions> iotCoreOptions)
        {
            _deviceConfigurationService = deviceConfigurationService ?? throw new ArgumentNullException(nameof(deviceConfigurationService));
            _iotCoreService = iotCoreService ?? throw new ArgumentNullException(nameof(iotCoreService));
            _iotCoreRuntime = iotCoreRuntime ?? throw new ArgumentNullException(nameof(iotCoreRuntime));
            var options = iotCoreOptions ?? throw new ArgumentNullException(nameof(iotCoreOptions));
            _iotCoreOptions = options.Value;

            var factory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            _logger = factory.CreateLogger<GlobalIoTCoreService>();
        }

        public async Task AddMirror(DeviceConfigurationViewModel deviceConfig)
        {
            await Task.Run(() =>
            {
                try
                {
                    _logger.LogInformation($"Adding VSEIoTCore {_iotCoreOptions.IoTCoreURI}:{deviceConfig.IoTCorePort} to global IoTCore");
                    _iotCoreRuntime.AddMirror(_iotCoreOptions.IoTCoreURI, deviceConfig.IoTCorePort);
                    _logger.LogInformation($"Successfully added VSEIoTCore {_iotCoreOptions.IoTCoreURI}:{deviceConfig.IoTCorePort} to global IoTCore");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error mirroring VSEIoTCores into global IoTCore: {ex.Message}");
                    throw;
                }
            });
        }

        public async Task Start()
        {
            if (_globalIoTCoreStatus.Status != GlobalIoTCoreStatus.Stopped)
            {
                throw new InvalidOperationException("Already running");
            }

            // Start the global IoTCore instance
            try
            {
                _logger.LogInformation("Starting global IoTCore server");
                _iotCoreRuntime.Start(_iotCoreOptions.IoTCoreURI, _iotCoreOptions.GlobalIoTCorePort);
                _globalIoTCoreStatus.Status = GlobalIoTCoreStatus.Starting;
                _logger.LogInformation("Global IoTCore server started");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error starting global IoTCore server: {ex.Message}");
                throw;
            }

            // Read device configurations from DataBase
            var deviceConfigurations = await _deviceConfigurationService.GetAll();

            // Start a VSEIoTCore instance for each device
            await StartVSEIoTCores(deviceConfigurations);

            // Only VSEIoTCore instances that have been started successfully can be added to the global IoT Core instance
            var startedDevices = await GetStartedDevices(deviceConfigurations);

            // Activate mirroring for every started VSEIoTCore
            await ActivateMirroring(startedDevices);

            _globalIoTCoreStatus.Status = startedDevices.Count < deviceConfigurations.Count ? GlobalIoTCoreStatus.PartlyRunning : GlobalIoTCoreStatus.Started;
        }

        public async Task Stop()
        {
            if (_globalIoTCoreStatus.Status != GlobalIoTCoreStatus.Started && _globalIoTCoreStatus.Status != GlobalIoTCoreStatus.PartlyRunning)
            {
                throw new InvalidOperationException("Not started");
            }

            _globalIoTCoreStatus.Status = GlobalIoTCoreStatus.Stopping;

            // Read device configurations from DataBase
            var deviceConfigurations = await _deviceConfigurationService.GetAll();

            // Stop the VSEIoTCore instance for each device
            await StopVSEIoTCores(deviceConfigurations);

            // IoTCores are stopped asynchronous
            // this call ensures each VSEIoTCore is stopped to avoid problems when stopping the global IoTCore instance
            var stopped = await WaitUntilStopped(deviceConfigurations);

            if (!stopped)
            {
                _logger.LogError("Error: VSEIoTCores are still running");
                throw new TimeoutException();
            }

            // Stop the global IoTCore instance
            try
            {
                _logger.LogInformation("Stopping global IoTCore server");
                _iotCoreRuntime.Stop();
                _globalIoTCoreStatus.Status = GlobalIoTCoreStatus.Stopped;
                _logger.LogInformation("Global IoTCore server stopped");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error stopping global IoTCore server: {ex.Message}");
                throw;
            }
        }

        public Task<GlobalIoTCoreStatusViewModel> GetStatus()
        {
            return Task.FromResult(_globalIoTCoreStatus);
        }

        private Task StartVSEIoTCores(List<DeviceConfigurationViewModel> deviceConfigurations)
        {
            return Task.WhenAll(deviceConfigurations.Select(device => _iotCoreService.Start(device.Id)));
        }

        private async Task ActivateMirroring(List<DeviceConfigurationViewModel> deviceConfigurations)
        {
            foreach (var device in deviceConfigurations)
            {
                await AddMirror(device);
            }
        }

        private Task StopVSEIoTCores(List<DeviceConfigurationViewModel> deviceConfigurations)
        {
            return Task.WhenAll(deviceConfigurations.Select(device => _iotCoreService.Stop(device.Id)));
        }

        private async Task<List<DeviceConfigurationViewModel>> GetStartedDevices(List<DeviceConfigurationViewModel> deviceConfigurations, int maxWaitInMilliseconds = 60_000)
        {
            var startedDevices = new List<DeviceConfigurationViewModel>();
            foreach (var device in deviceConfigurations)
            {
                var started = await IoTCoreUtils.WaitUntilVSEIoTCoreStarted(_iotCoreOptions.IoTCoreURI, device.IoTCorePort, maxWaitInMilliseconds);
                if (started)
                {
                    startedDevices.Add(device);
                }
            }

            return startedDevices;
        }

        private async Task<bool> WaitUntilStopped(List<DeviceConfigurationViewModel> deviceConfigurations, int maxWaitInMilliseconds = 60_000)
        {
            var allStopped = true;
            foreach (var device in deviceConfigurations)
            {
                allStopped &= await IoTCoreUtils.WaitUntilVSEIoTCoreStopped(_iotCoreOptions.IoTCoreURI, device.IoTCorePort, maxWaitInMilliseconds);
            }

            return allStopped;
        }
    }
}
