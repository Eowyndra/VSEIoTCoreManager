// ----------------------------------------------------------------------------
// Filename: GlobalControllerTest.cs
// Copyright (c) 2022 ifm diagnostic GmbH - All rights reserved.
// ----------------------------------------------------------------------------
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace VSEIoTCoreServer.UnitTest
{
    using System;
    using Microsoft.Extensions.Logging;
    using Moq;
    using VSEIoTCoreServer.WebApp.Controllers;
    using VSEIoTCoreServer.WebApp.Services;
    using Xunit;

    public class GlobalControllerTest : IDisposable
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

        public void Dispose()
        {
        }
    }
}