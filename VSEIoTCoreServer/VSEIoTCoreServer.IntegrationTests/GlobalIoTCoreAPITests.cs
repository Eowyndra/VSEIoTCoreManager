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
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc.Testing;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
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
            SetupTestServer(deviceConfigurations, testServerPort);

            // Act
            var response = await WebAPI_Post_Start(testServerPort);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            await AssertedGlobalIoTCoreStarted();

            // Finally
            response = await WebAPI_Post_Stop(testServerPort);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            await AssertedGlobalIoTCoreStopped();
        }

        [Fact]
        public async Task Stop_Test()
        {
            // Arrange
            var testServerPort = 5202;
            var deviceConfigurations = new List<DeviceConfiguration> { _deviceConfig1, _deviceConfig2 };
            SetupTestServer(deviceConfigurations, testServerPort);

            var response = await WebAPI_Post_Start(testServerPort);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            await AssertedGlobalIoTCoreStarted();

            // Act
            response = await WebAPI_Post_Stop(testServerPort);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            await AssertedGlobalIoTCoreStopped();
        }

        [Fact]
        public async Task GetStatus_Stopped_Test()
        {
            // Arrange
            var testServerPort = 5203;
            var deviceConfigurations = new List<DeviceConfiguration> { _deviceConfig1, _deviceConfig2 };
            SetupTestServer(deviceConfigurations, testServerPort);

            var response = await WebAPI_Post_Start(testServerPort);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            await AssertedGlobalIoTCoreStarted();

            response = await WebAPI_Post_Stop(testServerPort);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            await AssertedGlobalIoTCoreStopped();

            // Act
            response = await WebAPI_Get_Status(testServerPort);
            var globalIoTCoreStatus = JsonConvert.DeserializeObject<GlobalIoTCoreStatusViewModel>(await response.Content.ReadAsStringAsync());

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(GlobalIoTCoreStatus.Stopped, globalIoTCoreStatus.Status);
        }

        [Fact]
        public async Task GetStatus_Started_Test()
        {
            // Arrange
            var testServerPort = 5204;
            var deviceConfigurations = new List<DeviceConfiguration> { _deviceConfig1, _deviceConfig2 };
            SetupTestServer(deviceConfigurations, testServerPort);

            await WebAPI_Post_Start(testServerPort);
            await AssertedGlobalIoTCoreStarted();

            // Act
            var response = await WebAPI_Get_Status(testServerPort);
            var globalIoTCoreStatus = JsonConvert.DeserializeObject<GlobalIoTCoreStatusViewModel>(await response.Content.ReadAsStringAsync());

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.True(globalIoTCoreStatus.Status == GlobalIoTCoreStatus.Started || globalIoTCoreStatus.Status == GlobalIoTCoreStatus.PartlyRunning);

            // Finally
            response = await WebAPI_Post_Stop(testServerPort);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            await AssertedGlobalIoTCoreStopped();
        }

        public void Dispose()
        {
            _deviceConfig1 = null;
            _deviceConfig2 = null;
            _httpClient?.Dispose();
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
            var started = await IoTCoreUtils.WaitUntilGlobalIoTCoreStarted(_iotCoreOptions.Value.IoTCoreURI, _iotCoreOptions.Value.GlobalIoTCorePort);
            Assert.True(started);
        }

        private async Task AssertedGlobalIoTCoreStopped()
        {
            // Wait for the global IoTCore to stop
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
