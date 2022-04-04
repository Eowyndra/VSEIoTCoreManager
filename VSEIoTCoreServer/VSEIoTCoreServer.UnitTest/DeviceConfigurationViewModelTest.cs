// ----------------------------------------------------------------------------
// Filename: DeviceConfigurationViewModelTest.cs
// Copyright (c) 2022 ifm diagnostic GmbH - All rights reserved.
// ----------------------------------------------------------------------------
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace VSEIoTCoreServer.UnitTest
{
    using System;
    using System.IO;
    using Microsoft.Extensions.Configuration;
    using VSEIoTCoreServer.CommonTestUtils;
    using VSEIoTCoreServer.WebApp.ViewModels;
    using Xunit;

    public class DeviceConfigurationViewModelTest : IDisposable
    {
        private readonly TestDeviceOptions _testDevice1;

        public DeviceConfigurationViewModelTest()
        {
            var configuration = new ConfigurationBuilder()
               .SetBasePath(Directory.GetCurrentDirectory())
               .AddJsonFile(@"appsettings.Test.json", false, false)
               .AddEnvironmentVariables()
               .Build();

            _testDevice1 = new TestDeviceOptions();
            configuration.GetSection("TestDevices:TestDevice1").Bind(_testDevice1);
        }

        [Fact]
        public void ViewModelTest()
        {
            // Act
            var deviceConfigurationViewModel = new DeviceConfigurationViewModel()
            {
                Id = _testDevice1.Id,
                VseType = _testDevice1.VseType,
                VseIpAddress = _testDevice1.VseIpAddress,
                VsePort = _testDevice1.VsePort,
                IoTCorePort = _testDevice1.IoTCorePort,
            };

            // Assert
            Assert.Equal(_testDevice1.Id, deviceConfigurationViewModel.Id);
            Assert.Equal(_testDevice1.VseType, deviceConfigurationViewModel.VseType);
            Assert.Equal(_testDevice1.VseIpAddress, deviceConfigurationViewModel.VseIpAddress);
            Assert.Equal(_testDevice1.VsePort, deviceConfigurationViewModel.VsePort);
            Assert.Equal(_testDevice1.IoTCorePort, deviceConfigurationViewModel.IoTCorePort);
        }

        public void Dispose()
        {
        }
    }
}