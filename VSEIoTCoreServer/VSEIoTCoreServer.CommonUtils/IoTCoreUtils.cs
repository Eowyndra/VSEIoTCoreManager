using ifmIoTCore.Messages;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace VSEIoTCoreServer.CommonUtils
{
    public class IoTCoreUtils
    {
        public static ResponseMessage CreateResponseMessage(string jsonString)
        {
            return ifmIoTCore.Elements.ServiceData.ServiceDataBase.FromJson<ResponseMessage>(JToken.Parse(jsonString));
        }

        public static async Task<bool> WaitUntilVSEIoTCoreStopped(string iotCoreUri, int iotCorePort, int maxWaitInMilliseconds = 60_000)
        {
            var result = false;
            using (var client = new Client(iotCoreUri + ":" + iotCorePort))
            {
                while (maxWaitInMilliseconds > 0)
                {
                    try
                    {
                        var response = await client.SendRequestAndAwaitResponseAsync(IoTCoreRoutes.Device().Information().Device().GetData());
                    }
                    catch (HttpRequestException)
                    {
                        result = true;
                        break;
                    }

                    Thread.Sleep(500); // waiting 500 ms, then re-checking
                    maxWaitInMilliseconds -= 500;
                }
                return result;
            }
        }

        public static async Task<bool> WaitUntilVSEIoTCoreStarted(string iotCoreUri, int iotCorePort, int maxWaitInMilliseconds = 60_000)
        {
            var result = false;
            using (var client = new Client(iotCoreUri + ":" + iotCorePort))
            {
                while (maxWaitInMilliseconds > 0)
                {
                    try
                    {
                        var response = await client.SendRequestAndAwaitResponseAsync(IoTCoreRoutes.Device().Information().Device().GetData());
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
                }
                return result;
            }
        }

        public static async Task<bool> WaitUntilGlobalIoTCoreStopped(string iotCoreUri, int iotCorePort, int maxWaitInMilliseconds = 60_000)
        {
            var result = false;
            using (var client = new Client(iotCoreUri + ":" + iotCorePort))
            {
                while (maxWaitInMilliseconds > 0)
                {
                    try
                    {
                        var response = await client.SendRequestAndAwaitResponseAsync(IoTCoreRoutes.GetTree());
                    }
                    catch (HttpRequestException)
                    {
                        result = true;
                        break;
                    }

                    Thread.Sleep(500); // waiting 500 ms, then re-checking
                    maxWaitInMilliseconds -= 500;
                }
                return result;
            }
        }

        public static async Task<bool> WaitUntilGlobalIoTCoreStarted(string iotCoreUri, int iotCorePort, int maxWaitInMilliseconds = 60_000)
        {
            var result = false;
            using (var client = new Client(iotCoreUri + ":" + iotCorePort))
            {
                while (maxWaitInMilliseconds > 0)
                {
                    try
                    {
                        var response = await client.SendRequestAndAwaitResponseAsync(IoTCoreRoutes.GetTree());
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
                }
                return result;
            }
        }


    }
}
