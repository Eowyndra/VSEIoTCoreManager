// ----------------------------------------------------------------------------
// Filename: GlobalControllerTest.cs
// Copyright (c) 2022 ifm diagnostic GmbH - All rights reserved.
// ----------------------------------------------------------------------------
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace VSEIoTCoreServer.UnitTests
{
    using System;
    using Microsoft.Extensions.Logging;
    using Moq;
    using VSEIoTCoreServer.WebApp.Controllers;
    using VSEIoTCoreServer.WebApp.Models;
    using Xunit;

    public class GlobalControllerTest : IDisposable
    {
        private readonly IIoTCoreServer _iotCoreServerMock;
        private readonly ILoggerFactory _loggerFactoryMock;

        public GlobalControllerTest()
        {
            _iotCoreServerMock = new Mock<IIoTCoreServer>().Object;
            _loggerFactoryMock = new Mock<ILoggerFactory>().Object;
        }

        [Fact]
        public void Ctor_Test()
        {
            Assert.NotNull(new GlobalController(_iotCoreServerMock, _loggerFactoryMock));
        }

        [Fact]
        public void Ctor_IoTCoreServer_Null_Error_Test()
        {
            Assert.Throws<ArgumentNullException>("iotCoreServer", () => new GlobalController(null, _loggerFactoryMock));
        }

        [Fact]
        public void Ctor_LoggerFactory_Null_Error_Test()
        {
            Assert.Throws<ArgumentNullException>("loggerFactory", () => new GlobalController(_iotCoreServerMock, null));
        }

        public void Dispose()
        {
        }
    }
}
