using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Threading.Tasks;
using VSEIoTCoreServer.DAL;
using VSEIoTCoreServer.DAL.Models;
using VSEIoTCoreServer.WebApp;
using VSEIoTCoreServer.WebApp.Services;
using VSEIoTCoreServer.WebApp.ViewModels;
using Xunit;

namespace VSEIoTCoreServer.IntegrationTests
{
    [Collection("Sequential")]
    public class DeviceConfigurationTests
    {
        private readonly TestDeviceOptions _testDevice1;
        private readonly TestDeviceOptions _testDevice2;
        private readonly IOptions<IoTCoreOptions> _iotCoreOptions;
        private DeviceConfiguration _deviceConfig1;
        private DeviceConfiguration _deviceConfig2;
        private IMapper _mapper;
        private NullLoggerFactory _nullLoggerFactory;
        private SQLiteDbContext _dbContext;

        public DeviceConfigurationTests()
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

            var myProfile = new AutoMapperProfile();
            var configuration = new MapperConfiguration(cfg => cfg.AddProfile(myProfile));
            _mapper = new Mapper(configuration);
            _dbContext = GetInMemoryDatabaseContext().Result;
            _nullLoggerFactory = new NullLoggerFactory();
        }

        [Fact]
        public async Task DeviceConfigurationService_GetAll_Test()
        {
            // Arrange
            Arrange();

            // Act
            var deviceConfigurationService = new DeviceConfigurationService(_mapper, _dbContext, _nullLoggerFactory);
            var devices = await deviceConfigurationService.GetAll();

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
        public async Task DeviceConfigurationService_GetById_Test()
        {
            // Arrange
            Arrange();

            // Act
            var deviceConfigurationService = new DeviceConfigurationService(_mapper, _dbContext, _nullLoggerFactory);
            var device = await deviceConfigurationService.GetById(_deviceConfig1.Id);

            // Assert
            Assert.NotNull(device);
            Assert.Equal(_deviceConfig1.Id, device.Id);
            Assert.Equal(_deviceConfig1.VseType, device.VseType);
            Assert.Equal(_deviceConfig1.VseIpAddress, device.VseIpAddress);
            Assert.Equal(_deviceConfig1.VsePort, device.VsePort);
            Assert.Equal(_deviceConfig1.IoTCorePort, device.IoTCorePort);
        }


        [Fact]
        public async Task DeviceConfigurationService_EmptyDeviceList_Test()
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
