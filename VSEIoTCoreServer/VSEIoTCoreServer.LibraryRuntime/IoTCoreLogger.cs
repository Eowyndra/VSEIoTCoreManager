// ----------------------------------------------------------------------------
// Filename: IoTCoreLogger.cs
// Copyright (c) 2022 ifm diagnostic GmbH - All rights reserved.
// ----------------------------------------------------------------------------
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace VSEIoTCoreServer
{
    using Microsoft.Extensions.Logging;

    internal class IotCoreLogger : ifmIoTCore.Utilities.ILogger
    {
        private readonly ILogger _logger;

        public IotCoreLogger(ILogger logger)
        {
            _logger = logger;
        }

        public void Debug(string message)
        {
            _logger?.LogDebug(message);
        }

        public void Error(string message)
        {
            _logger?.LogError(message);
        }

        public void Info(string message)
        {
            _logger?.LogInformation(message);
        }

        public void Warning(string message)
        {
            _logger?.LogWarning(message);
        }
    }
}