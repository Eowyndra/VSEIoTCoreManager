using Xunit;
using Moq;
using VSEIoTCoreServer.Services;
using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace VSEIoTCoreServer.IntegrationTests
{
    public class IoTCoreServiceTest
    {
        [Fact]
        public void Ctor_Test()
        {
            var deviceConfiguratonServiceMock = new Mock<IDeviceConfigurationService>().Object;
            var loggerFactoryMock = new Mock<ILoggerFactory>().Object;
            var iotCoreOptionsMock = Options.Create<IoTCoreOptions>(new IoTCoreOptions());
            Assert.NotNull(new IoTCoreService(deviceConfiguratonServiceMock, loggerFactoryMock, iotCoreOptionsMock));
        }

        [Fact]
        public void Ctor_DeviceConfigService_Null_Error_Test()
        {
            var loggerFactoryMock = new Mock<ILoggerFactory>().Object;
            var iotCoreOptionsMock = Options.Create<IoTCoreOptions>(new IoTCoreOptions());
            Assert.Throws<ArgumentNullException>("deviceConfigurationService", () => new IoTCoreService(null, loggerFactoryMock, iotCoreOptionsMock));
        }

        [Fact]
        public void Ctor_LoggerFactory_Null_Error_Test()
        {
            var deviceConfiguratonServiceMock = new Mock<IDeviceConfigurationService>().Object;
            var iotCoreOptionsMock = Options.Create<IoTCoreOptions>(new IoTCoreOptions());
            Assert.Throws<ArgumentNullException>("loggerFactory", () => new IoTCoreService(deviceConfiguratonServiceMock, null, iotCoreOptionsMock));
        }

        [Fact]
        public void Ctor_IoTCoreOptions_Null_Error_Test()
        {
            var deviceConfiguratonServiceMock = new Mock<IDeviceConfigurationService>().Object;
            var loggerFactoryMock = new Mock<ILoggerFactory>().Object;
            Assert.Throws<ArgumentNullException>("iotCoreOptions", () => new IoTCoreService(deviceConfiguratonServiceMock, loggerFactoryMock, null));
        }
    }
}