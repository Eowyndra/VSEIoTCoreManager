using Xunit;
using Moq;
using VSEIoTCoreServer.Services;
using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using VSEIoTCoreServer.LibraryRuntime;

namespace VSEIoTCoreServer.UnitTest
{
    public class GlobalIoTCoreServiceTest
    {
        [Fact]
        public void Ctor_Test()
        {
            var deviceConfiguratonServiceMock = new Mock<IDeviceConfigurationService>().Object;
            var iotCoreServiceMock = new Mock<IIoTCoreService>().Object;
            var iotCoreRuntimeMock = new Mock<IIoTCoreRuntime>().Object;
            var loggerFactoryMock = new Mock<ILoggerFactory>().Object;
            var iotCoreOptionsMock = Options.Create(new IoTCoreOptions
            {
                AdapterLocation = "..\\..\\..\\..\\..\\VSEIoTCore\\win-x64\\vse-iotcore-adapter-process.exe",
                IoTCoreURI = "http://127.0.0.1",
                GlobalIoTCorePort = 8090
            });
            Assert.NotNull(new GlobalIoTCoreService(deviceConfiguratonServiceMock, iotCoreServiceMock, iotCoreRuntimeMock, loggerFactoryMock, iotCoreOptionsMock));
        }

        [Fact]
        public void Ctor_DeviceConfigService_Null_Error_Test()
        {
            var iotCoreServiceMock = new Mock<IIoTCoreService>().Object;
            var iotCoreRuntimeMock = new Mock<IIoTCoreRuntime>().Object;
            var loggerFactoryMock = new Mock<ILoggerFactory>().Object;
            var iotCoreOptionsMock = Options.Create<IoTCoreOptions>(new IoTCoreOptions());
            Assert.Throws<ArgumentNullException>("deviceConfigurationService", () => new GlobalIoTCoreService(null, iotCoreServiceMock, iotCoreRuntimeMock, loggerFactoryMock, iotCoreOptionsMock));
        }


        [Fact]
        public void Ctor_IoTCoreService_Null_Error_Test()
        {
            var deviceConfiguratonServiceMock = new Mock<IDeviceConfigurationService>().Object;
            var iotCoreRuntimeMock = new Mock<IIoTCoreRuntime>().Object;
            var loggerFactoryMock = new Mock<ILoggerFactory>().Object;
            var iotCoreOptionsMock = Options.Create<IoTCoreOptions>(new IoTCoreOptions());
            Assert.Throws<ArgumentNullException>("iotCoreService", () => new GlobalIoTCoreService(deviceConfiguratonServiceMock, null, iotCoreRuntimeMock, loggerFactoryMock, iotCoreOptionsMock));
        }

        [Fact]
        public void Ctor_IoTCoreRuntime_Null_Error_Test()
        {
            var deviceConfiguratonServiceMock = new Mock<IDeviceConfigurationService>().Object;
            var iotCoreServiceMock = new Mock<IIoTCoreService>().Object;
            var loggerFactoryMock = new Mock<ILoggerFactory>().Object;
            var iotCoreOptionsMock = Options.Create<IoTCoreOptions>(new IoTCoreOptions());
            Assert.Throws<ArgumentNullException>("iotCoreRuntime", () => new GlobalIoTCoreService(deviceConfiguratonServiceMock, iotCoreServiceMock, null, loggerFactoryMock, iotCoreOptionsMock));
        }

        [Fact]
        public void Ctor_LoggerFactory_Null_Error_Test()
        {
            var deviceConfiguratonServiceMock = new Mock<IDeviceConfigurationService>().Object;
            var iotCoreServiceMock = new Mock<IIoTCoreService>().Object;
            var iotCoreRuntimeMock = new Mock<IIoTCoreRuntime>().Object;
            var iotCoreOptionsMock = Options.Create<IoTCoreOptions>(new IoTCoreOptions());
            Assert.Throws<ArgumentNullException>("loggerFactory", () => new GlobalIoTCoreService(deviceConfiguratonServiceMock, iotCoreServiceMock, iotCoreRuntimeMock, null, iotCoreOptionsMock));
        }

        [Fact]
        public void Ctor_IoTCoreOptions_Null_Error_Test()
        {
            var deviceConfiguratonServiceMock = new Mock<IDeviceConfigurationService>().Object;
            var iotCoreServiceMock = new Mock<IIoTCoreService>().Object;
            var iotCoreRuntimeMock = new Mock<IIoTCoreRuntime>().Object;
            var loggerFactoryMock = new Mock<ILoggerFactory>().Object;
            Assert.Throws<ArgumentNullException>("iotCoreOptions", () => new GlobalIoTCoreService(deviceConfiguratonServiceMock, iotCoreServiceMock, iotCoreRuntimeMock, loggerFactoryMock, null));
        }
    }
}