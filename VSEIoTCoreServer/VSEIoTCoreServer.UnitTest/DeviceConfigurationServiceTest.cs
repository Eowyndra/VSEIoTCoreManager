using Xunit;
using Moq;
using VSEIoTCoreServer.WebApp.Services;
using System;
using AutoMapper;
using VSEIoTCoreServer.DAL;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VSEIoTCoreServer.WebApp;
using System.Threading.Tasks;
using VSEIoTCoreServer.CommonTestUtils;
using VSEIoTCoreServer.DAL.Models;
using Microsoft.EntityFrameworkCore;
using VSEIoTCoreServer.WebApp.ViewModels;
using Microsoft.Extensions.Configuration;
using System.IO;
using Microsoft.Extensions.Logging.Abstractions;
using System.Collections.Generic;
using Xunit.Sdk;

namespace VSEIoTCoreServer.UnitTest
{
    [Collection("Sequential")]
    public class DeviceConfigurationServiceTest
    {
        private readonly TestDeviceOptions _testDevice1;
        private readonly TestDeviceOptions _testDevice2;
        private readonly TestDeviceOptions _testDevice3;
        private DeviceConfiguration _deviceConfig1;
        private DeviceConfiguration _deviceConfig2;
        private DeviceConfiguration _deviceConfig3;
        private IMapper _mapper;
        private NullLoggerFactory _nullLoggerFactory;
        private SQLiteDbContext _dbContext;
        private DeviceConfigurationService _deviceConfigurationService;

        private readonly IMapper _mapperMock;
        private readonly SQLiteDbContext _dbContextMock;
        private readonly ILoggerFactory _loggerFactoryMock;

        public DeviceConfigurationServiceTest()
        {
            var configuration = new ConfigurationBuilder()
               .SetBasePath(Directory.GetCurrentDirectory())
               .AddJsonFile(@"appsettings.Test.json", false, false)
               .AddEnvironmentVariables()
               .Build();

            _testDevice1 = new TestDeviceOptions();
            configuration.GetSection("TestDevices:TestDevice1").Bind(_testDevice1);
            _testDevice2 = new TestDeviceOptions();
            configuration.GetSection("TestDevices:TestDevice2").Bind(_testDevice2);
            _testDevice3 = new TestDeviceOptions();
            configuration.GetSection("TestDevices:TestDevice3").Bind(_testDevice3);

            _mapperMock = new Mock<IMapper>().Object;
            _dbContextMock = new Mock<SQLiteDbContext>().Object;
            _loggerFactoryMock = new Mock<ILoggerFactory>().Object;
        }

        [Fact]
        public void Ctor_Test()
        {
            Assert.NotNull(new DeviceConfigurationService(
                _mapperMock, 
                _dbContextMock, 
                _loggerFactoryMock));
        }

        [Fact]
        public void Ctor_Mapper_Null_Error_Test()
        {
            Assert.Throws<ArgumentNullException>("mapper", () => new DeviceConfigurationService(
                null,
                _dbContextMock,
                _loggerFactoryMock));
        }


        [Fact]
        public void Ctor_Context_Null_Error_Test()
        {
            Assert.Throws<ArgumentNullException>("context", () => new DeviceConfigurationService(
                _mapperMock,
                null,
                _loggerFactoryMock));
        }

        [Fact]
        public void Ctor_Logger_Null_Error_Test()
        {
            Assert.Throws<ArgumentNullException>("loggerFactory", () => new DeviceConfigurationService(
                _mapperMock,
                _dbContextMock,
                null));
        }

        [Fact]
        public async Task GetAll_Test()
        {
            // Arrange
            Arrange();

            // Act
            var devices = await _deviceConfigurationService.GetAll();

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
            Arrange();
            var context = await GetInMemoryDatabaseContext_EmptyList();

            // Act
            var deviceConfigurationService = new DeviceConfigurationService(_mapper, context, _nullLoggerFactory);
            var devices = await deviceConfigurationService.GetAll();

            // Assert
            Assert.NotNull(devices);
            Assert.Empty(devices);
        }

        [Fact]
        public async Task GetById_Test()
        {
            // Arrange
            Arrange();

            // Act
            var device = await _deviceConfigurationService.GetById(_deviceConfig1.Id);

            // Assert
            Assert.NotNull(device);
            Assert.Equal(_deviceConfig1.Id, device.Id);
            Assert.Equal(_deviceConfig1.VseType, device.VseType);
            Assert.Equal(_deviceConfig1.VseIpAddress, device.VseIpAddress);
            Assert.Equal(_deviceConfig1.VsePort, device.VsePort);
            Assert.Equal(_deviceConfig1.IoTCorePort, device.IoTCorePort);
        }

