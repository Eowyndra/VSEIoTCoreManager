using Microsoft.Extensions.Options;
using VSEIoTCoreServer.LibraryRuntime;
using VSEIoTCoreServer.ViewModels;

namespace VSEIoTCoreServer.Services
{
    public class GlobalIoTCoreService : IGlobalIoTCoreService
    {
        private readonly IDeviceConfigurationService _deviceConfigurationService;
        private readonly IIoTCoreService _iotCoreService;
        private readonly IIoTCoreRuntime _iotCoreRuntime;
        private readonly IoTCoreOptions _iotCoreOptions;
        private readonly ILogger<GlobalIoTCoreService> _logger;
        private readonly Uri _globalIoTCoreServerUri;

        public GlobalIoTCoreService(IDeviceConfigurationService deviceConfigurationService, 
            IIoTCoreService iotCoreService,
            IIoTCoreRuntime iotCoreRuntime,
            ILoggerFactory loggerFactory, 
            IOptions<IoTCoreOptions> iotCoreOptions)
        {
            _deviceConfigurationService = deviceConfigurationService ?? throw new ArgumentNullException(nameof(deviceConfigurationService));
            _iotCoreService = iotCoreService ?? throw new ArgumentNullException(nameof(iotCoreService));
            _iotCoreRuntime = iotCoreRuntime ?? throw new ArgumentNullException(nameof(iotCoreRuntime));
            var options = iotCoreOptions ?? throw new ArgumentNullException(nameof(iotCoreOptions));

            var factory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            _logger = factory.CreateLogger<GlobalIoTCoreService>();

            _iotCoreOptions = options.Value;
            _globalIoTCoreServerUri = new Uri($"{_iotCoreOptions.IoTCoreURI}:{_iotCoreOptions.GlobalIoTCorePort}");
        }

        public async Task AddMirror(DeviceConfigurationViewModel deviceConfig)
        {
            await Task.Run(() =>
            {
                try
                {
                    _logger.LogInformation($"Mirroring VSEIoTCore {_iotCoreOptions.IoTCoreURI}:{deviceConfig.IoTCorePort} into global IoTCore {_globalIoTCoreServerUri} ...");
                    _iotCoreRuntime.AddMirror(_iotCoreOptions.IoTCoreURI, deviceConfig.IoTCorePort);
                    _logger.LogInformation($"Successfully mirrored VSEIoTCore {_iotCoreOptions.IoTCoreURI}:{deviceConfig.IoTCorePort} into global IoTCore {_globalIoTCoreServerUri}!");
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error mirroring VSEIoTCores into global IoTCore: " + ex.Message);
                    throw;
                }
            });
        }

        public async Task Start()
        {
            // start the global IoTCore instance
            try
            {
                _logger.LogInformation("Starting global IoTCore server... ");
                _iotCoreRuntime.Start(_iotCoreOptions.IoTCoreURI, _iotCoreOptions.GlobalIoTCorePort);
                _logger.LogInformation("Global IoTCore server started!");
            }
            catch (Exception ex)
            {
                _logger.LogError("Error starting global IoTCore server: " + ex.Message);
                throw;
            }

            // read device configurations from DataBase
            var deviceConfigurations = await _deviceConfigurationService.GetAll();

            // start a VSEIoTCore instance for each device
            await StartVSEIoTCores(deviceConfigurations);

            // IoTCores are started asynchronous
            // this waiting period is to ensure they are started to avoid problem when adding them to the global IoTCore instance
            Thread.Sleep(5000);

            // activate mirroring for every started VSEIoTCore
            await ActivateMirroring(deviceConfigurations);
        }

        public async Task Stop()
        {
            // read device configurations from DataBase
            var deviceConfigurations = await _deviceConfigurationService.GetAll();

            // stop the VSEIoTCore instance for each device
            await StopVSEIoTCores(deviceConfigurations);

            // stop the global IoTCore instance
            await Task.Run(() =>
            {
                try
                {
                    _logger.LogInformation("Stopping global IoTCore server... ");
                    _iotCoreRuntime.Stop();
                    _logger.LogInformation("Global IoTCore server stopped!");
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error stopping global IoTCore server: " + ex.Message);
                    throw;
                }
            });
        }

        private async Task StartVSEIoTCores(List<DeviceConfigurationViewModel> deviceConfigurations)
        {
            foreach (var device in deviceConfigurations)
            {
                await _iotCoreService.Start(device.Id);
            }
        }

        private async Task ActivateMirroring(List<DeviceConfigurationViewModel> deviceConfigurations)
        {
            foreach (var device in deviceConfigurations)
            {
                await AddMirror(device);
            }
        }

        private async Task StopVSEIoTCores(List<DeviceConfigurationViewModel> deviceConfigurations)
        {
            foreach (var device in deviceConfigurations)
            {
                await _iotCoreService.Stop(device.Id);
            }
        }
    }
}
