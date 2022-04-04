// ----------------------------------------------------------------------------
// Filename: IDeviceConfigurationService.cs
// Copyright (c) 2022 ifm diagnostic GmbH - All rights reserved.
// ----------------------------------------------------------------------------
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace VSEIoTCoreServer.WebApp.Services
{
    using VSEIoTCoreServer.WebApp.ViewModels;

    public interface IDeviceConfigurationService
    {
        /// <summary>
        /// Get a List of configured devices from the SQLite DataBase.
        /// </summary>
        /// <returns>A List of configured devices.</returns>
        Task<List<DeviceConfigurationViewModel>> GetAll();

        /// <summary>
        /// Get the device configuration for a specific device with the given deviceId from the SQLite DataBase.
        /// </summary>
        /// <param name="deviceId">The ID of the device.</param>
        /// <returns>The device configuration for the device specified by deviceId.</returns>
        Task<DeviceConfigurationViewModel> GetById(int deviceId);

        /// <summary>
        /// Add a list of new device configurations to the database.
        /// </summary>
        /// <param name="deviceModels">A list of devices that shall be added.</param>
        /// <returns>A list of devices that have been added to the database.</returns>
        Task<List<DeviceConfigurationViewModel>> AddDevices(List<AddDeviceViewModel> deviceModels);

        /// <summary>
        /// Update an existing device configuration in the database.
        /// </summary>
        /// <param name="deviceModel">The model containing the device configuration.</param>
        Task UpdateDevice(DeviceConfigurationViewModel deviceModel);
    }
}
