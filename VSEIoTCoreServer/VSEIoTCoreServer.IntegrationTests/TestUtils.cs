using ifmIoTCore.Messages;
using Newtonsoft.Json.Linq;

namespace VSEIoTCoreServer.IntegrationTests
{
    public class TestUtils
    {
        public static ResponseMessage CreateResponseMessage(string jsonString)
        {
            return ifmIoTCore.Elements.ServiceData.ServiceDataBase.FromJson<ResponseMessage>(JToken.Parse(jsonString));
        }
    }
}