        [Fact]
        public async Task GetById_NotFound_Test()
        {
            // Arrange
            Arrange();

            // Act
            var device = await _deviceConfigurationService.GetById(_deviceConfig3.Id);

            // Assert
            Assert.Null(device);
        }

        [Fact]
        public async Task AddDevices_Test()
        {
            // Arrange
            Arrange();

            var newDevices = new List<AddDeviceViewModel>() { new AddDeviceViewModel() {
                VseIpAddress = _deviceConfig3.VseIpAddress,
                VsePort = _deviceConfig3.VsePort,
                IoTCorePort = _deviceConfig3.IoTCorePort }
            };

            // Act
            var addedDevices = await _deviceConfigurationService.AddDevices(newDevices);
            var devices = await _deviceConfigurationService.GetAll();

            // Assert
            Assert.NotNull(addedDevices);
            Assert.Single(addedDevices);
            Assert.NotNull(devices);
            Assert.Equal(3, devices.Count);

            var deviceConfig3 = devices[2];
            Assert.Equal(_deviceConfig3.VseIpAddress, deviceConfig3.VseIpAddress);
            Assert.Equal(_deviceConfig3.VsePort, deviceConfig3.VsePort);
            Assert.Equal(_deviceConfig3.IoTCorePort, deviceConfig3.IoTCorePort);
        }

        [Fact]
        public async Task AddDevices_AlreadyExists_Test()
        {
            // Arrange
            Arrange();

            var newDevices = new List<AddDeviceViewModel>() { new AddDeviceViewModel() {
                VseIpAddress = _deviceConfig1.VseIpAddress,
                VsePort = _deviceConfig1.VsePort,
                IoTCorePort = _deviceConfig1.IoTCorePort }
            };

            try
            {
                // Act
                var addedDevice = await _deviceConfigurationService.AddDevices(newDevices);
            }
            catch (ArgumentException ex)
            {
                // Assert
                Assert.Equal("Device already exists!", ex.Message);
                return;
            }
            throw new XunitException("No Exception thrown");
        }

        [Fact]
        public async Task AddDevices_IoTCorePortAlreadyUsed_Test()
        {
            // Arrange
            Arrange();

            var newDevices = new List<AddDeviceViewModel>() { new AddDeviceViewModel() {
                VseIpAddress = _deviceConfig3.VseIpAddress,
                VsePort = _deviceConfig3.VsePort,
                IoTCorePort = _deviceConfig1.IoTCorePort }
            };

            try
            {
                // Act
                var addedDevice = await _deviceConfigurationService.AddDevices(newDevices);
            }
            catch (ArgumentException ex)
            {
                // Assert
                Assert.Equal("IoTCore port already being used!", ex.Message);
                return;
            }
            throw new XunitException("No Exception thrown");

        }


        private void Arrange()
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

            var myProfile = new AutoMapperProfile();
            var configuration = new MapperConfiguration(cfg => cfg.AddProfile(myProfile));
            _mapper = new Mapper(configuration);
            _dbContext = GetInMemoryDatabaseContext().Result;
            _nullLoggerFactory = new NullLoggerFactory();
            _deviceConfigurationService = new DeviceConfigurationService(_mapper, _dbContext, _nullLoggerFactory);
        }

        private async Task<SQLiteDbContext> GetInMemoryDatabaseContext()
        {
            var options = new DbContextOptionsBuilder<SQLiteDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var databaseContext = new SQLiteDbContext(options);
            databaseContext.Database.EnsureCreated();
            if (await databaseContext.DeviceConfigurations.CountAsync() <= 0)
            {
                databaseContext.DeviceConfigurations.Add(_deviceConfig1);
                databaseContext.DeviceConfigurations.Add(_deviceConfig2);
                await databaseContext.SaveChangesAsync();
            }
            return databaseContext;
        }

        private async Task<SQLiteDbContext> GetInMemoryDatabaseContext_EmptyList()
        {
            var options = new DbContextOptionsBuilder<SQLiteDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var databaseContext = new SQLiteDbContext(options);
            databaseContext.Database.EnsureCreated();
            await databaseContext.SaveChangesAsync();
            return databaseContext;
        }
    }
}