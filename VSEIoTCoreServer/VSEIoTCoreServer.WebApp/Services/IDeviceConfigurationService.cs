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
        
        /// <summary>
        /// Add a list of new device configurations to the database
        /// </summary>
        /// <param name="deviceModels"></param>
        /// <returns>A list of device configurations that shall be added to the database</returns>
        Task<List<DeviceConfigurationViewModel>> AddDevices(List<AddDeviceViewModel> deviceModels);

        /// <summary>
        /// Update an existing device configuration in the database
        /// </summary>
        /// <param name="deviceModel">The model containing the device configuration</param>
        /// <returns></returns>
        Task UpdateDevice(DeviceConfigurationViewModel deviceModel);
    }
}
