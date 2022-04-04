// ----------------------------------------------------------------------------
// Filename: IGlobalIoTCoreService.cs
// Copyright (c) 2022 ifm diagnostic GmbH - All rights reserved.
// ----------------------------------------------------------------------------
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace VSEIoTCoreServer.WebApp.Services
{
    using VSEIoTCoreServer.WebApp.ViewModels;

    public interface IGlobalIoTCoreService
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
        /// Add the VSEIoTCore instance of the device specified by deviceId to the global IoTCore.
        /// </summary>
        /// <param name="deviceConfig">The configuration of the device that shall be added to the global IoTCore.</param>
        Task AddMirror(DeviceConfigurationViewModel deviceConfig);

        /// <summary>
        /// Get the status of the global IoTCore instance.
        /// </summary>
        /// <returns>The status of the global IoTCore instance.</returns>
        Task<GlobalIoTCoreStatusViewModel> GetStatus();
    }
}
