// ----------------------------------------------------------------------------
// Filename: GlobalIoTCoreAPITests.cs
// Copyright (c) 2022 ifm diagnostic GmbH - All rights reserved.
// ----------------------------------------------------------------------------
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace VSEIoTCoreServer.IntegrationTests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
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
    using Xunit;

    [Collection("Sequential")]
    public class GlobalIoTCoreAPITests : IDisposable
    {
        private readonly TestDeviceOptions _testDevice1;
        private readonly TestDeviceOptions _testDevice2;
        private readonly IOptions<IoTCoreOptions> _iotCoreOptions;

        private DeviceConfiguration _deviceConfig1;
        private DeviceConfiguration _deviceConfig2;
        private HttpClient _httpClient;

        public GlobalIoTCoreAPITests()
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

            _deviceConfig1 = TestUtils.GetDeviceConfiguration(_testDevice1);
            _deviceConfig2 = TestUtils.GetDeviceConfiguration(_testDevice2);
        }

        [Fact]
        public async Task Start_Test()
        {
            // Arrange
            var testServerPort = 5201;
            var deviceConfigurations = new List<DeviceConfiguration> { _deviceConfig1, _deviceConfig2 };
            _httpClient = TestUtils.SetupTestServer(deviceConfigurations, testServerPort);

            // Act
            var response = await TestUtils.WebAPI_Post_Start_Global(_httpClient, testServerPort);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            await AssertedGlobalIoTCoreStarted();
            await AssertGlobalStatus(testServerPort, GlobalIoTCoreStatus.Started);

            // Finally
            response = await TestUtils.WebAPI_Post_Stop_Global(_httpClient, testServerPort);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            await AssertedGlobalIoTCoreStopped();
        }

        [Fact]
        public async Task Stop_Test()
        {
            // Arrange
            var testServerPort = 5202;
            var deviceConfigurations = new List<DeviceConfiguration> { _deviceConfig1, _deviceConfig2 };
            _httpClient = TestUtils.SetupTestServer(deviceConfigurations, testServerPort);

            var response = await TestUtils.WebAPI_Post_Start_Global(_httpClient, testServerPort);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            await AssertedGlobalIoTCoreStarted();
            await AssertGlobalStatus(testServerPort, GlobalIoTCoreStatus.Started);

            // Act
            response = await TestUtils.WebAPI_Post_Stop_Global(_httpClient, testServerPort);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            await AssertedGlobalIoTCoreStopped();
            await AssertGlobalStatus(testServerPort, GlobalIoTCoreStatus.Stopped);
        }

        [Fact]
        public async Task GetStatus_Stopped_Test()
        {
            // Arrange
            var testServerPort = 5203;
            var deviceConfigurations = new List<DeviceConfiguration> { _deviceConfig1, _deviceConfig2 };
            _httpClient = TestUtils.SetupTestServer(deviceConfigurations, testServerPort);

            // Act
            await AssertedGlobalIoTCoreStopped();

            // Assert
            await AssertGlobalStatus(testServerPort, GlobalIoTCoreStatus.Stopped);
        }

        [Fact]
        public async Task GetStatus_Starting_Test()
        {
            // Arrange
            var testServerPort = 5204;
            var deviceConfigurations = new List<DeviceConfiguration> { _deviceConfig1, _deviceConfig2 };
            _httpClient = TestUtils.SetupTestServer(deviceConfigurations, testServerPort);
            await AssertedGlobalIoTCoreStopped();

            // Act
            await TestUtils.WebAPI_Post_Start_Global(_httpClient, testServerPort);

            // Assert
            await AssertGlobalStatus(testServerPort, GlobalIoTCoreStatus.Starting);

            // Finally
            await AssertGlobalStatus(testServerPort, GlobalIoTCoreStatus.Started);
            var response = await TestUtils.WebAPI_Post_Stop_Global(_httpClient, testServerPort);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            await AssertedGlobalIoTCoreStopped();
        }

        [Fact]
        public async Task GetStatus_Stopping_Test()
        {
            // Arrange
            var testServerPort = 5205;
            var deviceConfigurations = new List<DeviceConfiguration> { _deviceConfig1, _deviceConfig2 };
            _httpClient = TestUtils.SetupTestServer(deviceConfigurations, testServerPort);
            await AssertedGlobalIoTCoreStopped();
            await TestUtils.WebAPI_Post_Start_Global(_httpClient, testServerPort);
            await AssertGlobalStatus(testServerPort, GlobalIoTCoreStatus.Started);

            // Act
            await TestUtils.WebAPI_Post_Stop_Global(_httpClient, testServerPort);

            // Assert
            await AssertGlobalStatus(testServerPort, GlobalIoTCoreStatus.Stopping);

            // Finally
            await AssertedGlobalIoTCoreStopped();
            await AssertGlobalStatus(testServerPort, GlobalIoTCoreStatus.Stopped);
        }

        [Fact]
        public async Task GetStatus_Started_Test()
        {
            // Arrange
            var testServerPort = 5206;
            var deviceConfigurations = new List<DeviceConfiguration> { _deviceConfig1, _deviceConfig2 };
            _httpClient = TestUtils.SetupTestServer(deviceConfigurations, testServerPort);
            await AssertedGlobalIoTCoreStopped();

            // Act
            await TestUtils.WebAPI_Post_Start_Global(_httpClient, testServerPort);

            // Assert
            await AssertedGlobalIoTCoreStarted();
            await AssertGlobalStatus(testServerPort, GlobalIoTCoreStatus.Started);

            // Finally
            var response = await TestUtils.WebAPI_Post_Stop_Global(_httpClient, testServerPort);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            await AssertedGlobalIoTCoreStopped();
        }

        [Fact]
        public async Task GetStatus_PartlyRunning_Test()
        {
            // Arrange
            var testServerPort = 5207;
            var timeoutDevice = _deviceConfig1;
            timeoutDevice.VseIpAddress = "123.123.123.123";
            var deviceConfigurations = new List<DeviceConfiguration> { timeoutDevice, _deviceConfig2 };
            _httpClient = TestUtils.SetupTestServer(deviceConfigurations, testServerPort);
            await AssertedGlobalIoTCoreStopped();

            // Act
            await TestUtils.WebAPI_Post_Start_Global(_httpClient, testServerPort);

            // Assert
            await AssertedGlobalIoTCoreStarted();
            await AssertGlobalStatus(testServerPort, GlobalIoTCoreStatus.PartlyRunning);

            // Finally
            var response = await TestUtils.WebAPI_Post_Stop_Global(_httpClient, testServerPort);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            await AssertedGlobalIoTCoreStopped();
        }

        public void Dispose()
        {
            _deviceConfig1 = null;
            _deviceConfig2 = null;
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
            // Wait for the global IoTCore to stop
            var stopped = await IoTCoreUtils.WaitUntilGlobalIoTCoreStopped(_iotCoreOptions.Value.IoTCoreURI, _iotCoreOptions.Value.GlobalIoTCorePort);
            Assert.True(stopped);
        }

        private async Task AssertGlobalStatus(int port, GlobalIoTCoreStatus status)
        {
            var statusReached = await TestUtils.WaitUntilGlobalStatus(_httpClient, port, status);
            Assert.True(statusReached);
        }
    }
}
