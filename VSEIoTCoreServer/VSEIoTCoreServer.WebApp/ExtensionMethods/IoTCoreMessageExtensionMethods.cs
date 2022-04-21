// ----------------------------------------------------------------------------
// Filename: IoTCoreMessageExtensionMethods.cs
// Copyright (c) 2022 ifm diagnostic GmbH - All rights reserved.
// ----------------------------------------------------------------------------
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace VSEIoTCoreServer.WebApp.ExtensionMethods
{
    using Newtonsoft.Json.Linq;
    using VSEIoTCoreServer.DAL.Models.Enums;

    public static class IoTCoreMessageExtensionMethods
    {
        public static string? GetConnectionState(this JToken objectData)
        {
            if (objectData?["value"] == null || objectData["value"]?.ToList().Count < 1)
            {
                return null;
            }

            return objectData["value"]?["ConnectionState"]?.ToString();
        }

        public static DeviceStatus GetDeviceStatus(this JToken objectData)
        {
            var connectionState = GetConnectionState(objectData);
            switch (connectionState)
            {
                case "disconnected":
                    return DeviceStatus.Disconnected;
                case "connecting":
                    return DeviceStatus.Connecting;
                case "connected":
                    return DeviceStatus.Connected;
                default:
                    return DeviceStatus.Pending;
            }
        }

        public static string? GetDeviceType(this JToken objectData)
        {
            if (objectData?["value"] == null)
            {
                return null;
            }

            return objectData["value"]?.ToString();
        }

        public static int? GetCount(this JToken objectData)
        {
            if (objectData?["value"] == null)
            {
                return null;
            }

            return objectData["value"]?.ToList().Count;
        }
    }
}
