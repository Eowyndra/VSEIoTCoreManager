using Xunit;
using Moq;
using VSEIoTCoreServer.WebApp.Services;
using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using VSEIoTCoreServer.LibraryRuntime;
using VSEIoTCoreServer.WebApp;
using VSEIoTCoreServer.CommonTestUtils;
using VSEIoTCoreServer.DAL.Models;
using AutoMapper;
using Microsoft.Extensions.Logging.Abstractions;
using VSEIoTCoreServer.DAL;
using System.IO;
using System.Threading.Tasks;
using VSEIoTCoreServer.CommonUtils;
using VSEIoTCoreServer.DAL.Models.Enums;
using VSEIoTCoreServer.WebApp.ExtensionMethods;
using System.Net.Http;
using System.Collections.Generic;
using System.Linq;
using MockQueryable.Moq;
using System.Diagnostics;
using VSEIoTCoreServer.WebApp.ViewModels;

namespace VSEIoTCoreServer.UnitTest
{
    public class GlobalIoTCoreServiceTest : IDisposable
    {
        private readonly TestDeviceOptions _testDevice1;
        private readonly TestDeviceOptions _testDevice2;
        private readonly IOptions<IoTCoreOptions> _iotCoreOptions;
        private readonly IDeviceConfigurationService _deviceConfigServiceMock;
        private readonly IIoTCoreService _iotCoreServiceMock;
        private readonly IIoTCoreRuntime _iotCoreRuntimeMock;
        private readonly ILoggerFactory _loggerFactoryMock;

        private DeviceConfiguration _deviceConfig1;
        private DeviceConfiguration _deviceConfig2;
        private IMapper _mapper;
        private NullLoggerFactory _nullLoggerFactory;
        private Mock<SQLiteDbContext> _mockDbContext;
        private DeviceConfigurationService _deviceConfigService;
        private IoTCoreService _iotCoreService;
        private GlobalIoTCoreService _globalIoTCoreService;
        private IIoTCoreRuntime _iotCoreRuntime;

        public GlobalIoTCoreServiceTest()
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

            _deviceConfigServiceMock = new Mock<IDeviceConfigurationService>().Object;
            _iotCoreServiceMock = new Mock<IIoTCoreService>().Object;
            _iotCoreRuntimeMock = new Mock<IIoTCoreRuntime>().Object;
            _loggerFactoryMock = new Mock<ILoggerFactory>().Object;
        }

        [Fact]
        public void Ctor_Test()
        {
            Assert.NotNull(new GlobalIoTCoreService(
                _deviceConfigServiceMock,
                _iotCoreServiceMock,
                _iotCoreRuntimeMock,
                _loggerFactoryMock,
                _iotCoreOptions));
        }

        [Fact]
        public void Ctor_DeviceConfigService_Null_Error_Test()
        {
            Assert.Throws<ArgumentNullException>("deviceConfigurationService", () => new GlobalIoTCoreService(
                null,
                _iotCoreServiceMock,
                _iotCoreRuntimeMock,
                _loggerFactoryMock,
                _iotCoreOptions));
        }


        [Fact]
        public void Ctor_IoTCoreService_Null_Error_Test()
        {
            Assert.Throws<ArgumentNullException>("iotCoreService", () => new GlobalIoTCoreService(
                _deviceConfigServiceMock,
                null,
                _iotCoreRuntimeMock,
                _loggerFactoryMock,
                _iotCoreOptions));
        }

        [Fact]
        public void Ctor_IoTCoreRuntime_Null_Error_Test()
        {
            Assert.Throws<ArgumentNullException>("iotCoreRuntime", () => new GlobalIoTCoreService(
                _deviceConfigServiceMock,
                _iotCoreServiceMock,
                null,
                _loggerFactoryMock,
                _iotCoreOptions));
        }

        [Fact]
        public void Ctor_LoggerFactory_Null_Error_Test()
        {
            Assert.Throws<ArgumentNullException>("loggerFactory", () => new GlobalIoTCoreService(
                _deviceConfigServiceMock,
                _iotCoreServiceMock,
                _iotCoreRuntimeMock,
                null,
                _iotCoreOptions));
        }

