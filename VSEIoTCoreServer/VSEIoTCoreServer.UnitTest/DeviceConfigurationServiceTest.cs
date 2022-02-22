using Xunit;
using Moq;
using VSEIoTCoreServer.Services;
using System;
using AutoMapper;
using VSEIoTCoreServer.DAL;

namespace VSEIoTCoreServer.IntegrationTests
{
    public class DeviceConfigurationServiceTest
    {
        [Fact]
        public void Ctor_Test()
        {
            var mapper = new Mock<IMapper>().Object;
            var dbContext = new Mock<SQLiteDbContext>().Object;
            Assert.NotNull(new DeviceConfigurationService(mapper, dbContext));
        }

        [Fact]
        public void Ctor_Mapper_Null_Error_Test()
        {
            var dbContext = new Mock<SQLiteDbContext>().Object;
            Assert.Throws<ArgumentNullException>("mapper", () => new DeviceConfigurationService(null, dbContext));
        }


        [Fact]
        public void Ctor_Context_Null_Error_Test()
        {
            var mapper = new Mock<IMapper>().Object;
            Assert.Throws<ArgumentNullException>("context", () => new DeviceConfigurationService(mapper, null));
        }


    }
}