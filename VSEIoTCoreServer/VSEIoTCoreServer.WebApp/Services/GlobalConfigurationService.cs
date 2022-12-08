// ----------------------------------------------------------------------------
// Filename: GlobalConfigurationService.cs
// Copyright (c) 2022 ifm diagnostic GmbH - All rights reserved.
// ----------------------------------------------------------------------------
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace VSEIoTCoreServer.WebApp.Services
{
    using System.Threading.Tasks;
    using AutoMapper;
    using Microsoft.EntityFrameworkCore;
    using VSEIoTCoreServer.DAL;
    using VSEIoTCoreServer.WebApp.ViewModels;

    public class GlobalConfigurationService : IGlobalConfigurationService
    {
        private readonly IMapper _mapper;
        private readonly SQLiteDbContext _context;
        private readonly ILogger<GlobalConfigurationService> _logger;

        public GlobalConfigurationService(
            IMapper mapper,
            SQLiteDbContext context,
            ILoggerFactory loggerFactory)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            var factory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            _logger = factory.CreateLogger<GlobalConfigurationService>();
        }

        public async Task<GlobalConfigurationViewModel> GetConfig()
        {
            GlobalConfigurationViewModel? globalConfig;

            try
            {
                _logger.LogInformation("Reading global configuration...");
                var dbConfig = await _context.GlobalConfiguration.FirstOrDefaultAsync();
                globalConfig = dbConfig != null ? _mapper.Map<GlobalConfigurationViewModel>(dbConfig) : null;
                _logger.LogInformation("Successfully read global configuration");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error reading global configuration: {ex.Message}");
                throw;
            }

            return globalConfig ?? throw new KeyNotFoundException();
        }

        public async Task UpdateConfig(GlobalConfigurationViewModel config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config), "Config is null");
            }

            try
            {
                var dbConfig = await _context.GlobalConfiguration.SingleOrDefaultAsync();
                if (dbConfig != null)
                {
                    dbConfig.GlobalIoTCorePort = config.GlobalIoTCorePort;
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Successfully updated global configuration");
                }
                else
                {
                    _logger.LogError("Global configuration not found");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating global configuration: {ex.Message}");
                throw;
            }
        }
    }
}
