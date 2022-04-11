// ----------------------------------------------------------------------------
// Filename: IoTCoreServiceTest.cs
// Copyright (c) 2022 ifm diagnostic GmbH - All rights reserved.
// ----------------------------------------------------------------------------
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace VSEIoTCoreServer.UnitTest
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;
    using AutoMapper;
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
    using VSEIoTCoreServer.WebApp;
    using VSEIoTCoreServer.WebApp.ExtensionMethods;
    using VSEIoTCoreServer.WebApp.Services;
    using VSEIoTCoreServer.WebApp.ViewModels;
    using Xunit;

    [Collection("Sequential")]
    public class IoTCoreServiceTest : IDisposable
    {
        private readonly TestDeviceOptions _testDevice1;
        private readonly TestDeviceOptions _testDevice2;
        private readonly IOptions<IoTCoreOptions> _iotCoreOptions;
        private readonly IMapper _mapperMock;
        private readonly IDeviceConfigurationService _deviceConfiguratonServiceMock;
        private readonly ILoggerFactory _loggerFactoryMock;
        private readonly IOptions<IoTCoreOptions> _iotCoreOptionsMock;

        private DeviceConfiguration _deviceConfig1;
        private DeviceConfiguration _deviceConfig2;
        private IMapper _mapper;
        private NullLoggerFactory _nullLoggerFactory;
        private Mock<SQLiteDbContext> _mockDbContext;
        private DeviceConfigurationService _deviceConfigService;
        private IoTCoreService _iotCoreService;

        public IoTCoreServiceTest()
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

            _mapperMock = new Mock<IMapper>().Object;
            _deviceConfiguratonServiceMock = new Mock<IDeviceConfigurationService>().Object;
            _loggerFactoryMock = new Mock<ILoggerFactory>().Object;
            _iotCoreOptionsMock = Options.Create(new IoTCoreOptions());
        }

        [Fact]
        public void Ctor_Test()
        {
            Assert.NotNull(new IoTCoreService(
                _mapperMock,
                _deviceConfiguratonServiceMock,
                _loggerFactoryMock,
                _iotCoreOptionsMock));
        }

        [Fact]
        public void Ctor_Mapper_Null_Error_Test()
        {
            Assert.Throws<ArgumentNullException>("mapper", () => new IoTCoreService(
                null,
                _deviceConfiguratonServiceMock,
                _loggerFactoryMock,
                _iotCoreOptionsMock));
        }

        [Fact]
        public void Ctor_DeviceConfigService_Null_Error_Test()
        {
            Assert.Throws<ArgumentNullException>("deviceConfigurationService", () => new IoTCoreService(
                _mapperMock,
                null,
                _loggerFactoryMock,
                _iotCoreOptionsMock));
        }

        [Fact]
        public void Ctor_LoggerFactory_Null_Error_Test()
        {
            Assert.Throws<ArgumentNullException>("loggerFactory", () => new IoTCoreService(
                _mapperMock,
                _deviceConfiguratonServiceMock,
                null,
                _iotCoreOptionsMock));
        }

        [Fact]
        public void Ctor_IoTCoreOptions_Null_Error_Test()
        {
            Assert.Throws<ArgumentNullException>("iotCoreOptions", () => new IoTCoreService(
                _mapperMock,
                _deviceConfiguratonServiceMock,
                _loggerFactoryMock,
                null));
        }

        [Fact]
        public async Task Start_Test()
        {
            // Arrange
            Arrange();

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

            // Finally
            await AssertedStop(_iotCoreService, _deviceConfig1.Id, _iotCoreOptions.Value.IoTCoreURI, _deviceConfig1.IoTCorePort);
        }

        [Fact]
        public async Task Stop_Test()
        {
            // Arrange
            Arrange();
            await AssertedStart(_iotCoreService, _deviceConfig1.Id, _iotCoreOptions.Value.IoTCoreURI, _deviceConfig1.IoTCorePort);

            // Act
            await AssertedStop(_iotCoreService, _deviceConfig1.Id, _iotCoreOptions.Value.IoTCoreURI, _deviceConfig1.IoTCorePort);

            // Assert
            using var client = new Client(_iotCoreOptions.Value.IoTCoreURI + ":" + _deviceConfig1.IoTCorePort);
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

        [Fact]
        public async Task GetStatus_Started_Test()
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

            // Finally
            await AssertedStop(_iotCoreService, _deviceConfig1.Id, _iotCoreOptions.Value.IoTCoreURI, _deviceConfig1.IoTCorePort);
        }

        [Fact]
        public async Task GetStatus_Stopped_Test()
        {
            // Arrange
            Arrange();
            await AssertedStart(_iotCoreService, _deviceConfig1.Id, _iotCoreOptions.Value.IoTCoreURI, _deviceConfig1.IoTCorePort);

            // Act
            await AssertedStop(_iotCoreService, _deviceConfig1.Id, _iotCoreOptions.Value.IoTCoreURI, _deviceConfig1.IoTCorePort);

            // Assert
            var device1 = await _iotCoreService.Status(_deviceConfig1.Id);

            Assert.NotNull(device1);
            Assert.Equal(IoTStatus.Stopped, device1.IoTStatus);
            Assert.Equal(DeviceStatus.Disconnected, device1.DeviceStatus);
        }

        public async void Dispose()
        {
            await AssertedStop(_iotCoreService, _deviceConfig1.Id, _iotCoreOptions.Value.IoTCoreURI, _deviceConfig1.IoTCorePort);
            await AssertedStop(_iotCoreService, _deviceConfig2.Id, _iotCoreOptions.Value.IoTCoreURI, _deviceConfig2.IoTCorePort);
            _deviceConfig1 = null;
            _deviceConfig2 = null;
            _mapper = null;
            _nullLoggerFactory = null;
            _mockDbContext = null;
            _deviceConfigService = null;
            _iotCoreService = null;
        }

        private static async Task AssertedStart(IIoTCoreService iotCoreService, int deviceId, string iotCoreUri, int iotCorePort)
        {
            await iotCoreService.Start(deviceId);

            // Wait for the IoTCore to start
            var started = await IoTCoreUtils.WaitUntilVSEIoTCoreStarted(iotCoreUri, iotCorePort);
            Assert.True(started);
        }

        private static async Task AssertedStop(IIoTCoreService iotCoreService, int deviceId, string iotCoreUri, int iotCorePort)
        {
            await iotCoreService.Stop(deviceId);

            // Wait for the IoTCore to stop
            var stopped = await IoTCoreUtils.WaitUntilVSEIoTCoreStopped(iotCoreUri, iotCorePort);
            Assert.True(stopped);
        }

        private void Arrange()
        {
            _deviceConfig1 = TestUtils.GetDeviceConfiguration(_testDevice1);
            _deviceConfig2 = TestUtils.GetDeviceConfiguration(_testDevice2);

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

            _deviceConfigService = new DeviceConfigurationService(_mapper, _mockDbContext.Object, _nullLoggerFactory);

            _iotCoreService = new IoTCoreService(_mapper, _deviceConfigService, _nullLoggerFactory, _iotCoreOptions);
        }
    }
}
