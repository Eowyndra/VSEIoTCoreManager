using Xunit;
using Moq;
using VSEIoTCoreServer.WebApp.Services;
using System;
using AutoMapper;
using VSEIoTCoreServer.DAL;
using Microsoft.Extensions.Logging;

namespace VSEIoTCoreServer.UnitTest
{
    public class DeviceConfigurationServiceTest
    {
        [Fact]
        public void Ctor_Test()
        {
            var mockMapper = new Mock<IMapper>().Object;
            var mockDbContext = new Mock<SQLiteDbContext>().Object;
            var mockLoggerFactory = new Mock<ILoggerFactory>().Object;
            Assert.NotNull(new DeviceConfigurationService(mockMapper, mockDbContext, mockLoggerFactory));
        }

        [Fact]
        public void Ctor_Mapper_Null_Error_Test()
        {
            var mockDbContext = new Mock<SQLiteDbContext>().Object;
            var mockLoggerFactory = new Mock<ILoggerFactory>().Object;
            Assert.Throws<ArgumentNullException>("mapper", () => new DeviceConfigurationService(null, mockDbContext, mockLoggerFactory));
        }


        [Fact]
        public void Ctor_Context_Null_Error_Test()
        {
            var mockMapper = new Mock<IMapper>().Object;
            var mockLoggerFactory = new Mock<ILoggerFactory>().Object;
            Assert.Throws<ArgumentNullException>("context", () => new DeviceConfigurationService(mockMapper, null, mockLoggerFactory));
        }

        [Fact]
        public void Ctor_Logger_Null_Error_Test()
        {
            var mockMapper = new Mock<IMapper>().Object;
            var mockDbContext = new Mock<SQLiteDbContext>().Object;
            Assert.Throws<ArgumentNullException>("loggerFactory", () => new DeviceConfigurationService(mockMapper, mockDbContext, null));
        }


    }
}