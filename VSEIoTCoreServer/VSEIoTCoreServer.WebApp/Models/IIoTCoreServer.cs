// ----------------------------------------------------------------------------
// Filename: IIoTCoreServer.cs
// Copyright (c) 2022 ifm diagnostic GmbH - All rights reserved.
// ----------------------------------------------------------------------------
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace VSEIoTCoreServer.WebApp.Models
{
    using VSEIoTCoreServer.WebApp.ViewModels;

    public interface IIoTCoreServer
    {
        /// <summary>
        /// Starts a global IoTCore instance and all configured VSEIoTCore instances and mirrors them into the global IoTCore.
        /// </summary>
        Task Start();

        /// <summary>
        /// Stops the global IoTCore instance and all started VSEIoTCore instances.
        /// </summary>
        Task Stop();

        /// <summary>
        /// Add a list of new devices to the cache.
        /// </summary>
        /// <param name="devices">A list of new devices to be added to the cache.</param>
        Task AddRange(List<DeviceConfigurationViewModel> devices);

        /// <summary>
        /// Add a new device to the cache.
        /// </summary>
        /// <param name="device">The device to be added to the cache.</param>
        Task Add(DeviceConfigurationViewModel device);

        /// <summary>
        /// Get the status of the global IoTCore instance.
        /// </summary>
        /// <returns>The status of the global IoTCore instance.</returns>
        Task<GlobalIoTCoreStatusViewModel> GetStatus();

        /// <summary>
        /// Get a list of devices that are stored in the cache.
        /// </summary>
        /// <returns>A list of devices stored in the cache.</returns>
        Task<List<DeviceIoTCore>> GetDevices();
    }
}
