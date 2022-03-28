using System;

namespace VSEIoTCoreServer.CommonTestUtils
{
    public class TestDeviceOptions
    {
        public const string TestDevice1 = "TestDevice1";
        public const string TestDevice2 = "TestDevice2";
        public const string TestDevice3 = "TestDevice3";

        public int Id { get; set; } = 0;
        public string VseType { get; set; } = string.Empty;
        public string VseIpAddress { get; set; } = string.Empty;
        public int VsePort { get; set; } = 0;
        public int IoTCorePort { get; set; } = 0;
    }
}
