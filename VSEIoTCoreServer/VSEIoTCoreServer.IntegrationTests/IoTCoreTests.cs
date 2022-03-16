using AutoMapper;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VSEIoTCoreServer.DAL;
using VSEIoTCoreServer.DAL.Models;
using VSEIoTCoreServer.Services;
using VSEIoTCoreServer.ViewModels;
using Xunit;
using MockQueryable.Moq;
using System.Net.Http;
using System.Threading;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Configuration;
using System.IO;
using Microsoft.Extensions.Options;
using VSEIoTCoreServer.Helpers;
using System;
using VSEIoTCoreServer.DAL.Models.Enums;
using VSEIoTCoreServer.ExtensionMethods;
using VSEIoTCoreServer.CommonTestUtils;

namespace VSEIoTCoreServer.IntegrationTests
{
    [Collection("Sequential")]
    public class IoTCoreTests : IDisposable
    {
        private readonly TestDeviceOptions _testDevice1;
        private readonly TestDeviceOptions _testDevice2;
        private readonly IOptions<IoTCoreOptions> _iotCoreOptions;
        private DeviceConfiguration _deviceConfig1;
        private DeviceConfiguration _deviceConfig2;
        private IMapper _mapper;
        private NullLoggerFactory _nullLoggerFactory;
        private Mock<SQLiteDbContext> _mockDbContext;
        private DeviceConfigurationService _deviceConfigService;
        private IoTCoreService _iotCoreService;

        public IoTCoreTests()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(@"appsettings.Test.json", false, false)
                .AddEnvironmentVariables()
                .Build();

            var options = new IoTCoreOptions();
            configuration.GetSection(IoTCoreOptions.IoTCoreSettings).Bind(options);

            _iotCoreOptions = Options.Create<IoTCoreOptions>(options);

            _testDevice1 = new TestDeviceOptions();
            configuration.GetSection("TestDevices:TestDevice1").Bind(_testDevice1);
            _testDevice2 = new TestDeviceOptions();
            configuration.GetSection("TestDevices:TestDevice2").Bind(_testDevice2);
        }

        [Fact]
        public async Task GetAllDevicesTest()
        {
            // Arrange 
            Arrange();

            // Act
            var devices = await _deviceConfigService.GetAll();

            // Assert
            Assert.NotNull(devices);
            Assert.NotEmpty(devices);
            Assert.Equal(2, devices.Count());

            var deviceConfig1 = devices[0];
            Assert.Equal(_deviceConfig1.Id, deviceConfig1.Id);
            Assert.Equal(_deviceConfig1.VseType, deviceConfig1.VseType);
            Assert.Equal(_deviceConfig1.VseIpAddress, deviceConfig1.VseIpAddress);
            Assert.Equal(_deviceConfig1.VsePort, deviceConfig1.VsePort);
            Assert.Equal(_deviceConfig1.IoTCorePort, deviceConfig1.IoTCorePort);

            var deviceConfig2 = devices[1];
            Assert.Equal(_deviceConfig2.Id, deviceConfig2.Id);
            Assert.Equal(_deviceConfig2.VseType, deviceConfig2.VseType);
            Assert.Equal(_deviceConfig2.VseIpAddress, deviceConfig2.VseIpAddress);
            Assert.Equal(_deviceConfig2.VsePort, deviceConfig2.VsePort);
            Assert.Equal(_deviceConfig2.IoTCorePort, deviceConfig2.IoTCorePort);
        }

        [Fact]
        public async Task StartAndStopDeviceTest()
        {
            // Arrange
            Arrange();

            var mockDeviceConfigurationService = new Mock<IDeviceConfigurationService>();
            mockDeviceConfigurationService.Setup(x => x.GetById(It.IsAny<int>()))
                .Returns(Task.FromResult(new DeviceConfigurationViewModel()
                {
                    Id = _deviceConfig1.Id,
                    VseType = _deviceConfig1.VseType,
                    VseIpAddress = _deviceConfig1.VseIpAddress,
                    VsePort = _deviceConfig1.VsePort,
                    IoTCorePort = _deviceConfig1.IoTCorePort
                }));
            _iotCoreService = new IoTCoreService(mockDeviceConfigurationService.Object, _nullLoggerFactory, _iotCoreOptions);

            // Act
            await AssertedStart(_iotCoreService, _deviceConfig1.Id, _iotCoreOptions.Value.IoTCoreURI, _deviceConfig1.IoTCorePort);

            using (var client = new Client(_iotCoreOptions.Value.IoTCoreURI + ":" + _deviceConfig1.IoTCorePort))
            {
                var result = await client.SendRequestAndAwaitResponseAsync(IoTCoreRoutes.Device().Status().GetData());
                var message = IoTCoreUtils.CreateResponseMessage(result);
                var deviceStatus = message.Data.GetDeviceStatus();

                // Assert
                Assert.NotNull(message);
                Assert.Equal(200, message.Code);
                Assert.True(deviceStatus == DeviceStatus.Connected || deviceStatus == DeviceStatus.Connecting);
            }

            // Act
            await AssertedStop(_iotCoreService, _deviceConfig1.Id, _iotCoreOptions.Value.IoTCoreURI, _deviceConfig1.IoTCorePort);

            using (var client = new Client(_iotCoreOptions.Value.IoTCoreURI + ":" + _deviceConfig1.IoTCorePort))
            {
                // Assert
                await Assert.ThrowsAsync<HttpRequestException>(async () =>
                {
                    try
                    {
                        var response = await client.SendRequestAndAwaitResponseAsync(IoTCoreRoutes.Device().Status().GetData());
                    }
                    catch (HttpRequestException ex)
                    {
                        Assert.Null(ex.StatusCode);
                        throw ex;
                    }
                });
            }
        }

