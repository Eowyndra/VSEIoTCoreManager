// ----------------------------------------------------------------------------
// Filename: Client.cs
// Copyright (c) 2022 ifm diagnostic GmbH - All rights reserved.
// ----------------------------------------------------------------------------
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace VSEIoTCoreServer.CommonUtils
{
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;

    public class Client : IDisposable
    {
        private HttpClient _client = null;

        public Client(string iotEndpoint)
        {
            _client = new HttpClient()
            {
                BaseAddress = new Uri(iotEndpoint),
            };
        }

        public void Dispose()
        {
            if (_client != null)
            {
                _client.Dispose();
                _client = null;
            }
        }

        public async Task<HttpResponseMessage> SendAsync(int cid, string addr, string data = null)
        {
            using var request = HttpRequestFactory.CreateHttpRequestMessage(cid, addr, data);
            return await _client.SendAsync(request);
        }

        public async Task<string> SendRequestAndAwaitResponseAsync(string addr = "", string data = null)
        {
            using var response = await SendAsync(0, addr, data);
            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadAsStringAsync();
            return body;
        }
    }
}
