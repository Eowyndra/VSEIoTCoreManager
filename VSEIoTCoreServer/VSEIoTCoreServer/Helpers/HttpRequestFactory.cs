using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;

namespace VSEIoTCoreServer.Helpers
{
    public class HttpRequestFactory
    {
        public static HttpRequestMessage CreateHttpRequestMessageFromContent(string content)
        {
            var request = new HttpRequestMessage();
            request.Method = HttpMethod.Post;
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Content = new StringContent(content);
            return request;
        }

        public static HttpRequestMessage CreateHttpRequestMessage(int cid, string addr, string data = null)
        {
            var content = CreateRequestContent(cid, addr, data);
            return CreateHttpRequestMessageFromContent(content);
        }

        // Creates the content for a request with cid, addr and optional data
        public static string CreateRequestContent(int cid, string addr, string data = null)
        {
            dynamic request;
            if (data == null)
            {
                request = new
                {
                    cid = cid,
                    code = "request",
                    adr = addr,
                };
            }
            else
            {
                request = new
                {
                    cid = cid,
                    code = "request",
                    adr = addr,
                    data = new { newvalue = data },
                };
            }

            return JsonConvert.SerializeObject(request);
        }
    }
}