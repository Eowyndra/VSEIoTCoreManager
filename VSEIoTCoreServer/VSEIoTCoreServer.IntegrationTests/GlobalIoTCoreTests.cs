using AutoMapper;
using ifmIoTCore.Messages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using MockQueryable.Moq;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using VSEIoTCoreServer.DAL;
using VSEIoTCoreServer.DAL.Models;
using VSEIoTCoreServer.Services;
using VSEIoTCoreServer.ViewModels;
using Xunit;
using System.Linq;
using System.Net.Http;
using System.Diagnostics;
using VSEIoTCoreServer.LibraryRuntime;

namespace VSEIoTCoreServer.IntegrationTests
{
    [Collection("Sequential")]
    public class GlobalIoTCoreTests
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
        private IIoTCoreRuntime _iotCoreRuntime;

        public GlobalIoTCoreTests()
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

            _deviceConfigService = new DeviceConfigurationService(_mapper, _mockDbContext.Object, _nullLoggerFactory);
            _iotCoreService = new IoTCoreService(_deviceConfigService, _nullLoggerFactory, _iotCoreOptions);
            _iotCoreRuntime = new IoTCoreRuntime();
        }

        [Fact]
        public async Task StartGlobalIoTCoreTest()
        {
            // Arrange
            Arrange();

            // Act - start the global IoTCore and all individual VSEIoTCores and mirror them into the global IoTCore
            var globalIoTCoreService = new GlobalIoTCoreService(_deviceConfigService, _iotCoreService, _iotCoreRuntime, _nullLoggerFactory, _iotCoreOptions);
            await globalIoTCoreService.Start();

            // Wait for the asynchronous call to finish
            Thread.Sleep(5000);

            // Assert
            using (var globalClient = new Client(_iotCoreOptions.Value.IoTCoreURI + ":" + _iotCoreOptions.Value.GlobalIoTCorePort))
            using (var vseClient1 = new Client(_iotCoreOptions.Value.IoTCoreURI + ":" + _deviceConfig1.IoTCorePort))
            using (var vseClient2 = new Client(_iotCoreOptions.Value.IoTCoreURI + ":" + _deviceConfig2.IoTCorePort))
            {
                // Request data from independent VSEIoTCore1
                var deviceInfo1 = await vseClient1.SendRequestAndAwaitResponseAsync("/Device/Information/Device/getdata");
                var deviceInfoMessage1 = TestUtils.CreateResponseMessage(deviceInfo1);
                var objectData1 = await vseClient1.SendRequestAndAwaitResponseAsync("/Device/Objects/Object0/getdata");
                var objectDataMessage1 = TestUtils.CreateResponseMessage(objectData1);
                var counterData1 = await vseClient1.SendRequestAndAwaitResponseAsync("/Device/Counters/Counter0/getdata");
                var counterDataMessage1 = TestUtils.CreateResponseMessage(counterData1);

                // Request data from independent VSEIoTCore2
                var deviceInfo2 = await vseClient2.SendRequestAndAwaitResponseAsync("/Device/Information/Device/getdata");
                var deviceInfoMessage2 = TestUtils.CreateResponseMessage(deviceInfo2);
                var objectData2 = await vseClient2.SendRequestAndAwaitResponseAsync("/Device/Objects/Object0/getdata");
                var objectDataMessage2 = TestUtils.CreateResponseMessage(objectData2);
                var counterData2 = await vseClient2.SendRequestAndAwaitResponseAsync("/Device/Counters/Counter0/getdata");
                var counterDataMessage2 = TestUtils.CreateResponseMessage(counterData2);

                // Request data from global IoTCore
                var globalTree = await globalClient.SendRequestAndAwaitResponseAsync("/gettree");
                var globalMessage = TestUtils.CreateResponseMessage(globalTree);

                // Request data from VSEIoTCore1 via global IoTCore remote mirror
                var remoteDeviceInfo1 = await globalClient.SendRequestAndAwaitResponseAsync("/remote/1/Device/Information/Device/getdata");
                var remoteDeviceInfoMessage1 = TestUtils.CreateResponseMessage(remoteDeviceInfo1);
                var remoteObjectData1 = await globalClient.SendRequestAndAwaitResponseAsync("/remote/1/Device/Objects/Object0/getdata");
                var remoteObjectDataMessage1 = TestUtils.CreateResponseMessage(remoteObjectData1);
                var remoteCounterData1 = await globalClient.SendRequestAndAwaitResponseAsync("/remote/1/Device/Counters/Counter0/getdata");
                var remoteCounterDataMessage1 = TestUtils.CreateResponseMessage(remoteCounterData1);

                // Request data from VSEIoTCore2 via global IoTCore remote mirror
                var remoteDeviceInfo2 = await globalClient.SendRequestAndAwaitResponseAsync("/remote/2/Device/Information/Device/getdata");
                var remoteDeviceInfoMessage2 = TestUtils.CreateResponseMessage(remoteDeviceInfo2);
                var remoteObjectData2 = await globalClient.SendRequestAndAwaitResponseAsync("/remote/2/Device/Objects/Object0/getdata");
                var remoteObjectDataMessage2 = TestUtils.CreateResponseMessage(remoteObjectData2);
                var remoteCounterData2 = await globalClient.SendRequestAndAwaitResponseAsync("/remote/2/Device/Counters/Counter0/getdata");
                var remoteCounterDataMessage2 = TestUtils.CreateResponseMessage(remoteCounterData2);

                // Assert - VSEIoTCore1 is reachable
                Assert.NotNull(deviceInfoMessage1);
                Assert.Equal(200, deviceInfoMessage1.Code);

                // Assert - VSEIoTCore2 is reachable
                Assert.NotNull(deviceInfoMessage2);
                Assert.Equal(200, deviceInfoMessage2.Code);

                // Assert - global IoTCore is reachable
                Assert.NotNull(globalMessage);
                Assert.Equal(200, globalMessage.Code);

                // Assert - mirrored remote VSEIoTCore1 is reachable
                Assert.NotNull(remoteDeviceInfoMessage1);
                Assert.Equal(200, remoteDeviceInfoMessage1.Code);

                // Assert - mirrored remote VSEIoTCore2 is reachable
                Assert.NotNull(remoteDeviceInfoMessage2);
                Assert.Equal(200, remoteDeviceInfoMessage2.Code);

                // Assert - deviceInfo data of remote VSEIoTCore1 matches deviceInfo data of VSEIoTCore1
                Assert.Equal(deviceInfoMessage1.Data["value"]["Name"], remoteDeviceInfoMessage1.Data["value"]["Name"]);
                Assert.Equal(deviceInfoMessage1.Data["value"]["Type"], remoteDeviceInfoMessage1.Data["value"]["Type"]);
                Assert.Equal(deviceInfoMessage1.Data["value"]["Revision"], remoteDeviceInfoMessage1.Data["value"]["Revision"]);
                Assert.Equal(deviceInfoMessage1.Data["value"]["Serial"], remoteDeviceInfoMessage1.Data["value"]["Serial"]);
                Assert.Equal(deviceInfoMessage1.Data["value"]["Firmware"], remoteDeviceInfoMessage1.Data["value"]["Firmware"]);

                // Assert - deviceInfo data of remote VSEIoTCore2 matches deviceInfo data of VSEIoTCore2
                Assert.Equal(deviceInfoMessage2.Data["value"]["Name"], remoteDeviceInfoMessage2.Data["value"]["Name"]);
                Assert.Equal(deviceInfoMessage2.Data["value"]["Type"], remoteDeviceInfoMessage2.Data["value"]["Type"]);
                Assert.Equal(deviceInfoMessage2.Data["value"]["Revision"], remoteDeviceInfoMessage2.Data["value"]["Revision"]);
                Assert.Equal(deviceInfoMessage2.Data["value"]["Serial"], remoteDeviceInfoMessage2.Data["value"]["Serial"]);
                Assert.Equal(deviceInfoMessage2.Data["value"]["Firmware"], remoteDeviceInfoMessage2.Data["value"]["Firmware"]);

                // Assert - object data of remote VSEIoTCore1 matches object data of VSEIoTCore1
                Assert.Equal(objectDataMessage1.Data["value"]["Name"], remoteObjectDataMessage1.Data["value"]["Name"]);
                Assert.Equal(objectDataMessage1.Data["value"]["Type"], remoteObjectDataMessage1.Data["value"]["Type"]);
                Assert.Equal(objectDataMessage1.Data["value"]["ID"], remoteObjectDataMessage1.Data["value"]["ID"]);
                Assert.Equal(objectDataMessage1.Data["value"]["Unit"], remoteObjectDataMessage1.Data["value"]["Unit"]);
                Assert.Equal(objectDataMessage1.Data["value"]["State"], remoteObjectDataMessage1.Data["value"]["State"]);
                Assert.Equal(objectDataMessage1.Data["value"]["Warning"], remoteObjectDataMessage1.Data["value"]["Warning"]);
                Assert.Equal(objectDataMessage1.Data["value"]["BaseLine"], remoteObjectDataMessage1.Data["value"]["BaseLine"]);
                Assert.Equal(objectDataMessage1.Data["value"]["Damage"], remoteObjectDataMessage1.Data["value"]["Damage"]);
                Assert.Equal(objectDataMessage1.Data["value"]["InputID"], remoteObjectDataMessage1.Data["value"]["InputID"]);
                Assert.Equal(objectDataMessage1.Data["value"]["InputType"], remoteObjectDataMessage1.Data["value"]["InputType"]);

                // Assert - object data of remote VSEIoTCore2 matches object data of VSEIoTCore2
                Assert.Equal(objectDataMessage2.Data["value"]["Name"], remoteObjectDataMessage2.Data["value"]["Name"]);
                Assert.Equal(objectDataMessage2.Data["value"]["Type"], remoteObjectDataMessage2.Data["value"]["Type"]);
                Assert.Equal(objectDataMessage2.Data["value"]["ID"], remoteObjectDataMessage2.Data["value"]["ID"]);
                Assert.Equal(objectDataMessage2.Data["value"]["Unit"], remoteObjectDataMessage2.Data["value"]["Unit"]);
                Assert.Equal(objectDataMessage2.Data["value"]["State"], remoteObjectDataMessage2.Data["value"]["State"]);
                Assert.Equal(objectDataMessage2.Data["value"]["Warning"], remoteObjectDataMessage2.Data["value"]["Warning"]);
                Assert.Equal(objectDataMessage2.Data["value"]["BaseLine"], remoteObjectDataMessage2.Data["value"]["BaseLine"]);
                Assert.Equal(objectDataMessage2.Data["value"]["Damage"], remoteObjectDataMessage2.Data["value"]["Damage"]);
                Assert.Equal(objectDataMessage2.Data["value"]["InputID"], remoteObjectDataMessage2.Data["value"]["InputID"]);
                Assert.Equal(objectDataMessage2.Data["value"]["InputType"], remoteObjectDataMessage2.Data["value"]["InputType"]);

                // Assert - counter data of remote VSEIoTCore1 matches object data of VSEIoTCore1
                Assert.Equal(counterDataMessage1.Data["value"]["Name"], remoteCounterDataMessage1.Data["value"]["Name"]);
                Assert.Equal(counterDataMessage1.Data["value"]["Type"], remoteCounterDataMessage1.Data["value"]["Type"]);
                Assert.Equal(counterDataMessage1.Data["value"]["Unit"], remoteCounterDataMessage1.Data["value"]["Unit"]);
                Assert.Equal(counterDataMessage1.Data["value"]["State"], remoteCounterDataMessage1.Data["value"]["State"]);
                Assert.Equal(counterDataMessage1.Data["value"]["Limit"], remoteCounterDataMessage1.Data["value"]["Limit"]);

                // Assert - counter data of remote VSEIoTCore2 matches object data of VSEIoTCore2
                Assert.Equal(counterDataMessage2.Data["value"]["Name"], remoteCounterDataMessage2.Data["value"]["Name"]);
                Assert.Equal(counterDataMessage2.Data["value"]["Type"], remoteCounterDataMessage2.Data["value"]["Type"]);
                Assert.Equal(counterDataMessage2.Data["value"]["Unit"], remoteCounterDataMessage2.Data["value"]["Unit"]);
                Assert.Equal(counterDataMessage2.Data["value"]["State"], remoteCounterDataMessage2.Data["value"]["State"]);
                Assert.Equal(counterDataMessage2.Data["value"]["Limit"], remoteCounterDataMessage2.Data["value"]["Limit"]);
            }

            // Finally - stop the global IoTCore and all instances of VSEIoTCore
            await globalIoTCoreService.Stop();

            // Wait for the asynchronous call to finish
            Thread.Sleep(5000);
        }

        [Fact]
        public async Task StopGlobalIoTCore()
        {
            // Arrange
            Arrange();

            // Act
            var globalIoTCoreService = new GlobalIoTCoreService(_deviceConfigService, _iotCoreService, _iotCoreRuntime, _nullLoggerFactory, _iotCoreOptions);
            await globalIoTCoreService.Start();

            // Wait for the asynchronous call to finish
            Thread.Sleep(5000);

            // Assert
            using (var globalClient = new Client(_iotCoreOptions.Value.IoTCoreURI + ":" + _iotCoreOptions.Value.GlobalIoTCorePort))
            using (var vseClient1 = new Client(_iotCoreOptions.Value.IoTCoreURI + ":" + _deviceConfig1.IoTCorePort))
            using (var vseClient2 = new Client(_iotCoreOptions.Value.IoTCoreURI + ":" + _deviceConfig2.IoTCorePort))
            {
                // Request data from global IoTCore
                var globalTree = await globalClient.SendRequestAndAwaitResponseAsync("/gettree");
                var globalMessage = ifmIoTCore.Elements.ServiceData.ServiceDataBase.FromJson<ResponseMessage>(JToken.Parse(globalTree));

                // Request status data from independent VSEIoTCore1
                var status1 = await vseClient1.SendRequestAndAwaitResponseAsync("/Device/Status/getdata");
                var message1 = TestUtils.CreateResponseMessage(status1);
                var statusMessage1 = message1.Data["value"]["ConnectionState"];

                // Request status data from independent VSEIoTCore2
                var status2 = await vseClient2.SendRequestAndAwaitResponseAsync("/Device/Status/getdata");
                var message2 = TestUtils.CreateResponseMessage(status2);
                var statusMessage2 = message2.Data["value"]["ConnectionState"];

                // Assert global IoTCore is reachable
                Assert.NotNull(globalMessage);
                Assert.Equal(200, globalMessage.Code);

                // Assert VSEIoTCore1 is reachable
                Assert.NotNull(message1);
                Assert.Equal(200, message1.Code);

                // Assert VSEIoTCore1 status is connected or connecting
                Assert.True(statusMessage1.ToString() == "connected" || statusMessage1.ToString() == "connecting");

                // Assert VSEIoTCore2 is reachable
                Assert.NotNull(message2);
                Assert.Equal(200, message2.Code);

                // Assert VSEIoTCore2 status is connected or connecting
                Assert.True(statusMessage2.ToString() == "connected" || statusMessage2.ToString() == "connecting");

            }

            // Act 
            await globalIoTCoreService.Stop();

            // Wait for the asynchronous call to finish
            Thread.Sleep(5000);

            // Assert
            using (var globalClient = new Client(_iotCoreOptions.Value.IoTCoreURI + ":" + _iotCoreOptions.Value.GlobalIoTCorePort))
            using (var vseClient1 = new Client(_iotCoreOptions.Value.IoTCoreURI + ":" + _deviceConfig1.IoTCorePort))
            using (var vseClient2 = new Client(_iotCoreOptions.Value.IoTCoreURI + ":" + _deviceConfig2.IoTCorePort))
            {
                // Assert global IoTCore is no longer reachable
                await Assert.ThrowsAsync<HttpRequestException>(async () =>
                {
                    try
                    {
                        var response = await globalClient.SendRequestAndAwaitResponseAsync("/gettree");
                    }
                    catch (HttpRequestException ex)
                    {
                        Assert.Null(ex.StatusCode);
                        throw ex;
                    }
                });

                // Assert VSEIoTCore1 is no longer reachable
                await Assert.ThrowsAsync<HttpRequestException>(async () =>
                {
                    try
                    {
                        var response = await vseClient1.SendRequestAndAwaitResponseAsync("/gettree");
                    }
                    catch (HttpRequestException ex)
                    {
                        Assert.Null(ex.StatusCode);
                        throw ex;
                    }
                });

                // Assert VSEIoTCore2 is no longer reachable
                await Assert.ThrowsAsync<HttpRequestException>(async () =>
                {
                    try
                    {
                        var response = await vseClient2.SendRequestAndAwaitResponseAsync("/gettree");
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
        public async Task AddMirrorTest()
        {
            // Arrange
            Arrange();

            // Mock device configuration that only contains one configured device
            var deviceConfigurations = new List<DeviceConfiguration>()
            {
                _deviceConfig1
            };

            var mockDbSet = deviceConfigurations.AsQueryable().BuildMockDbSet();

            var mockDbContext = new Mock<SQLiteDbContext>();
            mockDbContext.Setup(x => x.DeviceConfigurations).Returns(mockDbSet.Object);

            var deviceConfigurationService = new DeviceConfigurationService(_mapper, mockDbContext.Object, _nullLoggerFactory);
            var iotCoreService = new IoTCoreService(deviceConfigurationService, _nullLoggerFactory, _iotCoreOptions);
            var globalIoTCoreService = new GlobalIoTCoreService(deviceConfigurationService, iotCoreService, _iotCoreRuntime, _nullLoggerFactory, _iotCoreOptions);

            // Act 
            // start the global IoTCore and all configured VSEIoTCores - in this case only one single VSEIoTCore1 should be started
            await globalIoTCoreService.Start();

            // Wait for the asynchronous call to finish
            Thread.Sleep(5000);

            // Assert
            using (var globalClient = new Client(_iotCoreOptions.Value.IoTCoreURI + ":" + _iotCoreOptions.Value.GlobalIoTCorePort))
            {
                // Assert global IoTCore is reachable
                var globalTree = await globalClient.SendRequestAndAwaitResponseAsync("/gettree");
                var globalMessage = TestUtils.CreateResponseMessage(globalTree);
                Assert.NotNull(globalMessage);
                Assert.Equal(200, globalMessage.Code);

                // Assert remote VSEIoTCore1 is reachable
                var remote1 = await globalClient.SendRequestAndAwaitResponseAsync("/remote/1/gettree");
                var remoteMessage1 = TestUtils.CreateResponseMessage(remote1);
                Assert.NotNull(remoteMessage1);
                Assert.Equal(200, remoteMessage1.Code);

                // Assert remote VSEIoTCore2 is not found, because it has not been added yet
                var remote2 = await globalClient.SendRequestAndAwaitResponseAsync("/remote/2/gettree");
                var remoteMessage2 = TestUtils.CreateResponseMessage(remote2);
                Assert.NotNull(remoteMessage2);
                Assert.Equal(404, remoteMessage2.Code);
            }

            // Act - manually start VSEIoTCore2 
            Process process = null;
            await Task.Run(() =>
            {
                var startInfo = new ProcessStartInfo(_iotCoreOptions.Value.AdapterLocation);
                startInfo.Arguments = "--vse-ip " + _deviceConfig2.VseIpAddress +
                    " --vse-port " + _deviceConfig2.VsePort +
                    " --iotcore-uri " + _iotCoreOptions.Value.IoTCoreURI + ":" + _deviceConfig2.IoTCorePort +
                    " --iotcore-id " + _deviceConfig2.Id;
                process = Process.Start(startInfo);
            });

            // Wait for the asynchronous call to finish
            Thread.Sleep(5000);

            // Assert independent VSEIoTCore2 is reachable
            using (var vseClient = new Client(_iotCoreOptions.Value.IoTCoreURI + ":" + _deviceConfig2.IoTCorePort))
            {
                var vseIoTCoreTree = await vseClient.SendRequestAndAwaitResponseAsync("/gettree");
                var vseIoTCoreMessage = TestUtils.CreateResponseMessage(vseIoTCoreTree);
                Assert.NotNull(vseIoTCoreMessage);
                Assert.Equal(200, vseIoTCoreMessage.Code);
            }

            // Act - add VSEIoTCore2 remote mirror to the global IoTCore
            await globalIoTCoreService.AddMirror(new DeviceConfigurationViewModel()
            {
                Id = _deviceConfig2.Id,
                VseType = _deviceConfig2.VseType,
                VseIpAddress = _deviceConfig2.VseIpAddress,
                VsePort = _deviceConfig2.VsePort,
                IoTCorePort = _deviceConfig2.IoTCorePort
            });

            // Assert newly added remote mirror VSEIoTCore2 is reachable
            using (var globalClient = new Client(_iotCoreOptions.Value.IoTCoreURI + ":" + _iotCoreOptions.Value.GlobalIoTCorePort))
            {
                var remote2 = await globalClient.SendRequestAndAwaitResponseAsync("/remote/2/gettree");
                var remoteMessage2 = TestUtils.CreateResponseMessage(remote2);
                Assert.NotNull(remoteMessage2);
                Assert.Equal(200, remoteMessage2.Code);
            }

            // Finally - stop the global IoTCore and all instances of VSEIoTCore and stop manually started VSEIoTCore2
            await globalIoTCoreService.Stop();
            process?.Kill();
            process?.Dispose();

            // Wait for the asynchronous call to finish
            Thread.Sleep(5000);
        }
    }
}
