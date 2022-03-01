using System;

namespace VSEIoTCoreServer.IntegrationTests
{
    public class TestDeviceOptions
    {
        public const string TestDevice1 = "TestDevice1";
        public const string TestDevice2 = "TestDevice2";

        public int Id { get; set; } = 0;
        public string VseType { get; set; } = String.Empty;
        public string VseIpAddress { get; set; } = String.Empty;
        public int VsePort { get; set; } = 0;
        public int IoTCorePort { get; set; } = 0;
    }
}
