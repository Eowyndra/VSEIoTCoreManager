using VSEIoTCoreServer.WebApp.ViewModels;

namespace VSEIoTCoreServer.WebApp.Services
{
    public interface IGlobalIoTCoreService
    {
        /// <summary>
        /// Starts a global IoTCore instance and all configured VSEIoTCore instances and mirrors them into the global IoTCore.
        /// </summary>
        /// <returns></returns>
        Task Start();

        /// <summary>
        /// Stops the global IoTCore instance and all started VSEIoTCore instances
        /// </summary>
        /// <returns></returns>
        Task Stop();

        /// <summary>
        /// Add the VSEIoTCore instance of the device specified by deviceId to the global IoTCore.
        /// </summary>
        /// <param name="deviceConfig">The configuration of the device that shall be added to the global IoTCore</param>
        /// <returns></returns>
        Task AddMirror(DeviceConfigurationViewModel deviceConfig);

        /// <summary>
        /// Get the status of the global IoTCore instance. (Running/Stopped/PartlyRunning)
        /// </summary>
        /// <returns>The status of the global IoTCore instance. (Running/Stopped/PartlyRunning)</returns>
        Task<GlobalIoTCoreStatusViewModel> GetStatus();
    }
}
