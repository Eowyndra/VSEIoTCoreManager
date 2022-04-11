// ----------------------------------------------------------------------------
// Filename: TestUtils.cs
// Copyright (c) 2022 ifm diagnostic GmbH - All rights reserved.
// ----------------------------------------------------------------------------
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace VSEIoTCoreServer.CommonTestUtils
{
    using VSEIoTCoreServer.DAL.Models;

    public class TestUtils
    {
        public static DeviceConfiguration GetDeviceConfiguration(TestDeviceOptions testDevice)
        {
            if (testDevice == null)
            {
                throw new ArgumentNullException(nameof(testDevice));
            }

            var deviceConfig = new DeviceConfiguration()
            {
                Id = testDevice.Id,
                Name = testDevice.Name,
                VseType = testDevice.VseType,
                VseIpAddress = testDevice.VseIpAddress,
                VsePort = testDevice.VsePort,
                IoTCorePort = testDevice.IoTCorePort,
            };

            return deviceConfig;
        }
    }
}
