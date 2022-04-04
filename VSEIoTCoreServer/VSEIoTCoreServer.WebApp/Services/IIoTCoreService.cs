// ----------------------------------------------------------------------------
// Filename: IIoTCoreService.cs
// Copyright (c) 2022 ifm diagnostic GmbH - All rights reserved.
// ----------------------------------------------------------------------------
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace VSEIoTCoreServer.WebApp.Services
{
    using VSEIoTCoreServer.WebApp.ViewModels;

    public interface IIoTCoreService
    {
        /// <summary>
        /// Starts a VSEIoTCore process for the device with the given deviceId.
        /// </summary>
        /// <param name="deviceId">The ID of the device.</param>
        Task Start(int deviceId);

        /// <summary>
        /// Stops a VSEIoTCore process for the device with the given deviceId.
        /// </summary>
        /// <param name="deviceId">The ID of the device.</param>
        Task Stop(int deviceId);

        /// <summary>
        /// Get the current status of the device with the given deviceId.
        /// </summary>
        /// <param name="deviceId">The ID of the device.</param>
        /// <returns>The StatusViewModel containing the IoTStatus and DeviceStatus of the device.</returns>
        Task<StatusViewModel?> Status(int deviceId);
    }
}
