// ----------------------------------------------------------------------------
// Filename: GlobalConfigurationServiceTest.cs
// Copyright (c) 2022 ifm diagnostic GmbH - All rights reserved.
// ----------------------------------------------------------------------------
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace VSEIoTCoreServer.UnitTests
{
    using System;
    using System.Threading.Tasks;
    using AutoMapper;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Abstractions;
    using Moq;
    using VSEIoTCoreServer.DAL;
    using VSEIoTCoreServer.DAL.Models;
    using VSEIoTCoreServer.WebApp.Services;
    using VSEIoTCoreServer.WebApp.ViewModels;
    using Xunit;

    [Collection("Sequential")]
    public class GlobalConfigurationServiceTest : IDisposable
    {
        private readonly IMapper _mapperMock;
        private readonly SQLiteDbContext _dbContextMock;
        private readonly ILoggerFactory _loggerFactoryMock;

        private GlobalConfiguration _globalConfig;
        private IMapper _mapper;
        private NullLoggerFactory _nullLoggerFactory;
        private SQLiteDbContext _dbContext;
        private GlobalConfigurationService _globalConfigurationService;

        public GlobalConfigurationServiceTest()
        {
            _mapperMock = new Mock<IMapper>().Object;
            _dbContextMock = new Mock<SQLiteDbContext>().Object;
            _loggerFactoryMock = new Mock<ILoggerFactory>().Object;

            _globalConfig = new GlobalConfiguration();
            _globalConfig.GlobalIoTCorePort = 8090;
        }

        [Fact]
        public void Ctor_Test()
        {
            Assert.NotNull(new GlobalConfigurationService(
                _mapperMock,
                _dbContextMock,
                _loggerFactoryMock));
        }

        [Fact]
        public void Ctor_Mapper_Null_Error_Test()
        {
            Assert.Throws<ArgumentNullException>("mapper", () => new GlobalConfigurationService(
                null,
                _dbContextMock,
                _loggerFactoryMock));
        }

        [Fact]
        public void Ctor_Context_Null_Error_Test()
        {
            Assert.Throws<ArgumentNullException>("context", () => new GlobalConfigurationService(
                _mapperMock,
                null,
                _loggerFactoryMock));
        }

        [Fact]
        public void Ctor_Logger_Null_Error_Test()
        {
            Assert.Throws<ArgumentNullException>("loggerFactory", () => new GlobalConfigurationService(
                _mapperMock,
                _dbContextMock,
                null));
        }

        [Fact]
        public async Task GetConfig_Test()
        {
            // Arrange
            Arrange();

            // Act
            var config = await _globalConfigurationService.GetConfig();

            // Assert
            Assert.NotNull(config);
            Assert.Equal(_globalConfig.GlobalIoTCorePort, config.GlobalIoTCorePort);
        }

        [Fact]
        public async Task UpdateConfig_Test()
        {
            // Arrange
            Arrange();
            var testConfig = new GlobalConfigurationViewModel();
            testConfig.GlobalIoTCorePort = 1234;

            // Act
            await _globalConfigurationService.UpdateConfig(testConfig);
            var config = await _globalConfigurationService.GetConfig();

            // Assert
            Assert.NotNull(config);
            Assert.Equal(testConfig.GlobalIoTCorePort, config.GlobalIoTCorePort);
        }

        public void Dispose()
        {
            _globalConfig = null;
            _mapper = null;
            _nullLoggerFactory = null;
            _dbContext = null;
        }

        private async Task<SQLiteDbContext> GetInMemoryDatabaseContext()
        {
            var options = new DbContextOptionsBuilder<SQLiteDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var databaseContext = new SQLiteDbContext(options);
            databaseContext.Database.EnsureCreated();
            if (!await databaseContext.GlobalConfiguration.AnyAsync())
            {
                databaseContext.GlobalConfiguration.Add(_globalConfig);
                await databaseContext.SaveChangesAsync();
            }

            return databaseContext;
        }

        private void Arrange()
        {
            var myProfile = new AutoMapperProfile();
            var configuration = new MapperConfiguration(cfg => cfg.AddProfile(myProfile));
            _mapper = new Mapper(configuration);
            _dbContext = GetInMemoryDatabaseContext().Result;
            _nullLoggerFactory = new NullLoggerFactory();
            _globalConfigurationService = new GlobalConfigurationService(_mapper, _dbContext, _nullLoggerFactory);
        }
    }
}
