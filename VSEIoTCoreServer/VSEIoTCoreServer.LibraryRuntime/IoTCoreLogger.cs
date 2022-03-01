using Microsoft.Extensions.Logging;

namespace VSEIoTCoreServer
{
    internal class IotCoreLogger : ifmIoTCore.Utilities.ILogger
    {
        private readonly Microsoft.Extensions.Logging.ILogger _logger;

        public IotCoreLogger(Microsoft.Extensions.Logging.ILogger logger)
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