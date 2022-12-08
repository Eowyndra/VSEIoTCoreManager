// ----------------------------------------------------------------------------
// Filename: IGlobalConfigurationService.cs
// Copyright (c) 2022 ifm diagnostic GmbH - All rights reserved.
// ----------------------------------------------------------------------------
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace VSEIoTCoreServer.WebApp.Services
{
    using VSEIoTCoreServer.WebApp.ViewModels;

    public interface IGlobalConfigurationService
    {
        /// <summary>
        /// Get the global configuration from the SQLite database.
        /// </summary>
        /// <returns>The the global configuration.</returns>
        Task<GlobalConfigurationViewModel> GetConfig();

        /// <summary>
        /// Update the global configuration in the database.
        /// </summary>
        /// <param name="config">The model containing the global configuration.</param>
        Task UpdateConfig(GlobalConfigurationViewModel config);
    }
}
