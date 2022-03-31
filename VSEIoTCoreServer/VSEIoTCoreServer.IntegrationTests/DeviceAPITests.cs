using AutoMapper;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using VSEIoTCoreServer.CommonUtils;
using VSEIoTCoreServer.DAL;
using VSEIoTCoreServer.DAL.Models;
using VSEIoTCoreServer.WebApp;
using VSEIoTCoreServer.WebApp.Services;
using VSEIoTCoreServer.WebApp.ViewModels;
using Xunit;
using Microsoft.AspNetCore.Hosting;
using VSEIoTCoreServer.CommonTestUtils;
using System.Net;
using VSEIoTCoreServer.DAL.Models.Enums;

namespace VSEIoTCoreServer.IntegrationTests
{
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
        private static int _testServerPort = 5000; // Using a different port for the test server in each test to allow parallel testing

        public DeviceAPITests()
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
            _testDevice3 = new TestDeviceOptions();
            configuration.GetSection("TestDevices:TestDevice3").Bind(_testDevice3);

            Initialize();
        }

        [Fact]
        public async Task GetAll_Test()
        {
            // Arrange 
            _testServerPort++;
            List<DeviceConfiguration> deviceConfigurations = new List<DeviceConfiguration>();
            deviceConfigurations.Add(_deviceConfig1);
            deviceConfigurations.Add(_deviceConfig2);
            SetupTestServer(deviceConfigurations, _testServerPort);

            // Act
            var devices = await WebAPI_Get_Devices(_testServerPort);

            // Assert
            Assert.NotNull(devices);
            Assert.NotEmpty(devices);
            Assert.Equal(2, devices.Count);

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
        public async Task GetAll_EmptyDeviceList_Test()
        {
            // Arrange
            _testServerPort++;
            List<DeviceConfiguration> deviceConfigurations = new List<DeviceConfiguration>();
            SetupTestServer(deviceConfigurations, _testServerPort);

            // Act
            var devices = await WebAPI_Get_Devices(_testServerPort);

            // Assert
            Assert.NotNull(devices);
            Assert.Empty(devices);
        }

        [Fact]
        public async Task GetStatus_Stopped_Test()
        {
            // Arrange
            _testServerPort++;
            List<DeviceConfiguration> deviceConfigurations = new List<DeviceConfiguration>();
            deviceConfigurations.Add(_deviceConfig1);
            SetupTestServer(deviceConfigurations, _testServerPort);

            // Act
            var status = await WebAPI_Get_Status(_testDevice1.Id,_testServerPort);

            // Assert
            Assert.NotNull(status);
            Assert.Equal(IoTStatus.Stopped, status.IoTStatus);
            Assert.Equal(DeviceStatus.Disconnected, status.DeviceStatus);
        }

        [Fact]
        public async Task GetStatus_Started_Test()
        {
            // Arrange
            _testServerPort++;
            List<DeviceConfiguration> deviceConfigurations = new List<DeviceConfiguration>();
            deviceConfigurations.Add(_deviceConfig1);
            SetupTestServer(deviceConfigurations, _testServerPort);

            var response = await WebAPI_Post_Start(_testServerPort);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            await AssertedGlobalIoTCoreStarted();


            // Act
            var status = await WebAPI_Get_Status(_testDevice1.Id, _testServerPort);

            // Assert
            Assert.NotNull(status);
            Assert.Equal(IoTStatus.Running, status.IoTStatus);
            Assert.True(status.DeviceStatus == DeviceStatus.Connecting || status.DeviceStatus == DeviceStatus.Connected);
        }

        [Fact]
        public async Task AddDevices_AddSingleDevice_Test()
        {
            // Arrange
            _testServerPort++;
            List<DeviceConfiguration> deviceConfigurations = new List<DeviceConfiguration>();
            SetupTestServer(deviceConfigurations, _testServerPort);

            var newDevices = new List<AddDeviceViewModel>() { new AddDeviceViewModel() {
                VseIpAddress = _deviceConfig1.VseIpAddress,
                VsePort = _deviceConfig1.VsePort,
                IoTCorePort = _deviceConfig1.IoTCorePort }
            };

            // Act
            var response = await WebAPI_Post_Devices(newDevices, _testServerPort);
            var devices = await WebAPI_Get_Devices(_testServerPort);

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
            _testServerPort++;
            List<DeviceConfiguration> deviceConfigurations = new List<DeviceConfiguration>();
            SetupTestServer(deviceConfigurations, _testServerPort);

            var newDevices = new List<AddDeviceViewModel>() { 
                new AddDeviceViewModel() {
                    VseIpAddress = _deviceConfig1.VseIpAddress,
                    VsePort = _deviceConfig1.VsePort,
                    IoTCorePort = _deviceConfig1.IoTCorePort },
                new AddDeviceViewModel() {
                    VseIpAddress = _deviceConfig2.VseIpAddress,
                    VsePort= _deviceConfig2.VsePort,
                    IoTCorePort= _deviceConfig2.IoTCorePort },
                new AddDeviceViewModel() {
                    VseIpAddress = _deviceConfig3.VseIpAddress,
                    VsePort= _deviceConfig3.VsePort,
                    IoTCorePort= _deviceConfig3.IoTCorePort },
            };

            // Act
            var response = await WebAPI_Post_Devices(newDevices, _testServerPort);
            var devices = await WebAPI_Get_Devices(_testServerPort);

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
            _testServerPort++;
            List<DeviceConfiguration> deviceConfigurations = new List<DeviceConfiguration>();
            SetupTestServer(deviceConfigurations, _testServerPort);

            var newDevices = new List<AddDeviceViewModel>() {
                new AddDeviceViewModel() {
                    VseIpAddress = _deviceConfig1.VseIpAddress,
                    VsePort = _deviceConfig1.VsePort,
                    IoTCorePort = _deviceConfig1.IoTCorePort },
                new AddDeviceViewModel() {
                    VseIpAddress = _deviceConfig1.VseIpAddress,
                    VsePort= _deviceConfig1.VsePort,
                    IoTCorePort= _deviceConfig1.IoTCorePort }
            };

            // Act
            var response = await WebAPI_Post_Devices(newDevices, _testServerPort);
            var devices = await WebAPI_Get_Devices(_testServerPort);

            // Assert
            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
            Assert.NotNull(devices);

   
        }

        [Fact]
        public async Task AddDevices_AlreadyExists_Test()
        {
            // Arrange
            _testServerPort++;
            List<DeviceConfiguration> deviceConfigurations = new List<DeviceConfiguration>();
            deviceConfigurations.Add(_deviceConfig1);
            SetupTestServer(deviceConfigurations, _testServerPort);

            var newDevices = new List<AddDeviceViewModel>() { new AddDeviceViewModel() {
                VseIpAddress = _deviceConfig1.VseIpAddress,
                VsePort = _deviceConfig1.VsePort,
                IoTCorePort = _deviceConfig1.IoTCorePort }
            };

            // Act
            var response = await WebAPI_Post_Devices(newDevices, _testServerPort);
            var devices = await WebAPI_Get_Devices(_testServerPort);

            // Assert
            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
            Assert.NotNull(devices);
            Assert.Single(devices);

        }

        [Fact]
        public async Task AddDevices_IoTCorePortAlreadyUsed_Test()
        {
            // Arrange
            _testServerPort++;
            List<DeviceConfiguration> deviceConfigurations = new List<DeviceConfiguration>();
            deviceConfigurations.Add(_deviceConfig1);
            SetupTestServer(deviceConfigurations, _testServerPort);


            var newDevices = new List<AddDeviceViewModel>() { new AddDeviceViewModel() {
                VseIpAddress = _deviceConfig3.VseIpAddress,
                VsePort = _deviceConfig3.VsePort,
                IoTCorePort = _deviceConfig1.IoTCorePort }
            };

            // Act
            var response = await WebAPI_Post_Devices(newDevices, _testServerPort);
            var devices = await WebAPI_Get_Devices(_testServerPort);

            // Assert
            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
            Assert.NotNull(devices);
            Assert.Single(devices);
        }

        [Fact]
        public async Task AddDevices_InvalidVseIpAddress_Test()
        {
            // Arrange
            _testServerPort++;
            List<DeviceConfiguration> deviceConfigurations = new List<DeviceConfiguration>();
            SetupTestServer(deviceConfigurations, _testServerPort);


            var newDevices = new List<AddDeviceViewModel>() { new AddDeviceViewModel() {
                VseIpAddress = "123456788",
                VsePort = _deviceConfig1.VsePort,
                IoTCorePort = _deviceConfig1.IoTCorePort }
            };

            // Act
            var response = await WebAPI_Post_Devices(newDevices, _testServerPort);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task AddDevices_InvalidVsePort_Test()
        {
            // Arrange
            _testServerPort++;
            List<DeviceConfiguration> deviceConfigurations = new List<DeviceConfiguration>();
            SetupTestServer(deviceConfigurations, _testServerPort);


            var newDevices = new List<AddDeviceViewModel>() { new AddDeviceViewModel() {
                VseIpAddress = _deviceConfig1.VseIpAddress,
                VsePort = 70000,
                IoTCorePort = _deviceConfig1.IoTCorePort }
            };

            // Act
            var response = await WebAPI_Post_Devices(newDevices, _testServerPort);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task AddDevices_InvalidIoTCorePort_Test()
        {
            // Arrange
            _testServerPort++;
            List<DeviceConfiguration> deviceConfigurations = new List<DeviceConfiguration>();
            SetupTestServer(deviceConfigurations, _testServerPort);


            var newDevices = new List<AddDeviceViewModel>() { new AddDeviceViewModel() {
                VseIpAddress = _deviceConfig1.VseIpAddress,
                VsePort = _deviceConfig1.VsePort,
                IoTCorePort = -1 }
            };

            // Act
            var response = await WebAPI_Post_Devices(newDevices, _testServerPort);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
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
            List<DeviceConfigurationViewModel> devices = new List<DeviceConfigurationViewModel>();
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

        private async Task AssertedGlobalIoTCoreStarted()
        {
            // Wait for the global IoTCore to start
            bool started = await IoTCoreUtils.WaitUntilGlobalIoTCoreStarted(_iotCoreOptions.Value.IoTCoreURI, _iotCoreOptions.Value.GlobalIoTCorePort);
            Assert.True(started);
        }

        private void Initialize()
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


            _deviceConfig3 = new DeviceConfiguration()
            {
                Id = _testDevice3.Id,
                VseType = _testDevice3.VseType,
                VseIpAddress = _testDevice3.VseIpAddress,
                VsePort = _testDevice3.VsePort,
                IoTCorePort = _testDevice3.IoTCorePort
            };
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

                        using (var scope = sp.CreateScope())
                        {
                            var scopedServices = scope.ServiceProvider;
                            var db = scopedServices.GetRequiredService<SQLiteDbContext>();
                            db.Database.EnsureCreated();
                            foreach (var deviceConfig in deviceConfigurations)
                            {
                                db.DeviceConfigurations.Add(deviceConfig);
                            }
                            db.SaveChangesAsync();
                        }
                    });
                });

            // Create HttpClient to access test server
            _httpClient = application.CreateClient();
        }

        public void Dispose()
        {
            _deviceConfig1 = null;
            _deviceConfig2 = null;
            _deviceConfig3 = null;
            _httpClient?.Dispose();
        }
    }
}