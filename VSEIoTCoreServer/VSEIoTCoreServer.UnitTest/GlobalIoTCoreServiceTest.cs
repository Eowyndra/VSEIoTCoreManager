// ----------------------------------------------------------------------------
// Filename: GlobalIoTCoreServiceTest.cs
// Copyright (c) 2022 ifm diagnostic GmbH - All rights reserved.
// ----------------------------------------------------------------------------
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace VSEIoTCoreServer.UnitTest
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;
    using AutoMapper;
    using ifmIoTCore.Messages;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Abstractions;
    using Microsoft.Extensions.Options;
    using MockQueryable.Moq;
    using Moq;
    using VSEIoTCoreServer.CommonTestUtils;
    using VSEIoTCoreServer.CommonUtils;
    using VSEIoTCoreServer.DAL;
    using VSEIoTCoreServer.DAL.Models;
    using VSEIoTCoreServer.DAL.Models.Enums;
    using VSEIoTCoreServer.LibraryRuntime;
    using VSEIoTCoreServer.WebApp;
    using VSEIoTCoreServer.WebApp.ExtensionMethods;
    using VSEIoTCoreServer.WebApp.Services;
    using VSEIoTCoreServer.WebApp.ViewModels;
    using Xunit;

    [Collection("Sequential")]
    public class GlobalIoTCoreServiceTest : IAsyncDisposable
    {
        private readonly TestDeviceOptions _testDevice1;
        private readonly TestDeviceOptions _testDevice2;
        private readonly TestDeviceOptions _testDevice3;
        private readonly IOptions<IoTCoreOptions> _iotCoreOptions;
        private readonly IDeviceConfigurationService _deviceConfigServiceMock;
        private readonly IIoTCoreService _iotCoreServiceMock;
        private readonly IIoTCoreRuntime _iotCoreRuntimeMock;
        private readonly ILoggerFactory _loggerFactoryMock;

        private DeviceConfiguration _deviceConfig1;
        private DeviceConfiguration _deviceConfig2;
        private DeviceConfiguration _deviceConfig3;
        private IMapper _mapper;
        private NullLoggerFactory _nullLoggerFactory;
        private Mock<SQLiteDbContext> _mockDbContext;
        private DeviceConfigurationService _deviceConfigService;
        private IoTCoreService _iotCoreService;
        private GlobalIoTCoreService _globalIoTCoreService;
        private IIoTCoreRuntime _iotCoreRuntime;
        private Process _iotCoreProcess;

        public GlobalIoTCoreServiceTest()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(@"appsettings.Test.json", false, false)
                .AddEnvironmentVariables()
                .Build();

            var options = new IoTCoreOptions();
            configuration.GetSection(IoTCoreOptions.IoTCoreSettings).Bind(options);
            _iotCoreOptions = Options.Create(options);

            _testDevice1 = new TestDeviceOptions();
            configuration.GetSection("TestDevices:TestDevice1").Bind(_testDevice1);
            _testDevice2 = new TestDeviceOptions();
            configuration.GetSection("TestDevices:TestDevice2").Bind(_testDevice2);
            _testDevice3 = new TestDeviceOptions();
            configuration.GetSection("TestDevices:TestDevice3").Bind(_testDevice3);

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
                var deviceInfoMessage1 = await GetDeviceInfoMessage(vseClient1);
                var objectCount1 = await GetObjectCount(vseClient1);
                var objectDataMessage1 = await GetObjectDataMessage(vseClient1, 0);
                var counterCount1 = await GetCounterCount(vseClient1);
                var counterDataMessage1 = await GetCounterDataMessage(vseClient1, 0);

                // Request data from independent VSEIoTCore2
                var deviceInfoMessage2 = await GetDeviceInfoMessage(vseClient2);
                var objectCount2 = await GetObjectCount(vseClient2);
                var objectDataMessage2 = await GetObjectDataMessage(vseClient2, 0);
                var counterCount2 = await GetCounterCount(vseClient2);
                var counterDataMessage2 = await GetCounterDataMessage(vseClient2, 0);

                // Request data from global IoTCore
                var globalTree = await globalClient.SendRequestAndAwaitResponseAsync(IoTCoreRoutes.GetTree());
                var globalMessage = IoTCoreUtils.CreateResponseMessage(globalTree);

                // Request data from VSEIoTCore1 via global IoTCore remote mirror
                var remoteDeviceInfoMessage1 = await GetRemoteDeviceInfoMessage(globalClient, 1);
                var remoteObjectDataMessage1 = await GetRemoteObjectDataMessage(globalClient, 1, 0);
                var remoteCounterDataMessage1 = await GetRemoteCounterDataMessage(globalClient, 1, 0);

                // Request data from VSEIoTCore2 via global IoTCore remote mirror
                var remoteDeviceInfoMessage2 = await GetRemoteDeviceInfoMessage(globalClient, 2);
                var remoteObjectDataMessage2 = await GetRemoteObjectDataMessage(globalClient, 2, 0);
                var remoteCounterDataMessage2 = await GetRemoteCounterDataMessage(globalClient, 2, 0);

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
                if (objectCount1 >= 1)
                {
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
                }

                // Assert - object data of remote VSEIoTCore2 matches object data of VSEIoTCore2
                if (objectCount2 >= 1)
                {
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
                }

                // Assert - counter data of remote VSEIoTCore1 matches object data of VSEIoTCore1
                if (counterCount1 >= 1)
                {
                    Assert.Equal(counterDataMessage1.Data["value"]["Name"], remoteCounterDataMessage1.Data["value"]["Name"]);
                    Assert.Equal(counterDataMessage1.Data["value"]["Type"], remoteCounterDataMessage1.Data["value"]["Type"]);
                    Assert.Equal(counterDataMessage1.Data["value"]["Unit"], remoteCounterDataMessage1.Data["value"]["Unit"]);
                    Assert.Equal(counterDataMessage1.Data["value"]["State"], remoteCounterDataMessage1.Data["value"]["State"]);
                    Assert.Equal(counterDataMessage1.Data["value"]["Limit"], remoteCounterDataMessage1.Data["value"]["Limit"]);
                }

                // Assert - counter data of remote VSEIoTCore2 matches object data of VSEIoTCore2
                if (counterCount2 >= 1)
                {
                    Assert.Equal(counterDataMessage2.Data["value"]["Name"], remoteCounterDataMessage2.Data["value"]["Name"]);
                    Assert.Equal(counterDataMessage2.Data["value"]["Type"], remoteCounterDataMessage2.Data["value"]["Type"]);
                    Assert.Equal(counterDataMessage2.Data["value"]["Unit"], remoteCounterDataMessage2.Data["value"]["Unit"]);
                    Assert.Equal(counterDataMessage2.Data["value"]["State"], remoteCounterDataMessage2.Data["value"]["State"]);
                    Assert.Equal(counterDataMessage2.Data["value"]["Limit"], remoteCounterDataMessage2.Data["value"]["Limit"]);
                }
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
            using var globalClient = new Client(_iotCoreOptions.Value.IoTCoreURI + ":" + _iotCoreOptions.Value.GlobalIoTCorePort);
            using var vseClient1 = new Client(_iotCoreOptions.Value.IoTCoreURI + ":" + _deviceConfig1.IoTCorePort);
            using var vseClient2 = new Client(_iotCoreOptions.Value.IoTCoreURI + ":" + _deviceConfig2.IoTCorePort);

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

        [Fact]
        public async Task AddMirror_Test()
        {
            // Arrange
            Arrange();

            // Start the global IoTCore and all configured VSEIoTCores
            await AssertedStart(_globalIoTCoreService);

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

                // Remote VSEIoTCore2 is reachable
                var remote2 = await globalClient.SendRequestAndAwaitResponseAsync(IoTCoreRoutes.Remote(2).GetTree());
                var remoteMessage2 = IoTCoreUtils.CreateResponseMessage(remote2);
                Assert.NotNull(remoteMessage2);
                Assert.Equal(200, remoteMessage2.Code);

                // Remote VSEIoTCore3 is not found, because it has not been added yet
                var remote3 = await globalClient.SendRequestAndAwaitResponseAsync(IoTCoreRoutes.Remote(3).GetTree());
                var remoteMessage3 = IoTCoreUtils.CreateResponseMessage(remote3);
                Assert.NotNull(remoteMessage3);
                Assert.Equal(404, remoteMessage3.Code);
            }

            // Act
            // Manually start VSEIoTCore3
            await Task.Run(() =>
            {
                var startInfo = new ProcessStartInfo(_iotCoreOptions.Value.AdapterLocation);
                startInfo.Arguments = "--vse-ip " + _deviceConfig3.VseIpAddress +
                    " --vse-port " + _deviceConfig3.VsePort +
                    " --iotcore-uri " + _iotCoreOptions.Value.IoTCoreURI + ":" + _deviceConfig3.IoTCorePort +
                    " --iotcore-id " + _deviceConfig3.Id;
                _iotCoreProcess = Process.Start(startInfo);
            });

            // Wait for VSEIoTCore3 to start
            var started = await IoTCoreUtils.WaitUntilVSEIoTCoreStarted(_iotCoreOptions.Value.IoTCoreURI, _deviceConfig3.IoTCorePort);
            Assert.True(started);

            // Act
            // Add VSEIoTCore2 remote mirror to the global IoTCore
            await _globalIoTCoreService.AddMirror(new DeviceConfigurationViewModel(
                _deviceConfig3.VseIpAddress,
                _deviceConfig3.VsePort,
                _deviceConfig3.IoTCorePort)
            {
                Id = _deviceConfig3.Id,
                Name = _deviceConfig3.Name,
                VseType = _deviceConfig3.VseType,
            });

            // Assert
            // Newly added remote mirror VSEIoTCore2 is reachable
            using (var globalClient = new Client(_iotCoreOptions.Value.IoTCoreURI + ":" + _iotCoreOptions.Value.GlobalIoTCorePort))
            {
                var remote3 = await globalClient.SendRequestAndAwaitResponseAsync(IoTCoreRoutes.Remote(3).GetTree());
                var remoteMessage3 = IoTCoreUtils.CreateResponseMessage(remote3);
                Assert.NotNull(remoteMessage3);
                Assert.Equal(200, remoteMessage3.Code);
            }

            // Finally
            // Stop the global IoTCore and all instances of VSEIoTCore and stop manually started VSEIoTCore2
            await AssertedStop(_globalIoTCoreService);
            _iotCoreProcess?.Kill();
            _iotCoreProcess?.WaitForExit();
            Assert.True(_iotCoreProcess.HasExited);
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

        public async ValueTask DisposeAsync()
        {
            _iotCoreProcess?.Kill();
            _iotCoreProcess?.WaitForExit();
            _iotCoreProcess?.Dispose();
            await _globalIoTCoreService.Stop();

            _iotCoreProcess = null;
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

        private static async Task<int> GetObjectCount(Client vseClient)
        {
            var result = await vseClient.SendRequestAndAwaitResponseAsync(IoTCoreRoutes.Device().Objects().GetData());
            var message = IoTCoreUtils.CreateResponseMessage(result);
            var objectCount = message.Data.GetCount() ?? -1;
            return objectCount;
        }

        private static async Task<int> GetCounterCount(Client vseClient)
        {
            var result = await vseClient.SendRequestAndAwaitResponseAsync(IoTCoreRoutes.Device().Counters().GetData());
            var message = IoTCoreUtils.CreateResponseMessage(result);
            var counterCount = message.Data.GetCount() ?? -1;
            return counterCount;
        }

        private static async Task<ResponseMessage> GetObjectDataMessage(Client vseClient, int objectId)
        {
            var objectData = await vseClient.SendRequestAndAwaitResponseAsync(IoTCoreRoutes.Device().Objects().Object(objectId).GetData());
            var objectDataMessage = IoTCoreUtils.CreateResponseMessage(objectData);
            return objectDataMessage;
        }

        private static async Task<ResponseMessage> GetCounterDataMessage(Client vseClient, int counterId)
        {
            var counterData = await vseClient.SendRequestAndAwaitResponseAsync(IoTCoreRoutes.Device().Counters().Counter(counterId).GetData());
            var counterDataMessage = IoTCoreUtils.CreateResponseMessage(counterData);
            return counterDataMessage;
        }

        private static async Task<ResponseMessage> GetDeviceInfoMessage(Client vseClient)
        {
            var deviceInfo = await vseClient.SendRequestAndAwaitResponseAsync(IoTCoreRoutes.Device().Information().Device().GetData());
            var deviceInfoMessage = IoTCoreUtils.CreateResponseMessage(deviceInfo);
            return deviceInfoMessage;
        }

        private static async Task<ResponseMessage> GetRemoteObjectDataMessage(Client vseClient, int remoteId, int objectId)
        {
            var remoteObjectData = await vseClient.SendRequestAndAwaitResponseAsync(IoTCoreRoutes.Remote(remoteId).Device().Objects().Object(objectId).GetData());
            var remoteObjectDataMessage = IoTCoreUtils.CreateResponseMessage(remoteObjectData);
            return remoteObjectDataMessage;
        }

        private static async Task<ResponseMessage> GetRemoteCounterDataMessage(Client vseClient, int remoteId, int counterId)
        {
            var remoteCounterData = await vseClient.SendRequestAndAwaitResponseAsync(IoTCoreRoutes.Remote(remoteId).Device().Counters().Counter(counterId).GetData());
            var remoteCounterDataMessage = IoTCoreUtils.CreateResponseMessage(remoteCounterData);
            return remoteCounterDataMessage;
        }

        private static async Task<ResponseMessage> GetRemoteDeviceInfoMessage(Client vseClient, int remoteId)
        {
            var remoteDeviceInfo = await vseClient.SendRequestAndAwaitResponseAsync(IoTCoreRoutes.Remote(remoteId).Device().Information().Device().GetData());
            var remoteDeviceInfoMessage = IoTCoreUtils.CreateResponseMessage(remoteDeviceInfo);
            return remoteDeviceInfoMessage;
        }

        private void Arrange()
        {
            _deviceConfig1 = TestUtils.GetDeviceConfiguration(_testDevice1);
            _deviceConfig2 = TestUtils.GetDeviceConfiguration(_testDevice2);
            _deviceConfig3 = TestUtils.GetDeviceConfiguration(_testDevice3);

            var myProfile = new AutoMapperProfile();
            var configuration = new MapperConfiguration(cfg => cfg.AddProfile(myProfile));
            _mapper = new Mapper(configuration);

            _nullLoggerFactory = new NullLoggerFactory();

            var deviceConfigurations = new List<DeviceConfiguration>()
            {
                _deviceConfig1,
                _deviceConfig2,
            };

            var mockDbSet = deviceConfigurations.AsQueryable().BuildMockDbSet();

            _mockDbContext = new Mock<SQLiteDbContext>();
            _mockDbContext.Setup(x => x.DeviceConfigurations).Returns(mockDbSet.Object);

            _deviceConfigService = new DeviceConfigurationService(
                _mapper,
                _mockDbContext.Object,
                _nullLoggerFactory);

            _iotCoreService = new IoTCoreService(
                _mapper,
                _deviceConfigService,
                _nullLoggerFactory,
                _iotCoreOptions);

            _iotCoreRuntime = new IoTCoreRuntime();

            _globalIoTCoreService = new GlobalIoTCoreService(
                _deviceConfigService,
                _iotCoreService,
                _iotCoreRuntime,
                _nullLoggerFactory,
                _iotCoreOptions);
        }

        private async Task AssertedStart(IGlobalIoTCoreService globalIoTCoreService)
        {
            await globalIoTCoreService.Start();

            // Wait for the global IoTCore to start
            var started = await IoTCoreUtils.WaitUntilGlobalIoTCoreStarted(_iotCoreOptions.Value.IoTCoreURI, _iotCoreOptions.Value.GlobalIoTCorePort);
            Assert.True(started);
        }

        private async Task AssertedStop(IGlobalIoTCoreService globalIoTCoreService)
        {
            await globalIoTCoreService.Stop();

            // Wait for the global IoTCore to stop
            var stopped = await IoTCoreUtils.WaitUntilGlobalIoTCoreStopped(_iotCoreOptions.Value.IoTCoreURI, _iotCoreOptions.Value.GlobalIoTCorePort);
            Assert.True(stopped);
        }
    }
}
