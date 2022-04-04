// ----------------------------------------------------------------------------
// Filename: IStatus.cs
// Copyright (c) 2022 ifm diagnostic GmbH - All rights reserved.
// ----------------------------------------------------------------------------
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace VSEIoTCoreServer.WebApp.ViewModels
{
    using VSEIoTCoreServer.DAL.Models.Enums;

    public interface IStatus
    {
        IoTStatus IoTStatus { get; set; }
        DeviceStatus DeviceStatus { get; set; }
    }
}
