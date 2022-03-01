using ifmIoTCore;
using ifmIoTCore.Converter.Json;
using ifmIoTCore.NetAdapter.Http;
using ifmIoTCore.Profiles.Device;
using ifmIoTCore.Profiles.Device.ServiceData.Requests;
using ifmIoTCore.Utilities;

namespace VSEIoTCoreServer.LibraryRuntime
{
    public class IoTCoreRuntime : IIoTCoreRuntime
    {
        private static IIoTCore _iotCore;
        private static HttpServerNetAdapter _httpServer;
        private static DeviceManagementProfileBuilder _deviceManagementProfileBuilder;
        private static bool _globalIoTCoreStarted = false;
        private static Uri _globalIoTCoreServerUri;

        public IoTCoreRuntime()
        {
        }

        public void AddMirror(string vseIoTCoreIpAddress, int vseIoTCorePort)
        {
            if (!_globalIoTCoreStarted)
            {
                throw new InvalidOperationException("Global IoTCore not started");
            }

            var vseIoTCoreURI = vseIoTCoreIpAddress + ":" + vseIoTCorePort;
            _deviceManagementProfileBuilder.Mirror(new MirrorRequestServiceData(vseIoTCoreURI, _globalIoTCoreServerUri.ToString()));
        }

        public void Start(string ipAddress, int port)
        {
            _globalIoTCoreServerUri = new Uri($"{ipAddress}:{port}");
            _iotCore = IoTCoreFactory.Create("global_IoTCore", new NullLogger());
            _httpServer = new HttpServerNetAdapter(_iotCore, _globalIoTCoreServerUri, new JsonConverter());
            _iotCore.RegisterServerNetAdapter(_httpServer);
            _httpServer.Start();
            _iotCore.RegisterClientNetAdapterFactory(new HttpClientNetAdapterFactory(new JsonConverter()));
            _deviceManagementProfileBuilder = new DeviceManagementProfileBuilder(_iotCore);
            _deviceManagementProfileBuilder.Build();
            _globalIoTCoreStarted = true;
        }

        public void Stop()
        {
            _httpServer.Stop();
            _httpServer.Dispose();
            _iotCore.Dispose();
            _globalIoTCoreStarted = false;
        }
    }
}