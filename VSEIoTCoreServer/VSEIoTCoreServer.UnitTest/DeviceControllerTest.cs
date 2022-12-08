// ----------------------------------------------------------------------------
// Filename: DeviceControllerTest.cs
// Copyright (c) 2022 ifm diagnostic GmbH - All rights reserved.
// ----------------------------------------------------------------------------
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace VSEIoTCoreServer.UnitTests
{
    using System;
    using Microsoft.Extensions.Logging;
    using Moq;
    using VSEIoTCoreServer.WebApp.Controllers;
    using VSEIoTCoreServer.WebApp.Models;
    using VSEIoTCoreServer.WebApp.Services;
    using Xunit;

    public class DeviceControllerTest : IDisposable
    {
        private readonly IDeviceConfigurationService _deviceConfigurationServiceMock;
        private readonly ILoggerFactory _loggerFactoryMock;
        private readonly IIoTCoreServer _iotCoreServerMock;

        public DeviceControllerTest()
        {
            _deviceConfigurationServiceMock = new Mock<IDeviceConfigurationService>().Object;
            _loggerFactoryMock = new Mock<ILoggerFactory>().Object;
            _iotCoreServerMock = new Mock<IIoTCoreServer>().Object;
        }

        [Fact]
        public void Ctor_Test()
        {
            Assert.NotNull(new DeviceController(_deviceConfigurationServiceMock, _loggerFactoryMock, _iotCoreServerMock));
        }

        [Fact]
        public void Ctor_DeviceConfigService_Null_Error_Test()
        {
            Assert.Throws<ArgumentNullException>("deviceConfigurationService", () => new DeviceController(null, _loggerFactoryMock, _iotCoreServerMock));
        }

        [Fact]
        public void Ctor_Logger_Null_Error_Test()
        {
            Assert.Throws<ArgumentNullException>("loggerFactory", () => new DeviceController(_deviceConfigurationServiceMock, null, _iotCoreServerMock));
        }

        [Fact]
        public void Ctor_IoTCoreServer_Null_Error_Test()
        {
            Assert.Throws<ArgumentNullException>("iotCoreServer", () => new DeviceController(_deviceConfigurationServiceMock, _loggerFactoryMock, null));
        }

        public void Dispose()
        {
        }
    }
}
