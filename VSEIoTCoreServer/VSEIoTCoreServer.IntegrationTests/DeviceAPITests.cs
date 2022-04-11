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
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc.Testing;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Options;
    using Newtonsoft.Json;
    using VSEIoTCoreServer.CommonTestUtils;
    using VSEIoTCoreServer.CommonUtils;
    using VSEIoTCoreServer.DAL;
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
            SetupTestServer(deviceConfigurations, testServerPort);

            // Act
            var devices = await WebAPI_Get_Devices(testServerPort);

            // Assert
            Assert.NotNull(devices);
            Assert.NotEmpty(devices);
            Assert.Equal(2, devices.Count);

            var deviceConfig1 = devices[0];
            Assert.Equal(_deviceConfig1.Id, deviceConfig1.Id);
            Assert.Equal(_deviceConfig1.Name, deviceConfig1.Name);
            Assert.Equal(_deviceConfig1.VseType, deviceConfig1.VseType);
            Assert.Equal(_deviceConfig1.VseIpAddress, deviceConfig1.VseIpAddress);
            Assert.Equal(_deviceConfig1.VsePort, deviceConfig1.VsePort);
            Assert.Equal(_deviceConfig1.IoTCorePort, deviceConfig1.IoTCorePort);
            Assert.Equal(DeviceStatus.Disconnected, deviceConfig1.DeviceStatus);
            Assert.Equal(IoTStatus.Stopped, deviceConfig1.IoTStatus);

            var deviceConfig2 = devices[1];
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
            SetupTestServer(deviceConfigurations, testServerPort);

            // Act
            var devices = await WebAPI_Get_Devices(testServerPort);

            // Assert
            Assert.NotNull(devices);
            Assert.Empty(devices);
        }

        [Fact]
        public async Task GetStatus_Stopped_Test()
        {
            // Arrange
            var testServerPort = 5103;
            var deviceConfigurations = new List<DeviceConfiguration>() { _deviceConfig1 };
            SetupTestServer(deviceConfigurations, testServerPort);

            var response = await WebAPI_Post_Stop(testServerPort);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            await AssertedGlobalIoTCoreStopped();

            // Act
            var status = await WebAPI_Get_Status(_testDevice1.Id, testServerPort);

            // Assert
            Assert.NotNull(status);
            Assert.Equal(IoTStatus.Stopped, status.IoTStatus);
            Assert.Equal(DeviceStatus.Disconnected, status.DeviceStatus);
        }

        [Fact]
        public async Task GetStatus_Started_Test()
        {
            // Arrange
            var testServerPort = 5104;
            var deviceConfigurations = new List<DeviceConfiguration>() { _deviceConfig1 };
            SetupTestServer(deviceConfigurations, testServerPort);

            var response = await WebAPI_Post_Start(testServerPort);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            await AssertedGlobalIoTCoreStarted();

            // Act
            var status = await WebAPI_Get_Status(_testDevice1.Id, testServerPort);

            // Assert
            Assert.NotNull(status);
            Assert.Equal(IoTStatus.Running, status.IoTStatus);
            Assert.True(status.DeviceStatus == DeviceStatus.Connecting || status.DeviceStatus == DeviceStatus.Connected);
        }

        [Fact]
        public async Task AddDevices_AddSingleDevice_Test()
        {
            // Arrange
            var testServerPort = 5105;
            var deviceConfigurations = new List<DeviceConfiguration>();
            SetupTestServer(deviceConfigurations, testServerPort);

            var newDevices = new List<AddDeviceViewModel>()
            {
                new AddDeviceViewModel(_deviceConfig1.VseIpAddress, _deviceConfig1.VsePort, _deviceConfig1.IoTCorePort),
            };

            // Act
            var response = await WebAPI_Post_Devices(newDevices, testServerPort);
            var devices = await WebAPI_Get_Devices(testServerPort);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(devices);
            Assert.Single(devices);

            var deviceConfig = devices[0];
            Assert.Equal(_deviceConfig1.VseIpAddress, deviceConfig.VseIpAddress);
            Assert.Equal(_deviceConfig1.VsePort, deviceConfig.VsePort);
            Assert.Equal(_deviceConfig1.IoTCorePort, deviceConfig.IoTCorePort);
        }

        [Fact]
        public async Task AddDevices_AddMultipleDevices_Test()
        {
            // Arrange
            var testServerPort = 5106;
            var deviceConfigurations = new List<DeviceConfiguration>();
            SetupTestServer(deviceConfigurations, testServerPort);

            var newDevices = new List<AddDeviceViewModel>()
            {
                new AddDeviceViewModel(_deviceConfig1.VseIpAddress, _deviceConfig1.VsePort, _deviceConfig1.IoTCorePort),
                new AddDeviceViewModel(_deviceConfig2.VseIpAddress, _deviceConfig2.VsePort, _deviceConfig2.IoTCorePort),
                new AddDeviceViewModel(_deviceConfig3.VseIpAddress, _deviceConfig3.VsePort, _deviceConfig3.IoTCorePort),
            };

            // Act
            var response = await WebAPI_Post_Devices(newDevices, testServerPort);
            var devices = await WebAPI_Get_Devices(testServerPort);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(devices);
            Assert.Equal(3, devices.Count);

            var deviceConfig1 = devices[0];
            Assert.Equal(_deviceConfig1.VseIpAddress, deviceConfig1.VseIpAddress);
            Assert.Equal(_deviceConfig1.VsePort, deviceConfig1.VsePort);
            Assert.Equal(_deviceConfig1.IoTCorePort, deviceConfig1.IoTCorePort);

            var deviceConfig2 = devices[1];
            Assert.Equal(_deviceConfig2.VseIpAddress, deviceConfig2.VseIpAddress);
            Assert.Equal(_deviceConfig2.VsePort, deviceConfig2.VsePort);
            Assert.Equal(_deviceConfig2.IoTCorePort, deviceConfig2.IoTCorePort);

            var deviceConfig3 = devices[2];
            Assert.Equal(_deviceConfig3.VseIpAddress, deviceConfig3.VseIpAddress);
            Assert.Equal(_deviceConfig3.VsePort, deviceConfig3.VsePort);
            Assert.Equal(_deviceConfig3.IoTCorePort, deviceConfig3.IoTCorePort);
        }

        [Fact]
        public async Task AddDevices_AddDuplicateDevices_Test()
        {
            // Arrange
            var testServerPort = 5107;
            var deviceConfigurations = new List<DeviceConfiguration>();
            SetupTestServer(deviceConfigurations, testServerPort);

            var newDevices = new List<AddDeviceViewModel>()
            {
                new AddDeviceViewModel(_deviceConfig1.VseIpAddress, _deviceConfig1.VsePort, _deviceConfig1.IoTCorePort),
                new AddDeviceViewModel(_deviceConfig1.VseIpAddress, _deviceConfig1.VsePort, _deviceConfig1.IoTCorePort),
            };

            // Act
            var response = await WebAPI_Post_Devices(newDevices, testServerPort);
            var devices = await WebAPI_Get_Devices(testServerPort);

            // Assert
            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
            Assert.NotNull(devices);
        }

        [Fact]
        public async Task AddDevices_AlreadyExists_Test()
        {
            // Arrange
            var testServerPort = 5108;
            var deviceConfigurations = new List<DeviceConfiguration> { _deviceConfig1 };
            SetupTestServer(deviceConfigurations, testServerPort);

            var newDevices = new List<AddDeviceViewModel>()
            {
                new AddDeviceViewModel(_deviceConfig1.VseIpAddress, _deviceConfig1.VsePort, _deviceConfig1.IoTCorePort),
            };

            // Act
            var response = await WebAPI_Post_Devices(newDevices, testServerPort);
            var devices = await WebAPI_Get_Devices(testServerPort);

            // Assert
            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
            Assert.NotNull(devices);
            Assert.Single(devices);
        }

        [Fact]
        public async Task AddDevices_IoTCorePortAlreadyUsed_Test()
        {
            // Arrange
            var testServerPort = 5109;
            var deviceConfigurations = new List<DeviceConfiguration> { _deviceConfig1 };
            SetupTestServer(deviceConfigurations, testServerPort);

            var newDevices = new List<AddDeviceViewModel>()
            {
                new AddDeviceViewModel(_deviceConfig3.VseIpAddress, _deviceConfig3.VsePort, _deviceConfig1.IoTCorePort),
            };

            // Act
            var response = await WebAPI_Post_Devices(newDevices, testServerPort);
            var devices = await WebAPI_Get_Devices(testServerPort);

            // Assert
            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
            Assert.NotNull(devices);
            Assert.Single(devices);
        }

        [Fact]
        public async Task AddDevices_InvalidVseIpAddress_Test()
        {
            // Arrange
            var testServerPort = 5110;
            var deviceConfigurations = new List<DeviceConfiguration>();
            SetupTestServer(deviceConfigurations, testServerPort);

            var newDevices = new List<AddDeviceViewModel>()
            {
                new AddDeviceViewModel("invalidIpString", _deviceConfig1.VsePort, _deviceConfig1.IoTCorePort),
            };

            // Act
            var response = await WebAPI_Post_Devices(newDevices, testServerPort);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task AddDevices_InvalidVsePort_Test()
        {
            // Arrange
            var testServerPort = 5111;
            var deviceConfigurations = new List<DeviceConfiguration>();
            SetupTestServer(deviceConfigurations, testServerPort);

            var newDevices = new List<AddDeviceViewModel>()
            {
                new AddDeviceViewModel(_deviceConfig1.VseIpAddress, 70000, _deviceConfig1.IoTCorePort),
            };

            // Act
            var response = await WebAPI_Post_Devices(newDevices, testServerPort);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task AddDevices_InvalidIoTCorePort_Test()
        {
            // Arrange
            var testServerPort = 5112;
            var deviceConfigurations = new List<DeviceConfiguration>();
            SetupTestServer(deviceConfigurations, testServerPort);

            var newDevices = new List<AddDeviceViewModel>()
            {
                new AddDeviceViewModel(_deviceConfig1.VseIpAddress, _deviceConfig1.VsePort, -1),
            };

            // Act
            var response = await WebAPI_Post_Devices(newDevices, testServerPort);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task AddDevices_AddSingleDevice_WithName_Test()
        {
            // Arrange
            var testServerPort = 5113;
            var deviceConfigurations = new List<DeviceConfiguration>();
            SetupTestServer(deviceConfigurations, testServerPort);

            var newDevices = new List<AddDeviceViewModel>()
            {
                new AddDeviceViewModel(_deviceConfig1.VseIpAddress, _deviceConfig1.VsePort, _deviceConfig1.IoTCorePort)
                {
                    Name = _deviceConfig1.Name,
                },
            };

            // Act
            var response = await WebAPI_Post_Devices(newDevices, testServerPort);
            var devices = await WebAPI_Get_Devices(testServerPort);

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

        private async Task<HttpResponseMessage> WebAPI_Post_Devices(List<AddDeviceViewModel> newDevices, int port)
        {
            var jsonContent = JsonConvert.SerializeObject(newDevices);
            var buffer = System.Text.Encoding.UTF8.GetBytes(jsonContent);
            var byteContent = new ByteArrayContent(buffer);
            byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            var response = await _httpClient.PostAsync($"https://localhost:{port}/api/v1/Device", byteContent);
            return response;
        }

        private async Task<List<DeviceConfigurationViewModel>> WebAPI_Get_Devices(int port)
        {
            var devices = new List<DeviceConfigurationViewModel>();
            var response = await _httpClient.GetAsync($"https://localhost:{port}/api/v1/Device");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                devices = JsonConvert.DeserializeObject<List<DeviceConfigurationViewModel>>(content);
            }

            return devices;
        }

        private async Task<StatusViewModel> WebAPI_Get_Status(int deviceId, int port)
        {
            StatusViewModel status = null;
            var response = await _httpClient.GetAsync($"https://localhost:{port}/api/v1/Device/{deviceId}/status");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                status = JsonConvert.DeserializeObject<StatusViewModel>(content);
            }

            return status;
        }

        private async Task<HttpResponseMessage> WebAPI_Post_Start(int port)
        {
            var response = await _httpClient.PostAsync($"https://localhost:{port}/api/v1/Global/start", null);
            return response;
        }

        private async Task<HttpResponseMessage> WebAPI_Post_Stop(int port)
        {
            var response = await _httpClient.PostAsync($"https://localhost:{port}/api/v1/Global/stop", null);
            return response;
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

        private void SetupTestServer(List<DeviceConfiguration> deviceConfigurations, int port)
        {
            var application = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.UseEnvironment("Test");
                    builder.UseUrls($"https://localhost:{port}");
                    builder.ConfigureServices(services =>
                    {
                        // Setup in-memory database
                        var descriptor = services.SingleOrDefault(
                            d => d.ServiceType == typeof(DbContextOptions<SQLiteDbContext>));
                        services.Remove(descriptor);
                        services.AddDbContext<SQLiteDbContext>(options =>
                        {
                            options.UseInMemoryDatabase($"InMemoryDb{port}");
                        });

                        var sp = services.BuildServiceProvider();

                        using var scope = sp.CreateScope();
                        var scopedServices = scope.ServiceProvider;
                        var db = scopedServices.GetRequiredService<SQLiteDbContext>();
                        db.Database.EnsureCreated();
                        foreach (var deviceConfig in deviceConfigurations)
                        {
                            db.DeviceConfigurations.Add(deviceConfig);
                        }

                        db.SaveChangesAsync();
                    });
                });

            // Create HttpClient to access test server
            _httpClient = application.CreateClient();
        }
    }
}
