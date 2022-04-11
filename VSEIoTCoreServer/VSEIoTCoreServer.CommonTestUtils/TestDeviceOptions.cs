// ----------------------------------------------------------------------------
// Filename: TestDeviceOptions.cs
// Copyright (c) 2022 ifm diagnostic GmbH - All rights reserved.
// ----------------------------------------------------------------------------
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace VSEIoTCoreServer.CommonTestUtils
{
    public class TestDeviceOptions
    {
        public const string TestDevice1 = "TestDevice1";
        public const string TestDevice2 = "TestDevice2";
        public const string TestDevice3 = "TestDevice3";

        public int Id { get; set; } = 0;
        public string Name { get; set; } = string.Empty;
        public string VseType { get; set; } = string.Empty;
        public string VseIpAddress { get; set; } = string.Empty;
        public int VsePort { get; set; } = 0;
        public int IoTCorePort { get; set; } = 0;
    }
}
