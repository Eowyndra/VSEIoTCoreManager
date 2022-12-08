// ----------------------------------------------------------------------------
// Filename: DeviceIoTCoreTest.cs
// Copyright (c) 2022 ifm diagnostic GmbH - All rights reserved.
// ----------------------------------------------------------------------------
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace VSEIoTCoreServer.UnitTests
{
    using System;
    using System.IO;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging.Abstractions;
    using Microsoft.Extensions.Options;
    using VSEIoTCoreServer.CommonTestUtils;
    using VSEIoTCoreServer.CommonUtils;
    using VSEIoTCoreServer.CommonUtils.ExtensionMethods;
    using VSEIoTCoreServer.DAL.Models.Enums;
    using VSEIoTCoreServer.WebApp;
    using VSEIoTCoreServer.WebApp.Models;
    using VSEIoTCoreServer.WebApp.ViewModels;
    using Xunit;

    [Collection("Sequential")]
    public class DeviceIoTCoreTest : IDisposable
    {
        private readonly TestDeviceOptions _testDevice1;
        private readonly TestDeviceOptions _testDevice2;
        private readonly TestDeviceOptions _testDevice3;
        private readonly IOptions<IoTCoreOptions> _iotCoreOptions;
        private readonly NullLoggerFactory _nullLoggerFactory;
        private readonly DeviceConfigurationViewModel _deviceConfig1;
        private readonly DeviceConfigurationViewModel _deviceConfig2;
        private DeviceIoTCore _deviceIoTCore;

        public DeviceIoTCoreTest()
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

            _nullLoggerFactory = new NullLoggerFactory();
        }

        [Fact]
        public void Ctor_Test()
        {
            Assert.NotNull(new DeviceIoTCore(_nullLoggerFactory, _iotCoreOptions, _deviceConfig1));
        }

        [Fact]
        public void Ctor_LoggerFactory_Null_Error_Test()
        {
            Assert.Throws<ArgumentNullException>("loggerFactory", () => new DeviceIoTCore(null, _iotCoreOptions, _deviceConfig1));
        }

        [Fact]
        public void Ctor_IoTCoreOptions_Null_Error_Test()
        {
            Assert.Throws<ArgumentNullException>("iotCoreOptions", () => new DeviceIoTCore(_nullLoggerFactory, null, _deviceConfig1));
        }

        [Fact]
        public void Ctor_Configuration_Null_Error_Test()
        {
            Assert.Throws<ArgumentNullException>("configuration", () => new DeviceIoTCore(_nullLoggerFactory, _iotCoreOptions, null));
        }

        [Fact]
        public async Task Start_Test()
        {
            // Arrange
            _deviceIoTCore = new DeviceIoTCore(_nullLoggerFactory, _iotCoreOptions, _deviceConfig1);

            // Act
            await _deviceIoTCore.Start();
            await AssertVSEIoTCoreStarted(_iotCoreOptions.Value.IoTCoreURI, _deviceConfig1.IoTCorePort);

            using var client = new Client(_iotCoreOptions.Value.IoTCoreURI + ":" + _deviceConfig1.IoTCorePort);
            var result = await client.SendRequestAndAwaitResponseAsync(IoTCoreRoutes.Device().Status().GetData());
            var message = IoTCoreUtils.CreateResponseMessage(result);
            var deviceStatus = message.Data.GetDeviceStatus();

            // Assert
            Assert.NotNull(message);
            Assert.Equal(200, message.Code);
            Assert.True(deviceStatus == DeviceStatus.Connected || deviceStatus == DeviceStatus.Connecting);

            // Finally
            await _deviceIoTCore.Stop();
            await AssertVSEIoTCoreStopped(_iotCoreOptions.Value.IoTCoreURI, _deviceConfig1.IoTCorePort);
        }

        [Fact]
        public async Task Stop_Test()
        {
            // Arrange
            _deviceIoTCore = new DeviceIoTCore(_nullLoggerFactory, _iotCoreOptions, _deviceConfig1);
            await _deviceIoTCore.Start();
            await AssertVSEIoTCoreStarted(_iotCoreOptions.Value.IoTCoreURI, _deviceConfig1.IoTCorePort);

            // Act
            await _deviceIoTCore.Stop();
            await AssertVSEIoTCoreStopped(_iotCoreOptions.Value.IoTCoreURI, _deviceConfig1.IoTCorePort);

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
        public async Task GetStatus_Disconnected_Test()
        {
            // Arrange
            _deviceIoTCore = new DeviceIoTCore(_nullLoggerFactory, _iotCoreOptions, _deviceConfig1);
            await AssertVSEIoTCoreStopped(_iotCoreOptions.Value.IoTCoreURI, _deviceConfig1.IoTCorePort);

            // Act
            var status = _deviceIoTCore.GetStatus();

            // Assert
            Assert.Equal(IoTStatus.Stopped, status.IoTStatus);
            Assert.Equal(DeviceStatus.Disconnected, status.DeviceStatus);
        }

        [Fact]
        public async Task GetStatus_Connecting_Test()
        {
            // Arrange
            var timeoutDevice = _deviceConfig2;
            timeoutDevice.VseIpAddress = "123.123.123.123";
            _deviceIoTCore = new DeviceIoTCore(_nullLoggerFactory, _iotCoreOptions, timeoutDevice);
            await _deviceIoTCore.Start();
            await AssertVSEIoTCoreStarted(_iotCoreOptions.Value.IoTCoreURI, timeoutDevice.IoTCorePort);

            // Wait 5 seconds to give the cache time to poll the current status
            Thread.Sleep(5000);

            // Act
            var status = _deviceIoTCore.GetStatus();

            // Assert
            Assert.Equal(IoTStatus.Started, status.IoTStatus);
            Assert.Equal(DeviceStatus.Connecting, status.DeviceStatus);

            // Finally
            await _deviceIoTCore.Stop();
            await AssertVSEIoTCoreStopped(_iotCoreOptions.Value.IoTCoreURI, timeoutDevice.IoTCorePort);
        }

        [Fact]
        public async Task GetStatus_Timeout_Test()
        {
            // Arrange
            var timeoutDevice = _deviceConfig2;
            timeoutDevice.VseIpAddress = "123.123.123.123";
            _deviceIoTCore = new DeviceIoTCore(_nullLoggerFactory, _iotCoreOptions, timeoutDevice);
            await _deviceIoTCore.Start();
            await AssertVSEIoTCoreStarted(_iotCoreOptions.Value.IoTCoreURI, timeoutDevice.IoTCorePort);

            // Wait 15 seconds for the device to time out
            Thread.Sleep(15000);

            // Act
            var status = _deviceIoTCore.GetStatus();

            // Assert
            Assert.Equal(IoTStatus.Started, status.IoTStatus);
            Assert.Equal(DeviceStatus.Timeout, status.DeviceStatus);

            // Finally
            await _deviceIoTCore.Stop();
            await AssertVSEIoTCoreStopped(_iotCoreOptions.Value.IoTCoreURI, timeoutDevice.IoTCorePort);
        }

        [Fact]
        public async Task GetStatus_Connected_Test()
        {
            // Arrange
            _deviceIoTCore = new DeviceIoTCore(_nullLoggerFactory, _iotCoreOptions, _deviceConfig1);
            await _deviceIoTCore.Start();
            var connected = await IoTCoreUtils.WaitUntilDeviceConnected(_iotCoreOptions.Value.IoTCoreURI, _deviceConfig1.IoTCorePort);
            Assert.True(connected);

            // Wait 5 seconds to give the cache time to poll the current status
            Thread.Sleep(5000);

            // Act
            var status = _deviceIoTCore.GetStatus();

            // Assert
            Assert.Equal(IoTStatus.Started, status.IoTStatus);
            Assert.Equal(DeviceStatus.Connected, status.DeviceStatus);
        }

        [Fact]
        public void GetConfiguration_Test()
        {
            // Arrange
            _deviceIoTCore = new DeviceIoTCore(_nullLoggerFactory, _iotCoreOptions, _deviceConfig1);

            // Act
            var configuration = _deviceIoTCore.GetDeviceConfiguration();

            // Assert
            Assert.NotNull(configuration);
            Assert.Equal(_deviceConfig1, configuration);
        }

        public void Dispose()
        {
            if (_deviceIoTCore != null && _deviceIoTCore.GetStatus().IoTStatus == IoTStatus.Started)
            {
                _deviceIoTCore?.Stop();
            }

            _deviceIoTCore?.Dispose();
            _deviceIoTCore = null;
        }

        private static async Task AssertVSEIoTCoreStarted(string iotCoreUri, int iotCorePort)
        {
            // Wait for the IoTCore to start
            var started = await IoTCoreUtils.WaitUntilVSEIoTCoreStarted(iotCoreUri, iotCorePort);
            Assert.True(started);
        }

        private static async Task AssertVSEIoTCoreStopped(string iotCoreUri, int iotCorePort)
        {
            // Wait for the IoTCore to stop
            var stopped = await IoTCoreUtils.WaitUntilVSEIoTCoreStopped(iotCoreUri, iotCorePort);
            Assert.True(stopped);
        }
    }
}
