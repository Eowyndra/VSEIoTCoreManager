using ifmIoTCore.Messages;
using Newtonsoft.Json.Linq;
using System.Net;

namespace VSEIoTCoreServer.WebApp.Helpers
{
    public class IoTCoreUtils
    {
        public static ResponseMessage CreateResponseMessage(string jsonString)
        {
            return ifmIoTCore.Elements.ServiceData.ServiceDataBase.FromJson<ResponseMessage>(JToken.Parse(jsonString));
        }

        public static async Task<bool> WaitUntilVSEIoTCoreStopped(string iotCoreUri, int iotCorePort, int maxWaitInMilliseconds = 5_000)
        {
            var result = false;
            using (var client = new Client(iotCoreUri + ":" + iotCorePort))
            {
                while (maxWaitInMilliseconds > 0)
                {
                    try
                    {
                        var response = await client.RequestDeviceInformationDevice();
                    }
                    catch (HttpRequestException)
                    {
                        result = true;
                        break;
                    }

                    Thread.Sleep(500); // waiting 500 ms, then re-checking
                    maxWaitInMilliseconds -= 500;
                    continue;
                }
                return result;
            }
        }

        public static async Task<bool> WaitUntilVSEIoTCoreStarted(string iotCoreUri, int iotCorePort, int maxWaitInMilliseconds = 10_000)
        {
            var result = false;
            using (var client = new Client(iotCoreUri + ":" + iotCorePort))
            {
                while (maxWaitInMilliseconds > 0)
                {
                    try
                    {
                        var response = await client.RequestDeviceInformationDevice();
                        var msg = CreateResponseMessage(response);
                        if (msg.Code == 200)
                        {
                            result = true;
                            break;
                        }
                    }
                    catch (HttpRequestException)
                    {

                    }

                    Thread.Sleep(500); // waiting 500 ms, then re-checking
                    maxWaitInMilliseconds -= 500;
                    continue;
                }
                return result;
            }
        }

        public static async Task<bool> WaitUntilGlobalIoTCoreStopped(string iotCoreUri, int iotCorePort, int maxWaitInMilliseconds = 5_000)
        {
            var result = false;
            using (var client = new Client(iotCoreUri + ":" + iotCorePort))
            {
                while (maxWaitInMilliseconds > 0)
                {
                    try
                    {
                        var response = await client.RequestTree();
                    }
                    catch (HttpRequestException)
                    {
                        result = true;
                        break;
                    }

                    Thread.Sleep(500); // waiting 500 ms, then re-checking
                    maxWaitInMilliseconds -= 500;
                    continue;
                }
                return result;
            }
        }

        public static async Task<bool> WaitUntilGlobalIoTCoreStarted(string iotCoreUri, int iotCorePort, int maxWaitInMilliseconds = 5_000)
        {
            var result = false;
            using (var client = new Client(iotCoreUri + ":" + iotCorePort))
            {
                while (maxWaitInMilliseconds > 0)
                {
                    try
                    {
                        var response = await client.RequestTree();
                        var msg = CreateResponseMessage(response);
                        if (msg.Code == 200)
                        {
                            result = true;
                            break;
                        }
                    }
                    catch (HttpRequestException)
                    {

                    }

                    Thread.Sleep(500); // waiting 500 ms, then re-checking
                    maxWaitInMilliseconds -= 500;
                    continue;
                }
                return result;
            }
        }


    }
}
