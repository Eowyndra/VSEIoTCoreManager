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
    public class IoTCoreTests
    {
        private readonly IOptions<IoTCoreOptions> _iotCoreOptions;

        private readonly DeviceConfiguration _deviceConfig1 = new DeviceConfiguration()
        {
            Id = 1,
            VseType = "VSE100",
            VseIpAddress = "172.29.12.12",
            VsePort = 3321,
            IoTCorePort = 8092
        };

        private readonly DeviceConfiguration _deviceConfig2 = new DeviceConfiguration()
        {
            Id = 2,
            VseType = "VSE100",
            VseIpAddress = "172.29.12.13",
            VsePort = 3321,
            IoTCorePort = 8093
        };

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
        }

        [Fact]
        public async Task GetAllDevicesTest()
        {
            // Arrange 
            var myProfile = new AutoMapperProfile();
            var configuration = new MapperConfiguration(cfg => cfg.AddProfile(myProfile));
            var mapper = new Mapper(configuration);

            var nullLoggerFactory = new NullLoggerFactory();

            var deviceConfigurations = new List<DeviceConfiguration>()
            {
                _deviceConfig1,
                _deviceConfig2
            };

            var mockDbSet = deviceConfigurations.AsQueryable().BuildMockDbSet();

            var mockDbContext = new Mock<SQLiteDbContext>();
            mockDbContext.Setup(x => x.DeviceConfigurations).Returns(mockDbSet.Object);

            // Act
            var deviceConfigurationService = new DeviceConfigurationService(mapper, mockDbContext.Object);

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
            var nullLoggerFactory = new NullLoggerFactory();

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


            // Act
            var iotCoreService = new IoTCoreService(mockDeviceConfigurationService.Object, nullLoggerFactory, _iotCoreOptions);

            await iotCoreService.Start(_deviceConfig1.Id);
            Thread.Sleep(5000);

            using (var client = new Client("http://127.0.0.1:" + _deviceConfig1.IoTCorePort))
            {
                var result = await client.SendRequestAndAwaitResponseAsync("/Device/Status/getdata");
                var message = ifmIoTCore.Elements.ServiceData.ServiceDataBase.FromJson<ResponseMessage>(JToken.Parse(result));
                var status = message.Data["value"]["ConnectionState"];

                // Assert
                Assert.NotNull(message);
                Assert.Equal(200, message.Code);
                Assert.True(status.ToString() == "connected" || status.ToString() == "connecting");
            }



            // stop the IoTCore process for each configured device and assert that it is no longer reachable

            await iotCoreService.Stop(_deviceConfig1.Id);
            Thread.Sleep(5000);

            using (var client = new Client("http://127.0.0.1:" + _deviceConfig1.IoTCorePort))
            {
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
