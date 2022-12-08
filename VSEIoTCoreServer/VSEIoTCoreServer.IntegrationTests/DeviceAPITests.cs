// ----------------------------------------------------------------------------
// Filename: DeviceAPITests.cs
// Copyright (c) 2022 ifm diagnostic GmbH - All rights reserved.
// ----------------------------------------------------------------------------
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace VSEIoTCoreServer.IntegrationTests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;
    using VSEIoTCoreServer.CommonTestUtils;
    using VSEIoTCoreServer.CommonUtils;
    using VSEIoTCoreServer.DAL.Models;
    using VSEIoTCoreServer.DAL.Models.Enums;
    using VSEIoTCoreServer.WebApp;
    using VSEIoTCoreServer.WebApp.ViewModels;
    using Xunit;

    [Collection("Sequential")]
    public class DeviceAPITests : IDisposable
    {
        private readonly TestDeviceOptions _testDevice1;
        private readonly TestDeviceOptions _testDevice2;
        private readonly TestDeviceOptions _testDevice3;
        private readonly IOptions<IoTCoreOptions> _iotCoreOptions;

        private DeviceConfiguration _deviceConfig1;
        private DeviceConfiguration _deviceConfig2;
        private DeviceConfiguration _deviceConfig3;
        private HttpClient _httpClient;

        public DeviceAPITests()
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

            _deviceConfig1 = TestUtils.GetDeviceConfiguration(_testDevice1);
            _deviceConfig2 = TestUtils.GetDeviceConfiguration(_testDevice2);
            _deviceConfig3 = TestUtils.GetDeviceConfiguration(_testDevice3);
        }

        [Fact]
        public async Task GetAll_Test()
        {
            // Arrange
            var testServerPort = 5101;
            var deviceConfigurations = new List<DeviceConfiguration>() { _deviceConfig1, _deviceConfig2 };
            _httpClient = TestUtils.SetupTestServer(deviceConfigurations, testServerPort);

            // Act
            var devices = await TestUtils.WebAPI_Get_Devices_Global(_httpClient, testServerPort);

            // Assert
            Assert.NotNull(devices);
            Assert.NotEmpty(devices);
            Assert.Equal(2, devices.Count);

            var deviceConfig1 = devices.FirstOrDefault(device => device.Id == _deviceConfig1.Id);
            Assert.Equal(_deviceConfig1.Id, deviceConfig1.Id);
            Assert.Equal(_deviceConfig1.Name, deviceConfig1.Name);
            Assert.Equal(_deviceConfig1.VseType, deviceConfig1.VseType);
            Assert.Equal(_deviceConfig1.VseIpAddress, deviceConfig1.VseIpAddress);
            Assert.Equal(_deviceConfig1.VsePort, deviceConfig1.VsePort);
            Assert.Equal(_deviceConfig1.IoTCorePort, deviceConfig1.IoTCorePort);
            Assert.Equal(DeviceStatus.Disconnected, deviceConfig1.DeviceStatus);
            Assert.Equal(IoTStatus.Stopped, deviceConfig1.IoTStatus);

            var deviceConfig2 = devices.FirstOrDefault(device => device.Id == _deviceConfig2.Id);
            Assert.Equal(_deviceConfig2.Id, deviceConfig2.Id);
            Assert.Equal(_deviceConfig2.Name, deviceConfig2.Name);
            Assert.Equal(_deviceConfig2.VseType, deviceConfig2.VseType);
            Assert.Equal(_deviceConfig2.VseIpAddress, deviceConfig2.VseIpAddress);
            Assert.Equal(_deviceConfig2.VsePort, deviceConfig2.VsePort);
            Assert.Equal(_deviceConfig2.IoTCorePort, deviceConfig2.IoTCorePort);
            Assert.Equal(DeviceStatus.Disconnected, deviceConfig2.DeviceStatus);
            Assert.Equal(IoTStatus.Stopped, deviceConfig2.IoTStatus);
        }

        [Fact]
        public async Task GetAll_EmptyDeviceList_Test()
        {
            // Arrange
            var testServerPort = 5102;
            var deviceConfigurations = new List<DeviceConfiguration>();
            _httpClient = TestUtils.SetupTestServer(deviceConfigurations, testServerPort);

            // Act
            var devices = await TestUtils.WebAPI_Get_Devices_Global(_httpClient, testServerPort);

            // Assert
            Assert.NotNull(devices);
            Assert.Empty(devices);
        }

        [Fact]
        public async Task GetStatus_DeviceDisconnected_Test()
        {
            // Arrange
            var testServerPort = 5103;
            var deviceConfigurations = new List<DeviceConfiguration>() { _deviceConfig1 };
            _httpClient = TestUtils.SetupTestServer(deviceConfigurations, testServerPort);
            await AssertedGlobalIoTCoreStopped();

            // Assert
            await AssertDeviceStatus(_testDevice1.Id, testServerPort, DeviceStatus.Disconnected);
        }

        [Fact]
        public async Task GetStatus_DeviceConnecting_Test()
        {
            // Arrange
            var testServerPort = 5104;
            var deviceConfigurations = new List<DeviceConfiguration>() { _deviceConfig1 };
            _httpClient = TestUtils.SetupTestServer(deviceConfigurations, testServerPort);
            await AssertedGlobalIoTCoreStopped();

            // Act
            await TestUtils.WebAPI_Post_Start_Global(_httpClient, testServerPort);
            await AssertedGlobalIoTCoreStarted();

            // Assert
            await AssertDeviceStatus(_deviceConfig1.Id, testServerPort, DeviceStatus.Connecting);

            // Finally
            await TestUtils.WebAPI_Post_Stop_Global(_httpClient, testServerPort);
            await AssertedGlobalIoTCoreStopped();
        }

        [Fact]
        public async Task GetStatus_DeviceTimeout_Test()
        {
            // Arrange
            var testServerPort = 5105;
            var timeoutDevice = _deviceConfig1;
            timeoutDevice.VseIpAddress = "123.123.123.123";
            var deviceConfigurations = new List<DeviceConfiguration>() { timeoutDevice };
            _httpClient = TestUtils.SetupTestServer(deviceConfigurations, testServerPort);

            // Act
            await TestUtils.WebAPI_Post_Start_Global(_httpClient, testServerPort);
            await AssertedGlobalIoTCoreStarted();

            // Assert
            await AssertDeviceStatus(timeoutDevice.Id, testServerPort, DeviceStatus.Timeout);

            // Finally
            await TestUtils.WebAPI_Post_Stop_Global(_httpClient, testServerPort);
            await AssertedGlobalIoTCoreStopped();
        }

        [Fact]
        public async Task GetStatus_DeviceConnected_Test()
        {
            // Arrange
            var testServerPort = 5106;
            var deviceConfigurations = new List<DeviceConfiguration>() { _deviceConfig1 };
            _httpClient = TestUtils.SetupTestServer(deviceConfigurations, testServerPort);
            await AssertedGlobalIoTCoreStopped();

            // Act
            await TestUtils.WebAPI_Post_Start_Global(_httpClient, testServerPort);
            await AssertedGlobalIoTCoreStarted();

            // Assert
            await AssertDeviceStatus(_deviceConfig1.Id, testServerPort, DeviceStatus.Connected);

            // Finally
            await TestUtils.WebAPI_Post_Stop_Global(_httpClient, testServerPort);
            await AssertedGlobalIoTCoreStopped();
        }

        [Fact]
        public async Task GetStatus_IoTStopped_Test()
        {
            // Arrange
            var testServerPort = 5107;
            var deviceConfigurations = new List<DeviceConfiguration>() { _deviceConfig1 };
            _httpClient = TestUtils.SetupTestServer(deviceConfigurations, testServerPort);
            await AssertedGlobalIoTCoreStopped();

            // Assert
            await AssertIoTStatus(_deviceConfig1.Id, testServerPort, IoTStatus.Stopped);
        }

        [Fact]
        public async Task GetStatus_IoTStarted_Test()
        {
            // Arrange
            var testServerPort = 5108;
            var deviceConfigurations = new List<DeviceConfiguration>() { _deviceConfig1 };
            _httpClient = TestUtils.SetupTestServer(deviceConfigurations, testServerPort);
            await AssertedGlobalIoTCoreStopped();

            // Act
            await TestUtils.WebAPI_Post_Start_Global(_httpClient, testServerPort);
            await AssertedGlobalIoTCoreStarted();

            // Assert
            await AssertIoTStatus(_deviceConfig1.Id, testServerPort, IoTStatus.Started);

            // Finally
            await AssertGlobalStatus(testServerPort, GlobalIoTCoreStatus.Started);
            await TestUtils.WebAPI_Post_Stop_Global(_httpClient, testServerPort);
            await AssertedGlobalIoTCoreStopped();
        }

        [Fact]
        public async Task AddDevices_AddSingleDevice_Test()
        {
            // Arrange
            var testServerPort = 5109;
            var deviceConfigurations = new List<DeviceConfiguration>();
            _httpClient = TestUtils.SetupTestServer(deviceConfigurations, testServerPort);
            var devices = await TestUtils.WebAPI_Get_Devices_Global(_httpClient, testServerPort);
            Assert.NotNull(devices);
            Assert.Empty(devices);

            var newDevices = new List<AddDeviceViewModel>()
            {
                new AddDeviceViewModel(_deviceConfig1.VseIpAddress, _deviceConfig1.VsePort, _deviceConfig1.IoTCorePort),
            };

            // Act
            await TestUtils.WebAPI_Post_Devices_Global(_httpClient, newDevices, testServerPort);

            // Assert
            devices = await TestUtils.WebAPI_Get_Devices_Global(_httpClient, testServerPort);
            Assert.NotNull(devices);
            Assert.Single(devices);

            var deviceConfig = devices.FirstOrDefault(device => device.Id == _deviceConfig1.Id);
            Assert.Equal(_deviceConfig1.VseIpAddress, deviceConfig.VseIpAddress);
            Assert.Equal(_deviceConfig1.VsePort, deviceConfig.VsePort);
            Assert.Equal(_deviceConfig1.IoTCorePort, deviceConfig.IoTCorePort);
            Assert.Equal("Device_001", deviceConfig.Name);
        }

        [Fact]
        public async Task AddDevices_AddMultipleDevices_Test()
        {
            // Arrange
            var testServerPort = 5110;
            var deviceConfigurations = new List<DeviceConfiguration>();
            _httpClient = TestUtils.SetupTestServer(deviceConfigurations, testServerPort);
            var devices = await TestUtils.WebAPI_Get_Devices_Global(_httpClient, testServerPort);
            Assert.NotNull(devices);
            Assert.Empty(devices);

            var newDevices = new List<AddDeviceViewModel>()
            {
                new AddDeviceViewModel(_deviceConfig1.VseIpAddress, _deviceConfig1.VsePort, _deviceConfig1.IoTCorePort)
                {
                    Name = _deviceConfig1.Name,
                },
                new AddDeviceViewModel(_deviceConfig2.VseIpAddress, _deviceConfig2.VsePort, _deviceConfig2.IoTCorePort)
                {
                    Name = _deviceConfig2.Name,
                },
                new AddDeviceViewModel(_deviceConfig3.VseIpAddress, _deviceConfig3.VsePort, _deviceConfig3.IoTCorePort)
                {
                    Name = _deviceConfig3.Name,
                },
            };

            // Act
            await TestUtils.WebAPI_Post_Devices_Global(_httpClient, newDevices, testServerPort);

            // Assert
            devices = await TestUtils.WebAPI_Get_Devices_Global(_httpClient, testServerPort);
            Assert.NotNull(devices);
            Assert.Equal(3, devices.Count);

            var deviceConfig1 = devices.FirstOrDefault(device => device.Id == _deviceConfig1.Id);
            Assert.Equal(_deviceConfig1.VseIpAddress, deviceConfig1.VseIpAddress);
            Assert.Equal(_deviceConfig1.VsePort, deviceConfig1.VsePort);
            Assert.Equal(_deviceConfig1.IoTCorePort, deviceConfig1.IoTCorePort);
            Assert.Equal(_deviceConfig1.Name, deviceConfig1.Name);

            var deviceConfig2 = devices.FirstOrDefault(device => device.Id == _deviceConfig2.Id);
            Assert.Equal(_deviceConfig2.VseIpAddress, deviceConfig2.VseIpAddress);
            Assert.Equal(_deviceConfig2.VsePort, deviceConfig2.VsePort);
            Assert.Equal(_deviceConfig2.IoTCorePort, deviceConfig2.IoTCorePort);
            Assert.Equal(_deviceConfig2.Name, deviceConfig2.Name);

            var deviceConfig3 = devices.FirstOrDefault(device => device.Id == _deviceConfig3.Id);
            Assert.Equal(_deviceConfig3.VseIpAddress, deviceConfig3.VseIpAddress);
            Assert.Equal(_deviceConfig3.VsePort, deviceConfig3.VsePort);
            Assert.Equal(_deviceConfig3.IoTCorePort, deviceConfig3.IoTCorePort);
            Assert.Equal(_deviceConfig3.Name, deviceConfig3.Name);
        }

        [Fact]
        public async Task AddDevices_AddDuplicateDevice_Test()
        {
            // Arrange
            var testServerPort = 5111;
            var deviceConfigurations = new List<DeviceConfiguration>();
            _httpClient = TestUtils.SetupTestServer(deviceConfigurations, testServerPort);
            var devices = await TestUtils.WebAPI_Get_Devices_Global(_httpClient, testServerPort);
            Assert.NotNull(devices);
            Assert.Empty(devices);

            var newDevices = new List<AddDeviceViewModel>()
            {
                new AddDeviceViewModel(_deviceConfig1.VseIpAddress, _deviceConfig1.VsePort, _deviceConfig1.IoTCorePort),
                new AddDeviceViewModel(_deviceConfig1.VseIpAddress, _deviceConfig1.VsePort, _deviceConfig1.IoTCorePort),
            };

            // Act
            var response = await TestUtils.WebAPI_Post_Devices_Global(_httpClient, newDevices, testServerPort);

            // Assert
            devices = await TestUtils.WebAPI_Get_Devices_Global(_httpClient, testServerPort);
            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
            Assert.NotNull(devices);
            Assert.Single(devices);

            var deviceConfig = devices.FirstOrDefault(device => device.Id == _deviceConfig1.Id);
            Assert.Equal(_deviceConfig1.VseIpAddress, deviceConfig.VseIpAddress);
            Assert.Equal(_deviceConfig1.VsePort, deviceConfig.VsePort);
            Assert.Equal(_deviceConfig1.IoTCorePort, deviceConfig.IoTCorePort);
            Assert.Equal("Device_001", deviceConfig.Name);
        }

        [Fact]
        public async Task AddDevices_AlreadyExists_Test()
        {
            // Arrange
            var testServerPort = 5112;
            var deviceConfigurations = new List<DeviceConfiguration> { _deviceConfig1 };
            _httpClient = TestUtils.SetupTestServer(deviceConfigurations, testServerPort);

            var newDevices = new List<AddDeviceViewModel>()
            {
                new AddDeviceViewModel(_deviceConfig1.VseIpAddress, _deviceConfig1.VsePort, _deviceConfig1.IoTCorePort),
            };

            // Act
            var response = await TestUtils.WebAPI_Post_Devices_Global(_httpClient, newDevices, testServerPort);
            var devices = await TestUtils.WebAPI_Get_Devices_Global(_httpClient, testServerPort);

            // Assert
            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
            Assert.NotNull(devices);
            Assert.Single(devices);
        }

        [Fact]
        public async Task AddDevices_IoTCorePortAlreadyUsed_Test()
        {
            // Arrange
            var testServerPort = 5113;
            var deviceConfigurations = new List<DeviceConfiguration> { _deviceConfig1 };
            _httpClient = TestUtils.SetupTestServer(deviceConfigurations, testServerPort);

            var newDevices = new List<AddDeviceViewModel>()
            {
                new AddDeviceViewModel(_deviceConfig3.VseIpAddress, _deviceConfig3.VsePort, _deviceConfig1.IoTCorePort),
            };

            // Act
            var response = await TestUtils.WebAPI_Post_Devices_Global(_httpClient, newDevices, testServerPort);
            var devices = await TestUtils.WebAPI_Get_Devices_Global(_httpClient, testServerPort);

            // Assert
            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
            Assert.NotNull(devices);
            Assert.Single(devices);
        }

        [Fact]
        public async Task AddDevices_InvalidVseIpAddress_Test()
        {
            // Arrange
            var testServerPort = 5114;
            var deviceConfigurations = new List<DeviceConfiguration>();
            _httpClient = TestUtils.SetupTestServer(deviceConfigurations, testServerPort);

            var newDevices = new List<AddDeviceViewModel>()
            {
                new AddDeviceViewModel("invalidIpString", _deviceConfig1.VsePort, _deviceConfig1.IoTCorePort),
            };

            // Act
            var response = await TestUtils.WebAPI_Post_Devices_Global(_httpClient, newDevices, testServerPort);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task AddDevices_InvalidVsePort_Test()
        {
            // Arrange
            var testServerPort = 5115;
            var deviceConfigurations = new List<DeviceConfiguration>();
            _httpClient = TestUtils.SetupTestServer(deviceConfigurations, testServerPort);

            var newDevices = new List<AddDeviceViewModel>()
            {
                new AddDeviceViewModel(_deviceConfig1.VseIpAddress, 70000, _deviceConfig1.IoTCorePort),
            };

            // Act
            var response = await TestUtils.WebAPI_Post_Devices_Global(_httpClient, newDevices, testServerPort);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task AddDevices_InvalidIoTCorePort_Test()
        {
            // Arrange
            var testServerPort = 5116;
            var deviceConfigurations = new List<DeviceConfiguration>();
            _httpClient = TestUtils.SetupTestServer(deviceConfigurations, testServerPort);

            var newDevices = new List<AddDeviceViewModel>()
            {
                new AddDeviceViewModel(_deviceConfig1.VseIpAddress, _deviceConfig1.VsePort, -1),
            };

            // Act
            var response = await TestUtils.WebAPI_Post_Devices_Global(_httpClient, newDevices, testServerPort);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task AddDevices_AddSingleDevice_WithName_Test()
        {
            // Arrange
            var testServerPort = 5117;
            var deviceConfigurations = new List<DeviceConfiguration>();
            _httpClient = TestUtils.SetupTestServer(deviceConfigurations, testServerPort);

            var newDevices = new List<AddDeviceViewModel>()
            {
                new AddDeviceViewModel(_deviceConfig1.VseIpAddress, _deviceConfig1.VsePort, _deviceConfig1.IoTCorePort)
                {
                    Name = _deviceConfig1.Name,
                },
            };

            // Act
            var response = await TestUtils.WebAPI_Post_Devices_Global(_httpClient, newDevices, testServerPort);
            var devices = await TestUtils.WebAPI_Get_Devices_Global(_httpClient, testServerPort);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(devices);
            Assert.Single(devices);
            Assert.Equal(_deviceConfig1.Name, devices[0].Name);
        }

        public void Dispose()
        {
            _deviceConfig1 = null;
            _deviceConfig2 = null;
            _deviceConfig3 = null;
            _httpClient?.Dispose();
        }

        private async Task AssertedGlobalIoTCoreStarted()
        {
            // Wait for the global IoTCore to start
            var started = await IoTCoreUtils.WaitUntilGlobalIoTCoreStarted(_iotCoreOptions.Value.IoTCoreURI, _iotCoreOptions.Value.GlobalIoTCorePort);
            Assert.True(started);
        }

        private async Task AssertedGlobalIoTCoreStopped()
        {
            // Wait for the global IoTCore to start
            var stopped = await IoTCoreUtils.WaitUntilGlobalIoTCoreStopped(_iotCoreOptions.Value.IoTCoreURI, _iotCoreOptions.Value.GlobalIoTCorePort);
            Assert.True(stopped);
        }

        private async Task AssertGlobalStatus(int port, GlobalIoTCoreStatus status)
        {
            var statusReached = await TestUtils.WaitUntilGlobalStatus(_httpClient, port, status);
            Assert.True(statusReached);
        }

        private async Task AssertDeviceStatus(int deviceId, int port, DeviceStatus status)
        {
            var statusReached = await TestUtils.WaitUntilDeviceStatus(_httpClient, deviceId, port, status);
            Assert.True(statusReached);
        }

        private async Task AssertIoTStatus(int deviceId, int port, IoTStatus status)
        {
            var statusReached = await TestUtils.WaitUntilIoTStatus(_httpClient, deviceId, port, status);
            Assert.True(statusReached);
        }
    }
}
