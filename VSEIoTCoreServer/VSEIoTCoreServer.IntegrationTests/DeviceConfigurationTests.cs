using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using VSEIoTCoreServer.DAL;
using VSEIoTCoreServer.DAL.Models;
using VSEIoTCoreServer.Services;
using VSEIoTCoreServer.ViewModels;
using Xunit;

namespace VSEIoTCoreServer.UnitTest
{
    public class DeviceConfigurationTests
    {
        private readonly DeviceConfiguration _deviceConfig1 = new DeviceConfiguration()
        {
            Id = 13,
            VseType = "VSE100",
            VseIpAddress = "172.29.12.22",
            VsePort = 3321,
            IoTCorePort = 8092
        };

        private readonly DeviceConfiguration _deviceConfig2 = new DeviceConfiguration()
        {
            Id = 27,
            VseType = "VSE100",
            VseIpAddress = "172.29.12.23",
            VsePort = 3321,
            IoTCorePort = 8093
        };

        [Fact]
        public async Task DeviceConfigurationService_GetAll_Test()
        {
            // create AutoMapper as a dependency for the service
            var myProfile = new AutoMapperProfile();
            var configuration = new MapperConfiguration(cfg => cfg.AddProfile(myProfile));
            var mapper = new Mapper(configuration);

            // create DbContext as a dependency for the service
            var context = GetInMemoryDatabaseContext().Result;


            // instanciate DeviceConfigurationService
            var deviceConfigurationService = new DeviceConfigurationService(mapper, context);

            var devices = await deviceConfigurationService.GetAll();

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
        public async Task DeviceConfigurationService_GetDeviceConfiguration_Test()
        {
            // create AutoMapper as a dependency for the service
            var myProfile = new AutoMapperProfile();
            var configuration = new MapperConfiguration(cfg => cfg.AddProfile(myProfile));
            var mapper = new Mapper(configuration);

            // create DbContext as a dependency for the service
            var context = await GetInMemoryDatabaseContext();

            // instanciate DeviceConfigurationService
            var deviceConfigurationService = new DeviceConfigurationService(mapper, context);
            var device = await deviceConfigurationService.GetById(_deviceConfig1.Id);

            Assert.NotNull(device);
            Assert.Equal(_deviceConfig1.Id, device.Id);
            Assert.Equal(_deviceConfig1.VseType, device.VseType);
            Assert.Equal(_deviceConfig1.VseIpAddress, device.VseIpAddress);
            Assert.Equal(_deviceConfig1.VsePort, device.VsePort);
            Assert.Equal(_deviceConfig1.IoTCorePort, device.IoTCorePort);
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
    }
}
