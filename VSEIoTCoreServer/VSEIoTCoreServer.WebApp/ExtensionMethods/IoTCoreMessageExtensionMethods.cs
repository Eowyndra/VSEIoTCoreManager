using Newtonsoft.Json.Linq;
using VSEIoTCoreServer.DAL.Models.Enums;

namespace VSEIoTCoreServer.WebApp.ExtensionMethods
{
    internal static class IoTCoreMessageExtensionMethods
    {

        public static string? GetConnectionState(this JToken objectData)
        {
            if (objectData["value"] == null || objectData["value"]?.ToList().Count < 1)
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
                    return DeviceStatus.Disconnected;
            }
        }

        public static string GetDeviceType(this JToken objectData)
        {
            if (objectData["value"] == null)
            {
                return null;
            }
            return objectData["value"].ToString();
        }
    }
}
