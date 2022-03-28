using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System.IO;
using Xunit;
using System;
using VSEIoTCoreServer.WebApp;
using VSEIoTCoreServer.CommonTestUtils;
using VSEIoTCoreServer.DAL.Models;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using System.Linq;
using VSEIoTCoreServer.DAL;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net;
using VSEIoTCoreServer.CommonUtils;
using VSEIoTCoreServer.WebApp.ViewModels;
using Newtonsoft.Json;
using VSEIoTCoreServer.DAL.Models.Enums;

namespace VSEIoTCoreServer.IntegrationTests
{
    [Collection("Sequential")]
    public class GlobalIoTCoreAPITests : IDisposable
    {
        private readonly TestDeviceOptions _testDevice1;
        private readonly TestDeviceOptions _testDevice2;
        private readonly TestDeviceOptions _testDevice3;
        private readonly IOptions<IoTCoreOptions> _iotCoreOptions;

        private DeviceConfiguration _deviceConfig1;
        private DeviceConfiguration _deviceConfig2;
        private DeviceConfiguration _deviceConfig3;
        private HttpClient _httpClient;
        private static int _testServerPort = 5050; // Using a different port for the test server in each test to allow parallel testing


        public GlobalIoTCoreAPITests()
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
        public async Task Start_Test()
        {
            // Arrange 
            _testServerPort++;
            List<DeviceConfiguration> deviceConfigurations = new List<DeviceConfiguration>();
            deviceConfigurations.Add(_deviceConfig1);
            deviceConfigurations.Add(_deviceConfig2);
            SetupTestServer(deviceConfigurations, _testServerPort);

            // Act
            var response = await WebAPI_Post_Start(_testServerPort);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            await AssertedGlobalIoTCoreStarted();

            // Finally
            response = await WebAPI_Post_Stop(_testServerPort);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            await AssertedGlobalIoTCoreStopped();
        }

        [Fact]
        public async Task Stop_Test()
        {
            // Arrange 
            _testServerPort++;
            List<DeviceConfiguration> deviceConfigurations = new List<DeviceConfiguration>();
            deviceConfigurations.Add(_deviceConfig1);
            deviceConfigurations.Add(_deviceConfig2);
            SetupTestServer(deviceConfigurations, _testServerPort);

            var response = await WebAPI_Post_Start(_testServerPort);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            await AssertedGlobalIoTCoreStarted();

            // Act
            response = await WebAPI_Post_Stop(_testServerPort);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            await AssertedGlobalIoTCoreStopped();
        }

        [Fact]
        public async Task GetStatus_Stopped_Test()
        {
            // Arrange 
            _testServerPort++;
            List<DeviceConfiguration> deviceConfigurations = new List<DeviceConfiguration>();
            deviceConfigurations.Add(_deviceConfig1);
            deviceConfigurations.Add(_deviceConfig2);
            SetupTestServer(deviceConfigurations, _testServerPort);

            var response = await WebAPI_Post_Start(_testServerPort);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            await AssertedGlobalIoTCoreStarted();

            response = await WebAPI_Post_Stop(_testServerPort);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            await AssertedGlobalIoTCoreStopped();

            // Act
            response = await WebAPI_Get_Status(_testServerPort);
            var globalIoTCoreStatus = JsonConvert.DeserializeObject<GlobalIoTCoreStatusViewModel>(await response.Content.ReadAsStringAsync());

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(GlobalIoTCoreStatus.Stopped, globalIoTCoreStatus.Status);
        }

        [Fact]
        public async Task GetStatus_Started_Test()
        {
            // Arrange 
            _testServerPort++;
            List<DeviceConfiguration> deviceConfigurations = new List<DeviceConfiguration>();
            deviceConfigurations.Add(_deviceConfig1);
            deviceConfigurations.Add(_deviceConfig2);
            SetupTestServer(deviceConfigurations, _testServerPort);
            
            await WebAPI_Post_Start(_testServerPort);
            await AssertedGlobalIoTCoreStarted();

            // Act
            var response = await WebAPI_Get_Status(_testServerPort);
            var globalIoTCoreStatus = JsonConvert.DeserializeObject<GlobalIoTCoreStatusViewModel>(await response.Content.ReadAsStringAsync());

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.True(globalIoTCoreStatus.Status == GlobalIoTCoreStatus.Running || globalIoTCoreStatus.Status == GlobalIoTCoreStatus.PartlyRunning);

            // Finally
            response = await WebAPI_Post_Stop(_testServerPort);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            await AssertedGlobalIoTCoreStopped();
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

        private async Task<HttpResponseMessage> WebAPI_Get_Status(int port)
        {
            var response = await _httpClient.GetAsync($"https://localhost:{port}/api/v1/Global/status");
            return response;
        }

        private async Task AssertedGlobalIoTCoreStarted()
        {
            // Wait for the global IoTCore to start
            bool started = await IoTCoreUtils.WaitUntilGlobalIoTCoreStarted(_iotCoreOptions.Value.IoTCoreURI, _iotCoreOptions.Value.GlobalIoTCorePort);
            Assert.True(started);
        }

        private async Task AssertedGlobalIoTCoreStopped()
        {
            // Wait for the global IoTCore to stop
            bool stopped = await IoTCoreUtils.WaitUntilGlobalIoTCoreStopped(_iotCoreOptions.Value.IoTCoreURI, _iotCoreOptions.Value.GlobalIoTCorePort);
            Assert.True(stopped);
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
