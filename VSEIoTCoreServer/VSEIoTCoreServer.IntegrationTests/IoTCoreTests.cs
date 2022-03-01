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
using Newtonsoft.Json.Linq;
using ifmIoTCore.Messages;
using System.Net;

namespace VSEIoTCoreServer.IntegrationTests
{
    [Collection("Sequential")]
    public class IoTCoreTests
    {
        private readonly TestDeviceOptions _testDevice1;
        private readonly TestDeviceOptions _testDevice2;
        private readonly IOptions<IoTCoreOptions> _iotCoreOptions;
        private DeviceConfiguration _deviceConfig1;
        private DeviceConfiguration _deviceConfig2;
        private IMapper _mapper;
        private NullLoggerFactory _nullLoggerFactory;
        private Mock<SQLiteDbContext> _mockDbContext;

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
        }

        [Fact]
        public async Task GetAllDevicesTest()
        {
            // Arrange 
            Arrange();

            // Act
            var deviceConfigurationService = new DeviceConfigurationService(_mapper, _mockDbContext.Object, _nullLoggerFactory);

            var devices = await deviceConfigurationService.GetAll();

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
            var iotCoreService = new IoTCoreService(mockDeviceConfigurationService.Object, _nullLoggerFactory, _iotCoreOptions);

            // Act
            await iotCoreService.Start(_deviceConfig1.Id);

            // Wait for the asynchronous call to finish
            Thread.Sleep(5000);

            using (var client = new Client(_iotCoreOptions.Value.IoTCoreURI + ":" + _deviceConfig1.IoTCorePort))
            {
                var result = await client.SendRequestAndAwaitResponseAsync("/Device/Status/getdata");
                var message = TestUtils.CreateResponseMessage(result);
                var status = message.Data["value"]["ConnectionState"];

                // Assert
                Assert.NotNull(message);
                Assert.Equal(200, message.Code);
                Assert.True(status.ToString() == "connected" || status.ToString() == "connecting");
            }

            // Act
            await iotCoreService.Stop(_deviceConfig1.Id);

            // Wait for the asynchronous call to finish
            Thread.Sleep(5000);

            using (var client = new Client(_iotCoreOptions.Value.IoTCoreURI + ":" + _deviceConfig1.IoTCorePort))
            {
                // Assert
                await Assert.ThrowsAsync<HttpRequestException>(async () =>
                {
                    try
                    {
                        var response = await client.SendRequestAndAwaitResponseAsync("/Device/Status/getdata");
                    }
                    catch (HttpRequestException ex)
                    {
                        Assert.Null(ex.StatusCode);
                        throw ex;
                    }
                });
            }
        }
    }
}