        [Fact]
        public void Ctor_IoTCoreOptions_Null_Error_Test()
        {
            Assert.Throws<ArgumentNullException>("iotCoreOptions", () => new GlobalIoTCoreService(
                _deviceConfigServiceMock,
                _iotCoreServiceMock,
                _iotCoreRuntimeMock,
                _loggerFactoryMock,
                null));
        }

        [Fact]
        public async Task Start_Test()
        {
            // NOTE: This test requires that at least one object and counter is contained in the parameter set on both VSE test devices defined in appsettings.Test.json
            // ToDo: Assert that correct parameter set is used on the test devices

            // Arrange
            Arrange();

            // Act
            await AssertedStart(_globalIoTCoreService);

            // Assert
            using (var globalClient = new Client(_iotCoreOptions.Value.IoTCoreURI + ":" + _iotCoreOptions.Value.GlobalIoTCorePort))
            using (var vseClient1 = new Client(_iotCoreOptions.Value.IoTCoreURI + ":" + _deviceConfig1.IoTCorePort))
            using (var vseClient2 = new Client(_iotCoreOptions.Value.IoTCoreURI + ":" + _deviceConfig2.IoTCorePort))
            {
                // Request data from independent VSEIoTCore1
                var deviceInfo1 = await vseClient1.SendRequestAndAwaitResponseAsync(IoTCoreRoutes.Device().Information().Device().GetData());
                var deviceInfoMessage1 = IoTCoreUtils.CreateResponseMessage(deviceInfo1);
                var objectData1 = await vseClient1.SendRequestAndAwaitResponseAsync(IoTCoreRoutes.Device().Objects().Object(0).GetData());
                var objectDataMessage1 = IoTCoreUtils.CreateResponseMessage(objectData1);
                var counterData1 = await vseClient1.SendRequestAndAwaitResponseAsync(IoTCoreRoutes.Device().Counters().Counter(0).GetData());
                var counterDataMessage1 = IoTCoreUtils.CreateResponseMessage(counterData1);

                // Request data from independent VSEIoTCore2
                var deviceInfo2 = await vseClient2.SendRequestAndAwaitResponseAsync(IoTCoreRoutes.Device().Information().Device().GetData());
                var deviceInfoMessage2 = IoTCoreUtils.CreateResponseMessage(deviceInfo2);
                var objectData2 = await vseClient2.SendRequestAndAwaitResponseAsync(IoTCoreRoutes.Device().Objects().Object(0).GetData());
                var objectDataMessage2 = IoTCoreUtils.CreateResponseMessage(objectData2);
                var counterData2 = await vseClient2.SendRequestAndAwaitResponseAsync(IoTCoreRoutes.Device().Counters().Counter(0).GetData());
                var counterDataMessage2 = IoTCoreUtils.CreateResponseMessage(counterData2);

                // Request data from global IoTCore
                var globalTree = await globalClient.SendRequestAndAwaitResponseAsync(IoTCoreRoutes.GetTree());
                var globalMessage = IoTCoreUtils.CreateResponseMessage(globalTree);

                // Request data from VSEIoTCore1 via global IoTCore remote mirror
                var remoteDeviceInfo1 = await globalClient.SendRequestAndAwaitResponseAsync(IoTCoreRoutes.Remote(1).Device().Information().Device().GetData());
                var remoteDeviceInfoMessage1 = IoTCoreUtils.CreateResponseMessage(remoteDeviceInfo1);
                var remoteObjectData1 = await globalClient.SendRequestAndAwaitResponseAsync(IoTCoreRoutes.Remote(1).Device().Objects().Object(0).GetData());
                var remoteObjectDataMessage1 = IoTCoreUtils.CreateResponseMessage(remoteObjectData1);
                var remoteCounterData1 = await globalClient.SendRequestAndAwaitResponseAsync(IoTCoreRoutes.Remote(1).Device().Counters().Counter(0).GetData());
                var remoteCounterDataMessage1 = IoTCoreUtils.CreateResponseMessage(remoteCounterData1);

                // Request data from VSEIoTCore2 via global IoTCore remote mirror
                var remoteDeviceInfo2 = await globalClient.SendRequestAndAwaitResponseAsync(IoTCoreRoutes.Remote(2).Device().Information().Device().GetData());
                var remoteDeviceInfoMessage2 = IoTCoreUtils.CreateResponseMessage(remoteDeviceInfo2);
                var remoteObjectData2 = await globalClient.SendRequestAndAwaitResponseAsync(IoTCoreRoutes.Remote(2).Device().Objects().Object(0).GetData());
                var remoteObjectDataMessage2 = IoTCoreUtils.CreateResponseMessage(remoteObjectData2);
                var remoteCounterData2 = await globalClient.SendRequestAndAwaitResponseAsync(IoTCoreRoutes.Remote(2).Device().Counters().Counter(0).GetData());
                var remoteCounterDataMessage2 = IoTCoreUtils.CreateResponseMessage(remoteCounterData2);

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

            // Finally
            await AssertedStop(_globalIoTCoreService);
        }

        [Fact]
        public async Task Stop_Test()
        {
            // Arrange
            Arrange();
            await AssertedStart(_globalIoTCoreService);

            // Act 
            await AssertedStop(_globalIoTCoreService);

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
                        var response = await globalClient.SendRequestAndAwaitResponseAsync(IoTCoreRoutes.GetTree());
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
                        var response = await vseClient1.SendRequestAndAwaitResponseAsync(IoTCoreRoutes.GetTree());
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
                        var response = await vseClient2.SendRequestAndAwaitResponseAsync(IoTCoreRoutes.GetTree());
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
        public async Task AddMirror_Test()
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
            var iotCoreService = new IoTCoreService(_mapper, deviceConfigurationService, _nullLoggerFactory, _iotCoreOptions);
            var globalIoTCoreService = new GlobalIoTCoreService(deviceConfigurationService, iotCoreService, _iotCoreRuntime, _nullLoggerFactory, _iotCoreOptions);

            // Start the global IoTCore and all configured VSEIoTCores - in this case only one single VSEIoTCore1 should be started
            await AssertedStart(globalIoTCoreService);
            using (var globalClient = new Client(_iotCoreOptions.Value.IoTCoreURI + ":" + _iotCoreOptions.Value.GlobalIoTCorePort))
            {
                // Global IoTCore is reachable
                var globalTree = await globalClient.SendRequestAndAwaitResponseAsync(IoTCoreRoutes.GetTree());
                var globalMessage = IoTCoreUtils.CreateResponseMessage(globalTree);
                Assert.NotNull(globalMessage);
                Assert.Equal(200, globalMessage.Code);

                // Remote VSEIoTCore1 is reachable
                var remote1 = await globalClient.SendRequestAndAwaitResponseAsync(IoTCoreRoutes.Remote(1).GetTree());
                var remoteMessage1 = IoTCoreUtils.CreateResponseMessage(remote1);
                Assert.NotNull(remoteMessage1);
                Assert.Equal(200, remoteMessage1.Code);

                // Remote VSEIoTCore2 is not found, because it has not been added yet
                var remote2 = await globalClient.SendRequestAndAwaitResponseAsync(IoTCoreRoutes.Remote(2).GetTree());
                var remoteMessage2 = IoTCoreUtils.CreateResponseMessage(remote2);
                Assert.NotNull(remoteMessage2);
                Assert.Equal(404, remoteMessage2.Code);
            }

            // Act
            // Manually start VSEIoTCore2 
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

            // Wait for the IoTCore to start
            bool started = await IoTCoreUtils.WaitUntilVSEIoTCoreStarted(_iotCoreOptions.Value.IoTCoreURI, _deviceConfig2.IoTCorePort);
            Assert.True(started);

            // Independent VSEIoTCore2 is reachable
            using (var vseClient = new Client(_iotCoreOptions.Value.IoTCoreURI + ":" + _deviceConfig2.IoTCorePort))
            {
                var vseIoTCoreTree = await vseClient.SendRequestAndAwaitResponseAsync(IoTCoreRoutes.GetTree());
                var vseIoTCoreMessage = IoTCoreUtils.CreateResponseMessage(vseIoTCoreTree);
                Assert.NotNull(vseIoTCoreMessage);
                Assert.Equal(200, vseIoTCoreMessage.Code);
            }

            // Act
            // Add VSEIoTCore2 remote mirror to the global IoTCore
            await globalIoTCoreService.AddMirror(new DeviceConfigurationViewModel()
            {
                Id = _deviceConfig2.Id,
                VseType = _deviceConfig2.VseType,
                VseIpAddress = _deviceConfig2.VseIpAddress,
                VsePort = _deviceConfig2.VsePort,
                IoTCorePort = _deviceConfig2.IoTCorePort
            });

            // Assert
            // Newly added remote mirror VSEIoTCore2 is reachable
            using (var globalClient = new Client(_iotCoreOptions.Value.IoTCoreURI + ":" + _iotCoreOptions.Value.GlobalIoTCorePort))
            {
                var remote2 = await globalClient.SendRequestAndAwaitResponseAsync(IoTCoreRoutes.Remote(2).GetTree());
                var remoteMessage2 = IoTCoreUtils.CreateResponseMessage(remote2);
                Assert.NotNull(remoteMessage2);
                Assert.Equal(200, remoteMessage2.Code);
            }

            // Finally
            // Stop the global IoTCore and all instances of VSEIoTCore and stop manually started VSEIoTCore2
            await AssertedStop(globalIoTCoreService);
            process?.Kill();
            process?.Dispose();
        }

        [Fact]
        public async Task GetStatus_Started_Test()
        {
            // Arrange
            Arrange();
            await AssertedStart(_globalIoTCoreService);

            // Act
            var globalIoTCoreStatus = await _globalIoTCoreService.GetStatus();

            // Assert
            Assert.True(globalIoTCoreStatus.Status == GlobalIoTCoreStatus.Running || globalIoTCoreStatus.Status == GlobalIoTCoreStatus.PartlyRunning);

            // Finally
            await AssertedStop(_globalIoTCoreService);
        }

        [Fact]
        public async Task GetStatus_Stopped_Test()
        {
            // Arrange
            Arrange();
            await AssertedStop(_globalIoTCoreService);


            // Act
            var globalIoTCoreStatus = await _globalIoTCoreService.GetStatus();

            // Assert
            Assert.Equal(GlobalIoTCoreStatus.Stopped, globalIoTCoreStatus.Status);
  
            // Finally
            await AssertedStop(_globalIoTCoreService);
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
            _iotCoreService = new IoTCoreService(_mapper, _deviceConfigService, _nullLoggerFactory, _iotCoreOptions);
            _iotCoreRuntime = new IoTCoreRuntime();

            _globalIoTCoreService = new GlobalIoTCoreService(_deviceConfigService, _iotCoreService, _iotCoreRuntime, _nullLoggerFactory, _iotCoreOptions);
        }

        private async Task AssertedStart(IGlobalIoTCoreService globalIoTCoreService)
        {
            await globalIoTCoreService.Start();

            // Wait for the global IoTCore to start
            bool started = await IoTCoreUtils.WaitUntilGlobalIoTCoreStarted(_iotCoreOptions.Value.IoTCoreURI, _iotCoreOptions.Value.GlobalIoTCorePort);
            Assert.True(started);
        }

        private async Task AssertedStop(IGlobalIoTCoreService globalIoTCoreService)
        {
            await globalIoTCoreService.Stop();

            // Wait for the global IoTCore to stop
            bool stopped = await IoTCoreUtils.WaitUntilGlobalIoTCoreStopped(_iotCoreOptions.Value.IoTCoreURI, _iotCoreOptions.Value.GlobalIoTCorePort);
            Assert.True(stopped);
        }

        public async void Dispose()
        {
            await AssertedStop(_globalIoTCoreService);
            _mapper = null;
            _nullLoggerFactory = null;
            _mockDbContext = null;
            _deviceConfigService = null;
            _iotCoreService = null;
            _globalIoTCoreService = null;
            _iotCoreRuntime = null;
            _deviceConfig1 = null;
            _deviceConfig2 = null;
        }
    }
}