using Xunit;
using Moq;
using VSEIoTCoreServer.Services;
using VSEIoTCoreServer.Controllers;
using System;
using Microsoft.Extensions.Logging;

namespace VSEIoTCoreServer.UnitTest
{
    public class DeviceControllerTest
    {
        [Fact]
        public void Ctor_Test()
        {
            var deviceConfigurationServiceMock = new Mock<IDeviceConfigurationService>().Object;
            var iotCoreServiceMock = new Mock<IIoTCoreService>().Object;
            var loggerFactoryMock = new Mock<ILoggerFactory>().Object;
            Assert.NotNull(new DeviceController(deviceConfigurationServiceMock, iotCoreServiceMock, loggerFactoryMock));
        }

        [Fact]
        public void Ctor_DeviceConfigService_Null_Error_Test()
        {
            var iotCoreServiceMock = new Mock<IIoTCoreService>().Object;
            var loggerFactoryMock = new Mock<ILoggerFactory>().Object;
            Assert.Throws<ArgumentNullException>("deviceConfigurationService", () => new DeviceController(null, iotCoreServiceMock, loggerFactoryMock));
        }

        [Fact]
        public void Ctor_IoTCoreService_Null_Error_Test()
        {
            var deviceConfigurationServiceMock = new Mock<IDeviceConfigurationService>().Object;
            var loggerFactoryMock = new Mock<ILoggerFactory>().Object;
            Assert.Throws<ArgumentNullException>("iotCoreService", () => new DeviceController(deviceConfigurationServiceMock, null, loggerFactoryMock));
        }

        [Fact]
        public void Ctor_Logger_Null_Error_Test()
        {
            var iotCoreServiceMock = new Mock<IIoTCoreService>().Object;
            var deviceConfigurationServiceMock = new Mock<IDeviceConfigurationService>().Object;
            Assert.Throws<ArgumentNullException>("loggerFactory", () => new DeviceController(deviceConfigurationServiceMock, iotCoreServiceMock, null));
        }


    }
}