        [Fact]
        public async Task GetDeviceStatusTest()
        {
            // Arrange 
            Arrange();

            // Act
            await AssertedStart(_iotCoreService, _deviceConfig1.Id, _iotCoreOptions.Value.IoTCoreURI, _deviceConfig1.IoTCorePort);

            // Assert
            var device1 = await _iotCoreService.Status(_deviceConfig1.Id);

            Assert.NotNull(device1);
            Assert.Equal(IoTStatus.Running, device1.IoTStatus);
            Assert.True(device1.DeviceStatus == DeviceStatus.Connected || device1.DeviceStatus == DeviceStatus.Connecting);

            var device2 = await _iotCoreService.Status(_deviceConfig2.Id);

            Assert.NotNull(device2);
            Assert.Equal(IoTStatus.Stopped, device2.IoTStatus);
            Assert.Equal(DeviceStatus.Disconnected, device2.DeviceStatus);

            // Act 
            await AssertedStop(_iotCoreService, _deviceConfig1.Id, _iotCoreOptions.Value.IoTCoreURI, _deviceConfig1.IoTCorePort);

            // Assert
            device1 = await _iotCoreService.Status(_deviceConfig1.Id);

            Assert.NotNull(device1);
            Assert.Equal(IoTStatus.Stopped, device1.IoTStatus);
            Assert.Equal(DeviceStatus.Disconnected, device1.DeviceStatus);
        }

        private void Arrange()
        {
            _deviceConfig1 = new DeviceConfiguration()
            {
                Id = _testDevice1.Id,
                VseType = _testDevice1.VseType,
                VseIpAddress = _testDevice1.VseIpAddress,
                VsePort = _testDevice1.VsePort,
                IoTCorePort = _testDevice1.IoTCorePort
            };

            _deviceConfig2 = new DeviceConfiguration()
            {
                Id = _testDevice2.Id,
                VseType = _testDevice2.VseType,
                VseIpAddress = _testDevice2.VseIpAddress,
                VsePort = _testDevice2.VsePort,
                IoTCorePort = _testDevice2.IoTCorePort
            };

            var myProfile = new AutoMapperProfile();
            var configuration = new MapperConfiguration(cfg => cfg.AddProfile(myProfile));
            _mapper = new Mapper(configuration);

            _nullLoggerFactory = new NullLoggerFactory();

            var deviceConfigurations = new List<DeviceConfiguration>()
            {
                _deviceConfig1,
                _deviceConfig2
            };

            var mockDbSet = deviceConfigurations.AsQueryable().BuildMockDbSet();

            _mockDbContext = new Mock<SQLiteDbContext>();
            _mockDbContext.Setup(x => x.DeviceConfigurations).Returns(mockDbSet.Object);

            _deviceConfigService = new DeviceConfigurationService(_mapper, _mockDbContext.Object, _nullLoggerFactory);

            _iotCoreService = new IoTCoreService(_deviceConfigService, _nullLoggerFactory, _iotCoreOptions);
        }

        public async void Dispose()
        {
            await AssertedStop(_iotCoreService, _deviceConfig1.Id, _iotCoreOptions.Value.IoTCoreURI, _deviceConfig1.IoTCorePort);
            await AssertedStop(_iotCoreService, _deviceConfig2.Id, _iotCoreOptions.Value.IoTCoreURI, _deviceConfig2.IoTCorePort);
            _nullLoggerFactory = null;
            _mockDbContext = null;
            _deviceConfigService = null;
            _iotCoreService = null;
        }

        private async Task AssertedStart(IIoTCoreService iotCoreService, int deviceId, string iotCoreUri, int iotCorePort)
        {
            await iotCoreService.Start(deviceId);

            // Wait for the IoTCore to start
            bool started = await IoTCoreUtils.WaitUntilVSEIoTCoreStarted(iotCoreUri, iotCorePort);
            Assert.True(started);
        }

        private async Task AssertedStop(IIoTCoreService iotCoreService, int deviceId, string iotCoreUri, int iotCorePort)
        {
            await iotCoreService.Stop(deviceId);

            // Wait for the IoTCore to stop
            bool stopped = await IoTCoreUtils.WaitUntilVSEIoTCoreStopped(iotCoreUri, iotCorePort);
            Assert.True(stopped);
        }
    }
}
