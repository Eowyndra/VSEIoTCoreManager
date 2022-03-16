using VSEIoTCoreServer.ViewModels;

namespace VSEIoTCoreServer.Services
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
        /// <returns></returns>
        Task<IStatus> Status(int deviceId);
    }
}
