// ----------------------------------------------------------------------------
// Filename: IoTCoreServerTest.cs
// Copyright (c) 2022 ifm diagnostic GmbH - All rights reserved.
// ----------------------------------------------------------------------------
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace VSEIoTCoreServer.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using ifmIoTCore.Messages;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Abstractions;
    using Microsoft.Extensions.Options;
    using Moq;
    using VSEIoTCoreServer.CommonTestUtils;
    using VSEIoTCoreServer.CommonUtils;
    using VSEIoTCoreServer.CommonUtils.ExtensionMethods;
    using VSEIoTCoreServer.DAL.Models;
    using VSEIoTCoreServer.DAL.Models.Enums;
    using VSEIoTCoreServer.WebApp;
    using VSEIoTCoreServer.WebApp.Models;
    using VSEIoTCoreServer.WebApp.Services;
    using VSEIoTCoreServer.WebApp.ViewModels;
    using Xunit;

    [Collection("Sequential")]
    public class IoTCoreServerTest : IDisposable
    {
        private readonly TestDeviceOptions _testDevice1;
        private readonly TestDeviceOptions _testDevice2;
        private readonly TestDeviceOptions _testDevice3;
        private readonly DeviceConfigurationViewModel _deviceConfig1;
        private readonly DeviceConfigurationViewModel _deviceConfig2;
        private readonly DeviceConfigurationViewModel _deviceConfig3;
        private readonly GlobalConfiguration _globalConfiguration;
        private readonly IServiceProvider _serviceProviderMock;
        private readonly IOptions<IoTCoreOptions> _iotCoreOptions;
        private readonly ILoggerFactory _loggerFactoryMock;

        private NullLoggerFactory _nullLoggerFactory;
        private IIoTCoreServer _iotCoreServer;

        public IoTCoreServerTest()
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

            _deviceConfig1 = TestUtils.GetDeviceConfigurationViewModel(_testDevice1);
            _deviceConfig2 = TestUtils.GetDeviceConfigurationViewModel(_testDevice2);
            _deviceConfig3 = TestUtils.GetDeviceConfigurationViewModel(_testDevice3);

            _globalConfiguration = TestUtils.GetGlobalConfiguration();

            _serviceProviderMock = new Mock<IServiceProvider>().Object;
            _loggerFactoryMock = new Mock<ILoggerFactory>().Object;
        }

        [Fact]
        public void Ctor_Test()
        {
            Assert.NotNull(new IoTCoreServer(_serviceProviderMock, _loggerFactoryMock, _iotCoreOptions));
        }

        [Fact]
        public void Ctor_ServiceProvider_Null_Error_Test()
        {
            Assert.Throws<ArgumentNullException>("serviceProvider", () => new IoTCoreServer(null, _loggerFactoryMock, _iotCoreOptions));
        }

        [Fact]
        public void Ctor_LoggerFactory_Null_Error_Test()
        {
            Assert.Throws<ArgumentNullException>("loggerFactory", () => new IoTCoreServer(_serviceProviderMock, null, _iotCoreOptions));
        }

        [Fact]
        public void Ctor_IoTCoreOptions_Null_Error_Test()
        {
            Assert.Throws<ArgumentNullException>("iotCoreOptions", () => new IoTCoreServer(_serviceProviderMock, _loggerFactoryMock, null));
        }

        [Fact]
        public async Task Start_CheckDeviceInfo_Test()
        {
            // Arrange
            Arrange(new List<DeviceConfigurationViewModel>()
            {
                _deviceConfig1,
                _deviceConfig2,
            });

            // Act
            await _iotCoreServer.Start();
            await AssertGlobalIoTCoreStarted();
            await AssertVSEIoTCoreStarted(_iotCoreOptions.Value.IoTCoreURI, _deviceConfig1.IoTCorePort);
            await AssertVSEIoTCoreStarted(_iotCoreOptions.Value.IoTCoreURI, _deviceConfig2.IoTCorePort);
            await AssertRemoteIoTCoreIsReachable(1);
            await AssertRemoteIoTCoreIsReachable(2);

            using var globalIoTCore = new Client(_iotCoreOptions.Value.IoTCoreURI + ":" + _globalConfiguration.GlobalIoTCorePort);
            using var vseIoTCore1 = new Client(_iotCoreOptions.Value.IoTCoreURI + ":" + _deviceConfig1.IoTCorePort);
            using var vseIoTCore2 = new Client(_iotCoreOptions.Value.IoTCoreURI + ":" + _deviceConfig2.IoTCorePort);

            // Request data from independent VSEIoTCores
            var deviceInfoMessage1 = await GetDeviceInfoMessage(vseIoTCore1);
            var deviceInfoMessage2 = await GetDeviceInfoMessage(vseIoTCore2);

            // Request data from global IoTCore remote mirror
            var remoteDeviceInfoMessage1 = await GetRemoteDeviceInfoMessage(globalIoTCore, 1);
            var remoteDeviceInfoMessage2 = await GetRemoteDeviceInfoMessage(globalIoTCore, 2);

            // Stop the global IoTCore instance
            await _iotCoreServer.Stop();
            await AssertGlobalIoTCoreStopped();

            // Assert
            AssertDeviceInfoMatches(deviceInfoMessage1, remoteDeviceInfoMessage1);
            AssertDeviceInfoMatches(deviceInfoMessage2, remoteDeviceInfoMessage2);
        }

        [Fact]
        public async Task Start_CheckObject_Test()
        {
            // Arrange
            Arrange(new List<DeviceConfigurationViewModel>()
            {
                _deviceConfig1,
                _deviceConfig2,
            });

            // Act
            await _iotCoreServer.Start();
            await AssertGlobalIoTCoreStarted();
            await AssertVSEIoTCoreStarted(_iotCoreOptions.Value.IoTCoreURI, _deviceConfig1.IoTCorePort);
            await AssertVSEIoTCoreStarted(_iotCoreOptions.Value.IoTCoreURI, _deviceConfig2.IoTCorePort);
            await AssertRemoteIoTCoreIsReachable(1);
            await AssertRemoteIoTCoreIsReachable(2);

            using var globalIoTCore = new Client(_iotCoreOptions.Value.IoTCoreURI + ":" + _globalConfiguration.GlobalIoTCorePort);
            using var vseIoTCore1 = new Client(_iotCoreOptions.Value.IoTCoreURI + ":" + _deviceConfig1.IoTCorePort);
            using var vseIoTCore2 = new Client(_iotCoreOptions.Value.IoTCoreURI + ":" + _deviceConfig2.IoTCorePort);

            // Request data from independent VSEIoTCores
            var objectCount1 = await GetObjectCount(vseIoTCore1);
            var objectCount2 = await GetObjectCount(vseIoTCore2);
            var objectDataMessage1 = await GetObjectDataMessage(vseIoTCore1, 0);
            var objectDataMessage2 = await GetObjectDataMessage(vseIoTCore2, 0);

            // Request data from global IoTCore remote mirror
            var remoteObjectDataMessage1 = await GetRemoteObjectDataMessage(globalIoTCore, 1, 0);
            var remoteObjectDataMessage2 = await GetRemoteObjectDataMessage(globalIoTCore, 2, 0);

            // Stop the global IoTCore instance
            await _iotCoreServer.Stop();
            await AssertGlobalIoTCoreStopped();

            // Assert - only if at least one object is configured
            if (objectCount1 >= 1)
            {
                AssertObjectMatches(objectDataMessage1, remoteObjectDataMessage1);
            }

            if (objectCount2 >= 1)
            {
                AssertObjectMatches(objectDataMessage2, remoteObjectDataMessage2);
            }
        }

        [Fact]
        public async Task Start_CheckCounter_Test()
        {
            // Arrange
            Arrange(new List<DeviceConfigurationViewModel>()
            {
                _deviceConfig1,
                _deviceConfig2,
            });

            // Act
            await _iotCoreServer.Start();
            await AssertGlobalIoTCoreStarted();
            await AssertVSEIoTCoreStarted(_iotCoreOptions.Value.IoTCoreURI, _deviceConfig1.IoTCorePort);
            await AssertVSEIoTCoreStarted(_iotCoreOptions.Value.IoTCoreURI, _deviceConfig2.IoTCorePort);
            await AssertRemoteIoTCoreIsReachable(1);
            await AssertRemoteIoTCoreIsReachable(2);

            using var globalIoTCore = new Client(_iotCoreOptions.Value.IoTCoreURI + ":" + _globalConfiguration.GlobalIoTCorePort);
            using var vseIoTCore1 = new Client(_iotCoreOptions.Value.IoTCoreURI + ":" + _deviceConfig1.IoTCorePort);
            using var vseIoTCore2 = new Client(_iotCoreOptions.Value.IoTCoreURI + ":" + _deviceConfig2.IoTCorePort);

            // Request data from independent VSEIoTCores
            var counterCount1 = await GetCounterCount(vseIoTCore1);
            var counterCount2 = await GetCounterCount(vseIoTCore2);
            var counterDataMessage1 = await GetCounterDataMessage(vseIoTCore1, 0);
            var counterDataMessage2 = await GetCounterDataMessage(vseIoTCore2, 0);

            // Request data from global IoTCore remote mirror
            var remoteCounterDataMessage1 = await GetRemoteCounterDataMessage(globalIoTCore, 1, 0);
            var remoteCounterDataMessage2 = await GetRemoteCounterDataMessage(globalIoTCore, 2, 0);

            // Stop the global IoTCore instance
            await _iotCoreServer.Stop();
            await AssertGlobalIoTCoreStopped();

            // Assert - only if at least one counter is configured
            if (counterCount1 >= 1)
            {
                AssertCounterMatches(counterDataMessage1, remoteCounterDataMessage1);
            }

            if (counterCount2 >= 1)
            {
                AssertCounterMatches(counterDataMessage2, remoteCounterDataMessage2);
            }
        }

        [Fact]
        public async Task Stop_Test()
        {
            // Arrange
            Arrange(new List<DeviceConfigurationViewModel>()
            {
                _deviceConfig1,
                _deviceConfig2,
            });
            await _iotCoreServer.Start();
            await AssertGlobalIoTCoreStarted();
            await AssertGlobalStatus(GlobalIoTCoreStatus.Started);

            // Act
            await _iotCoreServer.Stop();

            // Assert
            await AssertGlobalIoTCoreStopped();
            await AssertVSEIoTCoreStopped(_iotCoreOptions.Value.IoTCoreURI, _deviceConfig1.IoTCorePort);
            await AssertVSEIoTCoreStopped(_iotCoreOptions.Value.IoTCoreURI, _deviceConfig2.IoTCorePort);
        }

        [Fact]
        public async Task Add_Test()
        {
            // Arrange
            Arrange(new List<DeviceConfigurationViewModel>()
            {
                _deviceConfig1,
                _deviceConfig2,
            });

            await _iotCoreServer.Start();
            await AssertGlobalIoTCoreStarted();
            await AssertVSEIoTCoreStarted(_iotCoreOptions.Value.IoTCoreURI, _deviceConfig1.IoTCorePort);
            await AssertVSEIoTCoreStarted(_iotCoreOptions.Value.IoTCoreURI, _deviceConfig2.IoTCorePort);

            // Remote VSEIoTCore 1 & 2 reachable, 3 has not been added yet
            await AssertRemoteIoTCoreIsReachable(1);
            await AssertRemoteIoTCoreIsReachable(2);
            await AssertRemoteIoTCoreIsNotReachable(3);

            // Act
            var newDeviceConfig = new DeviceConfigurationViewModel(_deviceConfig3.VseIpAddress, _deviceConfig3.VsePort, _deviceConfig3.IoTCorePort, _deviceConfig3.Name)
            {
                Id = _deviceConfig3.Id,
                VseType = _deviceConfig3.VseType,
            };
            await _iotCoreServer.Add(newDeviceConfig);

            // Assert
            await AssertVSEIoTCoreStarted(_iotCoreOptions.Value.IoTCoreURI, _deviceConfig3.IoTCorePort);
            await AssertRemoteIoTCoreIsReachable(3);

            // Finally
            await _iotCoreServer.Stop();
            await AssertGlobalIoTCoreStopped();
            await AssertVSEIoTCoreStopped(_iotCoreOptions.Value.IoTCoreURI, _deviceConfig3.IoTCorePort);
        }

        [Fact]
        public async Task GetStatus_Started_Test()
        {
            // Arrange
            Arrange(new List<DeviceConfigurationViewModel>()
            {
                _deviceConfig1,
                _deviceConfig2,
            });
            await _iotCoreServer.Start();
            await AssertGlobalIoTCoreStarted();
            await AssertGlobalStatus(GlobalIoTCoreStatus.Started);

            // Act
            var globalIoTCoreStatus = await _iotCoreServer.GetStatus();

            // Assert
            Assert.Equal(GlobalIoTCoreStatus.Started, globalIoTCoreStatus.Status);

            // Finally
            await _iotCoreServer.Stop();
            await AssertGlobalIoTCoreStopped();
        }

        [Fact]
        public async Task GetStatus_Starting_Test()
        {
            // Arrange
            Arrange(new List<DeviceConfigurationViewModel>()
            {
                _deviceConfig1,
                _deviceConfig2,
            });

            // Act
            await _iotCoreServer.Start();
            await AssertGlobalStatus(GlobalIoTCoreStatus.Starting);

            // Assert
            var globalIoTCoreStatus = await _iotCoreServer.GetStatus();
            Assert.Equal(GlobalIoTCoreStatus.Starting, globalIoTCoreStatus.Status);

            // Finally
            await AssertGlobalStatus(GlobalIoTCoreStatus.Started);
            await _iotCoreServer.Stop();
            await AssertGlobalIoTCoreStopped();
        }

        [Fact]
        public async Task GetStatus_Stopping_Test()
        {
            // Arrange
            Arrange(new List<DeviceConfigurationViewModel>()
            {
                _deviceConfig1,
                _deviceConfig2,
            });
            await _iotCoreServer.Start();
            await AssertGlobalIoTCoreStarted();
            await AssertGlobalStatus(GlobalIoTCoreStatus.Started);

            // Act
            await _iotCoreServer.Stop();
            await AssertGlobalStatus(GlobalIoTCoreStatus.Stopping);

            // Assert
            var globalIoTCoreStatus = await _iotCoreServer.GetStatus();
            Assert.Equal(GlobalIoTCoreStatus.Stopping, globalIoTCoreStatus.Status);

            // Finally
            await AssertGlobalIoTCoreStopped();
        }

        [Fact]
        public async Task GetStatus_PartlyRunning_Test()
        {
            // Arrange
            var timeoutDevice = _deviceConfig2;
            timeoutDevice.VseIpAddress = "123.123.123.123";
            Arrange(new List<DeviceConfigurationViewModel>()
            {
                _deviceConfig1,
                timeoutDevice,
            });
            await _iotCoreServer.Start();
            await AssertGlobalIoTCoreStarted();
            await AssertGlobalStatus(GlobalIoTCoreStatus.PartlyRunning);

            // Act
            var globalIoTCoreStatus = await _iotCoreServer.GetStatus();

            // Assert
            Assert.Equal(GlobalIoTCoreStatus.PartlyRunning, globalIoTCoreStatus.Status);

            // Finally
            await _iotCoreServer.Stop();
            await AssertGlobalIoTCoreStopped();
        }

        [Fact]
        public async Task GetStatus_Stopped_Test()
        {
            // Arrange
            Arrange(new List<DeviceConfigurationViewModel>()
            {
                _deviceConfig1,
                _deviceConfig2,
            });

            await AssertGlobalIoTCoreStopped();

            // Act
            var globalIoTCoreStatus = await _iotCoreServer.GetStatus();

            // Assert
            Assert.Equal(GlobalIoTCoreStatus.Stopped, globalIoTCoreStatus.Status);
        }

        public void Dispose()
        {
            if (_iotCoreServer != null && _iotCoreServer.GetStatus().GetAwaiter().GetResult().Status != GlobalIoTCoreStatus.Stopped)
            {
                _iotCoreServer.Stop().GetAwaiter().GetResult();
            }

            _nullLoggerFactory = null;
            _iotCoreServer = null;
        }

        private static void AssertDeviceInfoMatches(ResponseMessage deviceInfoMsg, ResponseMessage remoteDeviceInfoMsg)
        {
            Assert.NotNull(deviceInfoMsg);
            Assert.Equal(200, deviceInfoMsg.Code);
            Assert.NotNull(remoteDeviceInfoMsg);
            Assert.Equal(200, remoteDeviceInfoMsg.Code);

            Assert.Equal(deviceInfoMsg.Data["value"]["Name"], remoteDeviceInfoMsg.Data["value"]["Name"]);
            Assert.Equal(deviceInfoMsg.Data["value"]["Type"], remoteDeviceInfoMsg.Data["value"]["Type"]);
            Assert.Equal(deviceInfoMsg.Data["value"]["Revision"], remoteDeviceInfoMsg.Data["value"]["Revision"]);
            Assert.Equal(deviceInfoMsg.Data["value"]["Serial"], remoteDeviceInfoMsg.Data["value"]["Serial"]);
            Assert.Equal(deviceInfoMsg.Data["value"]["Firmware"], remoteDeviceInfoMsg.Data["value"]["Firmware"]);
        }

        private static void AssertObjectMatches(ResponseMessage objectMsg, ResponseMessage remoteObjectMsg)
        {
            Assert.NotNull(objectMsg);
            Assert.Equal(200, objectMsg.Code);
            Assert.NotNull(remoteObjectMsg);
            Assert.Equal(200, remoteObjectMsg.Code);

            Assert.Equal(objectMsg.Data["value"]["Name"], remoteObjectMsg.Data["value"]["Name"]);
            Assert.Equal(objectMsg.Data["value"]["Type"], remoteObjectMsg.Data["value"]["Type"]);
            Assert.Equal(objectMsg.Data["value"]["ID"], remoteObjectMsg.Data["value"]["ID"]);
            Assert.Equal(objectMsg.Data["value"]["Unit"], remoteObjectMsg.Data["value"]["Unit"]);
            Assert.Equal(objectMsg.Data["value"]["State"], remoteObjectMsg.Data["value"]["State"]);
            Assert.Equal(objectMsg.Data["value"]["Warning"], remoteObjectMsg.Data["value"]["Warning"]);
            Assert.Equal(objectMsg.Data["value"]["BaseLine"], remoteObjectMsg.Data["value"]["BaseLine"]);
            Assert.Equal(objectMsg.Data["value"]["Damage"], remoteObjectMsg.Data["value"]["Damage"]);
            Assert.Equal(objectMsg.Data["value"]["InputID"], remoteObjectMsg.Data["value"]["InputID"]);
            Assert.Equal(objectMsg.Data["value"]["InputType"], remoteObjectMsg.Data["value"]["InputType"]);
        }

        private static void AssertCounterMatches(ResponseMessage counterMsg, ResponseMessage remoteCounterMsg)
        {
            Assert.NotNull(counterMsg);
            Assert.Equal(200, counterMsg.Code);
            Assert.NotNull(remoteCounterMsg);
            Assert.Equal(200, remoteCounterMsg.Code);

            Assert.Equal(counterMsg.Data["value"]["Name"], remoteCounterMsg.Data["value"]["Name"]);
            Assert.Equal(counterMsg.Data["value"]["Type"], remoteCounterMsg.Data["value"]["Type"]);
            Assert.Equal(counterMsg.Data["value"]["Unit"], remoteCounterMsg.Data["value"]["Unit"]);
            Assert.Equal(counterMsg.Data["value"]["State"], remoteCounterMsg.Data["value"]["State"]);
            Assert.Equal(counterMsg.Data["value"]["Limit"], remoteCounterMsg.Data["value"]["Limit"]);
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

        private static async Task AssertVSEIoTCoreStopped(string iotCoreUri, int iotCorePort)
        {
            // Wait for the IoTCore to stop
            var stopped = await IoTCoreUtils.WaitUntilVSEIoTCoreStopped(iotCoreUri, iotCorePort);
            Assert.True(stopped);
        }

        private static async Task AssertVSEIoTCoreStarted(string iotCoreUri, int iotCorePort)
        {
            // Wait for the IoTCore to start
            var started = await IoTCoreUtils.WaitUntilVSEIoTCoreStarted(iotCoreUri, iotCorePort);
            Assert.True(started);
        }

        private async void Arrange(List<DeviceConfigurationViewModel> deviceConfigurations)
        {
            _nullLoggerFactory = new NullLoggerFactory();

            // Mock IServiceProvider to return IGlobalConfigurationService used in IIoTCoreServer
            var globalConfigViewModel = TestUtils.GetGlobalConfigurationViewModel(_globalConfiguration);
            var globalConfigMock = new Mock<IGlobalConfigurationService>();
            globalConfigMock
                .Setup(x => x.GetConfig())
                .Returns(Task.FromResult(globalConfigViewModel));

            var serviceProvider = new Mock<IServiceProvider>();
            serviceProvider
                .Setup(x => x.GetService(typeof(IGlobalConfigurationService)))
                .Returns(globalConfigMock.Object);

            var serviceScope = new Mock<IServiceScope>();
            serviceScope
                .Setup(x => x.ServiceProvider)
                .Returns(serviceProvider.Object);

            var serviceScopeFactory = new Mock<IServiceScopeFactory>();
            serviceScopeFactory
                .Setup(x => x.CreateScope())
                .Returns(serviceScope.Object);

            serviceProvider
                .Setup(x => x.GetService(typeof(IServiceScopeFactory)))
                .Returns(serviceScopeFactory.Object);

            _iotCoreServer = new IoTCoreServer(serviceProvider.Object, _nullLoggerFactory, _iotCoreOptions);

            foreach (var deviceConfiguration in deviceConfigurations)
            {
                await _iotCoreServer.Add(deviceConfiguration);
            }
        }

        private async Task AssertGlobalIoTCoreStarted()
        {
            // Wait for the global IoTCore to start
            var started = await IoTCoreUtils.WaitUntilGlobalIoTCoreStarted(_iotCoreOptions.Value.IoTCoreURI, _globalConfiguration.GlobalIoTCorePort);
            Assert.True(started);
        }

        private async Task AssertGlobalIoTCoreStopped()
        {
            // Wait for the global IoTCore to stop
            var stopped = await IoTCoreUtils.WaitUntilGlobalIoTCoreStopped(_iotCoreOptions.Value.IoTCoreURI, _globalConfiguration.GlobalIoTCorePort);
            Assert.True(stopped);
        }

        private async Task AssertRemoteIoTCoreIsReachable(int remoteId)
        {
            // Wait for the remote IoTCore to be reachable
            var reachable = await IoTCoreUtils.WaitUntilRemoteIoTCoreReachable(_iotCoreOptions.Value.IoTCoreURI, _globalConfiguration.GlobalIoTCorePort, remoteId);
            Assert.True(reachable);
        }

        private async Task AssertRemoteIoTCoreIsNotReachable(int remoteId)
        {
            // Wait for the remote IoTCore to be reachable
            var notReachable = await IoTCoreUtils.WaitUntilRemoteIoTCoreNotReachable(_iotCoreOptions.Value.IoTCoreURI, _globalConfiguration.GlobalIoTCorePort, remoteId);
            Assert.True(notReachable);
        }

        private async Task AssertGlobalStatus(GlobalIoTCoreStatus status)
        {
            // Wait for the global IoTCore to start
            var statusReached = await WaitUntilStatus(status);
            Assert.True(statusReached);
        }

        private async Task<bool> WaitUntilStatus(GlobalIoTCoreStatus status, int maxWaitInMilliseconds = 60_000)
        {
            var result = false;
            while (maxWaitInMilliseconds > 0)
            {
                var currentStatus = await _iotCoreServer.GetStatus();
                if (currentStatus.Status == status)
                {
                    result = true;
                    break;
                }

                Thread.Sleep(500); // waiting 500 ms, then re-checking
                maxWaitInMilliseconds -= 500;
            }

            return result;
        }
    }
}
