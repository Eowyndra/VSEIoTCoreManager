using Xunit;
using Moq;
using VSEIoTCoreServer.Services;
using VSEIoTCoreServer.Controllers;
using System;
using Microsoft.Extensions.Logging;

namespace VSEIoTCoreServer.UnitTest
{
    public class GlobalControllerTest
    {
        [Fact]
        public void Ctor_Test()
        {
            var mockGlobalIoTCoreService = new Mock<IGlobalIoTCoreService>().Object;
            var mockLoggerFactory = new Mock<ILoggerFactory>().Object;
            Assert.NotNull(new GlobalController(mockGlobalIoTCoreService, mockLoggerFactory));
        }

        [Fact]
        public void Ctor_Service_Null_Error_Test()
        {
            var mockLoggerFactory = new Mock<ILoggerFactory>().Object;
            Assert.Throws<ArgumentNullException>("globalIoTCoreService", () => new GlobalController(null, mockLoggerFactory));
        }

        [Fact]
        public void Ctor_Logger_Null_Error_Test()
        {
            var mockGlobalIoTCoreService = new Mock<IGlobalIoTCoreService>().Object;
            Assert.Throws<ArgumentNullException>("loggerFactory", () => new GlobalController(mockGlobalIoTCoreService, null));
        }


    }
}