using VSEIoTCoreServer.WebApp.ViewModels;

namespace VSEIoTCoreServer.WebApp.Services
{
    public interface IIoTCoreService
    {
        /// <summary>
        /// Starts a VSEIoTCore process for the device with the given deviceId
        /// </summary>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        Task Start(int deviceId);

        /// <summary>
        /// Stops a VSEIoTCore process for the device with the given deviceId
        /// </summary>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        Task Stop(int deviceId);

        /// <summary>
        /// Get the current status of the device with the given deviceId
        /// </summary>
        /// <param name="deviceId"></param>
        /// <returns>The device configuration model containing the status</returns>
        Task<StatusViewModel?> Status(int deviceId);
    }
}
