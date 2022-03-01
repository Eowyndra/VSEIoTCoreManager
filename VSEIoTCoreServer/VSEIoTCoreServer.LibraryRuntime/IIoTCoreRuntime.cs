﻿namespace VSEIoTCoreServer.LibraryRuntime
{
    public interface IIoTCoreRuntime
    {
        /// <summary>
        /// Starts a global IoTCore instance
        /// </summary>
        /// <param name="ipAddress">The IP-Address of the global IoTCore instance</param>
        /// <param name="port">The Port of the global IoTCore instance</param>
        void Start(string ipAddress, int port);
        /// <summary>
        /// Stops the global IoTCore instance
        /// </summary>
        /// <returns></returns>
        void Stop();
        /// <summary>
        /// Add a running VSEIoTCore to the global IoTCore via Mirroring
        /// </summary>
        /// <param name="vseIoTCoreIpAddress">The IP-Address of the VSEIoTCore that shall be added to the global IoTCore</param>
        /// <param name="vseIoTCorePort">The Port of the VSEIoTCore that shall be added to the global IoTCore</param>
        void AddMirror(string vseIoTCoreIpAddress, int vseIoTCorePort);
    }
}
