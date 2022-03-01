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
            var loggerFactoryMock = new Mock<ILoggerFactory>().Object;
            Assert.NotNull(new DeviceController(deviceConfigurationServiceMock, loggerFactoryMock));
        }

        [Fact]
        public void Ctor_Service_Null_Error_Test()
        {
            var loggerFactoryMock = new Mock<ILoggerFactory>().Object;
            Assert.Throws<ArgumentNullException>("deviceConfigurationService", () => new DeviceController(null, loggerFactoryMock));
        }

        [Fact]
        public void Ctor_Logger_Null_Error_Test()
        {
            var deviceConfigurationServiceMock = new Mock<IDeviceConfigurationService>().Object;
            Assert.Throws<ArgumentNullException>("loggerFactory", () => new DeviceController(deviceConfigurationServiceMock, null));
        }


    }
}