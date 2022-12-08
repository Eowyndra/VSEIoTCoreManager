// ----------------------------------------------------------------------------
// Filename: TestUtils.cs
// Copyright (c) 2022 ifm diagnostic GmbH - All rights reserved.
// ----------------------------------------------------------------------------
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace VSEIoTCoreServer.CommonTestUtils
{
    using System.Net.Http.Headers;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc.Testing;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using Newtonsoft.Json;
    using VSEIoTCoreServer.DAL;
    using VSEIoTCoreServer.DAL.Models;
    using VSEIoTCoreServer.DAL.Models.Enums;
    using VSEIoTCoreServer.WebApp;
    using VSEIoTCoreServer.WebApp.ViewModels;

    public class TestUtils
    {
        public static DeviceConfiguration GetDeviceConfiguration(TestDeviceOptions testDevice)
        {
            if (testDevice == null)
            {
                throw new ArgumentNullException(nameof(testDevice));
            }

            var deviceConfig = new DeviceConfiguration()
            {
                Id = testDevice.Id,
                Name = testDevice.Name,
                VseType = testDevice.VseType,
                VseIpAddress = testDevice.VseIpAddress,
                VsePort = testDevice.VsePort,
                IoTCorePort = testDevice.IoTCorePort,
            };

            return deviceConfig;
        }

        public static DeviceConfigurationViewModel GetDeviceConfigurationViewModel(TestDeviceOptions testDevice)
        {
            if (testDevice == null)
            {
                throw new ArgumentNullException(nameof(testDevice));
            }

            var deviceConfigViewModel = new DeviceConfigurationViewModel(testDevice.VseIpAddress, testDevice.VsePort, testDevice.IoTCorePort, testDevice.Name)
            {
                Id = testDevice.Id,
                VseType = testDevice.VseType,
            };

            return deviceConfigViewModel;
        }

        public static async Task<HttpResponseMessage> WebAPI_Post_Start_Global(HttpClient client, int port)
        {
            if (client == null)
            {
                throw new ArgumentException("null", nameof(client));
            }

            var response = await client.PostAsync($"https://localhost:{port}/api/v1/Global/start", null);
            return response;
        }

        public static async Task<HttpResponseMessage> WebAPI_Post_Stop_Global(HttpClient client, int port)
        {
            if (client == null)
            {
                throw new ArgumentException("null", nameof(client));
            }

            var response = await client.PostAsync($"https://localhost:{port}/api/v1/Global/stop", null);
            return response;
        }

        public static async Task<GlobalIoTCoreStatusViewModel> WebAPI_Get_Status_Global(HttpClient client, int port)
        {
            if (client == null)
            {
                throw new ArgumentException("null", nameof(client));
            }

            GlobalIoTCoreStatusViewModel status = new ();
            var response = await client.GetAsync($"https://localhost:{port}/api/v1/Global/status");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                if (content != null)
                {
                    status = JsonConvert.DeserializeObject<GlobalIoTCoreStatusViewModel>(content) ?? new ();
                }
            }

            return status;
        }

        public static async Task<StatusViewModel> WebAPI_Get_Status_Device(HttpClient client, int deviceId, int port)
        {
            if (client == null)
            {
                throw new ArgumentException("null", nameof(client));
            }

            StatusViewModel status = new ();
            var response = await client.GetAsync($"https://localhost:{port}/api/v1/Device/{deviceId}/status");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                if (content != null)
                {
                    status = JsonConvert.DeserializeObject<StatusViewModel>(content) ?? new ();
                }
            }

            return status;
        }

        public static async Task<bool> WaitUntilGlobalStatus(HttpClient client, int port, GlobalIoTCoreStatus status, int maxWaitInMilliseconds = 60_000)
        {
            var result = false;
            while (maxWaitInMilliseconds > 0)
            {
                var currentStatus = await WebAPI_Get_Status_Global(client, port);
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

        public static async Task<bool> WaitUntilDeviceStatus(HttpClient client, int deviceId, int port, DeviceStatus status, int maxWaitInMilliseconds = 60_000)
        {
            var result = false;
            while (maxWaitInMilliseconds > 0)
            {
                var currentStatus = await WebAPI_Get_Status_Device(client, deviceId, port);
                if (currentStatus.DeviceStatus == status)
                {
                    result = true;
                    break;
                }

                Thread.Sleep(500); // waiting 500 ms, then re-checking
                maxWaitInMilliseconds -= 500;
            }

            return result;
        }

        public static async Task<bool> WaitUntilIoTStatus(HttpClient client, int deviceId, int port, IoTStatus status, int maxWaitInMilliseconds = 60_000)
        {
            var result = false;
            while (maxWaitInMilliseconds > 0)
            {
                var currentStatus = await WebAPI_Get_Status_Device(client, deviceId, port);
                if (currentStatus.IoTStatus == status)
                {
                    result = true;
                    break;
                }

                Thread.Sleep(500); // waiting 500 ms, then re-checking
                maxWaitInMilliseconds -= 500;
            }

            return result;
        }

        public static async Task<HttpResponseMessage> WebAPI_Post_Devices_Global(HttpClient client, List<AddDeviceViewModel> newDevices, int port)
        {
            if (client == null)
            {
                throw new ArgumentException("null", nameof(client));
            }

            var jsonContent = JsonConvert.SerializeObject(newDevices);
            var buffer = System.Text.Encoding.UTF8.GetBytes(jsonContent);
            var byteContent = new ByteArrayContent(buffer);
            byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            var response = await client.PostAsync($"https://localhost:{port}/api/v1/Device", byteContent);
            return response;
        }

        public static async Task<List<DeviceConfigurationViewModel>> WebAPI_Get_Devices_Global(HttpClient client, int port)
        {
            if (client == null)
            {
                throw new ArgumentException("null", nameof(client));
            }

            var devices = new List<DeviceConfigurationViewModel>();
            var response = await client.GetAsync($"https://localhost:{port}/api/v1/Device");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                devices = JsonConvert.DeserializeObject<List<DeviceConfigurationViewModel>>(content) ?? new List<DeviceConfigurationViewModel>();
            }

            return devices;
        }

        public static HttpClient SetupTestServer(List<DeviceConfiguration> deviceConfigurations, int port)
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
                        if (descriptor != null)
                        {
                            services.Remove(descriptor);
                        }

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
            return application.CreateClient();
        }
    }
}
