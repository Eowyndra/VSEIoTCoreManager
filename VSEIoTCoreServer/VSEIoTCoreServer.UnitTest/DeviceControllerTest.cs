// ----------------------------------------------------------------------------
// Filename: DeviceControllerTest.cs
// Copyright (c) 2022 ifm diagnostic GmbH - All rights reserved.
// ----------------------------------------------------------------------------
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace VSEIoTCoreServer.UnitTest
{
    using System;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Moq;
    using VSEIoTCoreServer.WebApp;
    using VSEIoTCoreServer.WebApp.Controllers;
    using VSEIoTCoreServer.WebApp.Services;
    using Xunit;

    [Collection("Sequential")]
    public class DeviceControllerTest : IDisposable
    {
        private readonly IDeviceConfigurationService _deviceConfigurationServiceMock;
        private readonly IIoTCoreService _iotCoreServiceMock;
        private readonly IGlobalIoTCoreService _globalIoTCoreServiceMock;
        private readonly ILoggerFactory _loggerFactoryMock;
        private readonly IOptions<IoTCoreOptions> _iotCoreOptionsMock;

        public DeviceControllerTest()
        {
            _deviceConfigurationServiceMock = new Mock<IDeviceConfigurationService>().Object;
            _iotCoreServiceMock = new Mock<IIoTCoreService>().Object;
            _globalIoTCoreServiceMock = new Mock<IGlobalIoTCoreService>().Object;
            _loggerFactoryMock = new Mock<ILoggerFactory>().Object;
            _iotCoreOptionsMock = Options.Create(new IoTCoreOptions());
        }

        [Fact]
        public void Ctor_Test()
        {
            Assert.NotNull(new DeviceController(
                _deviceConfigurationServiceMock,
                _iotCoreServiceMock,
                _globalIoTCoreServiceMock,
                _loggerFactoryMock,
                _iotCoreOptionsMock));
        }

        [Fact]
        public void Ctor_DeviceConfigService_Null_Error_Test()
        {
            Assert.Throws<ArgumentNullException>("deviceConfigurationService", () => new DeviceController(
                null,
                _iotCoreServiceMock,
                _globalIoTCoreServiceMock,
                _loggerFactoryMock,
                _iotCoreOptionsMock));
        }

        [Fact]
        public void Ctor_IoTCoreService_Null_Error_Test()
        {
            Assert.Throws<ArgumentNullException>("iotCoreService", () => new DeviceController(
                _deviceConfigurationServiceMock,
                null,
                _globalIoTCoreServiceMock,
                _loggerFactoryMock,
                _iotCoreOptionsMock));
        }

        [Fact]
        public void Ctor_GlobalIoTCoreService_Null_Error_Test()
        {
            Assert.Throws<ArgumentNullException>("globalIoTCoreService", () => new DeviceController(
                _deviceConfigurationServiceMock,
                _iotCoreServiceMock,
                null,
                _loggerFactoryMock,
                _iotCoreOptionsMock));
        }

        [Fact]
        public void Ctor_Logger_Null_Error_Test()
        {
            Assert.Throws<ArgumentNullException>("loggerFactory", () => new DeviceController(
                _deviceConfigurationServiceMock,
                _iotCoreServiceMock,
                _globalIoTCoreServiceMock,
                null,
                _iotCoreOptionsMock));
        }

        [Fact]
        public void Ctor_IoTCoreOptions_Null_Error_Test()
        {
            Assert.Throws<ArgumentNullException>("iotCoreOptions", () => new DeviceController(
                _deviceConfigurationServiceMock,
                _iotCoreServiceMock,
                _globalIoTCoreServiceMock,
                _loggerFactoryMock,
                null));
        }

        public void Dispose()
        {
        }
    }
}
