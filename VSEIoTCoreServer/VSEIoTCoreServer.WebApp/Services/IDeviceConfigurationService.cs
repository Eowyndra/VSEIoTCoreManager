using VSEIoTCoreServer.WebApp.ViewModels;

namespace VSEIoTCoreServer.WebApp.Services
{
    public interface IDeviceConfigurationService
    {
        /// <summary>
        /// Get a List of configured devices from the SQLite DataBase
        /// </summary>
        /// <returns>A List of configured devices</returns>
        Task<List<DeviceConfigurationViewModel>> GetAll();

        /// <summary>
        /// Get the device configuration for a specific device with the given deviceId from the SQLite DataBase
        /// </summary>
        /// <param name="deviceId"></param>
        /// <returns>The device configuration for the device specified by deviceId</returns>
        Task<DeviceConfigurationViewModel> GetById(int deviceId);
    }
}
