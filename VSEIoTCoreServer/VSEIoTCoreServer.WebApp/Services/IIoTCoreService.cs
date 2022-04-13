// ----------------------------------------------------------------------------
// Filename: IIoTCoreService.cs
// Copyright (c) 2022 ifm diagnostic GmbH - All rights reserved.
// ----------------------------------------------------------------------------
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace VSEIoTCoreServer.WebApp.Services
{
    using VSEIoTCoreServer.DAL.Models.Enums;
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

        /// <summary>
        /// Get the status of the device with the given url and port.
        /// </summary>
        /// <param name="iotCoreUrl">The URL of the device.</param>
        /// <param name="iotCorePort">The Port of the device.</param>
        /// <returns>The DeviceStatus of the device.</returns>
        Task<DeviceStatus> GetDeviceStatus(string iotCoreUrl, int iotCorePort);

        /// <summary>
        /// Get the status of the IoTCore process for the device with the given deviceId.
        /// </summary>
        /// <param name="deviceId">The ID of the device.</param>
        /// <returns>The IoTStatus of the device.</returns>
        IoTStatus GetIoTStatus(int deviceId);
    }
}
