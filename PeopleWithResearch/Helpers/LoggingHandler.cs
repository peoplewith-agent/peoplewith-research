// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace PeopleWithResearch
{
    public class LoggingHandler : DelegatingHandler
    {
        public LoggingHandler() : base()
        {
        }

        public LoggingHandler(HttpMessageHandler innerHandler) : base(innerHandler)
        {
        }

        private readonly string _clientPrincipal;
        private readonly string _apiRole;

        // Update constructors to accept the values
        public LoggingHandler(string clientPrincipal, string apiRole) : base()
        {
            _clientPrincipal = clientPrincipal;
            _apiRole = apiRole;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken = default)
        {

            //// Remove any existing ZUMO-API-VERSION header
            //if (request.Headers.Contains("ZUMO-API-VERSION"))
            //{
            //    request.Headers.Remove("ZUMO-API-VERSION");
            //}


            //// Add the correct ZUMO-API-VERSION header
            //request.Headers.Add("ZUMO-API-VERSION", "2.0.0");

            // 1. ADD YOUR CUSTOM HEADERS HERE
            if (!string.IsNullOrEmpty(_clientPrincipal))
            {
                request.Headers.TryAddWithoutValidation("X-MS-CLIENT-PRINCIPAL", _clientPrincipal);
            }

            if (!string.IsNullOrEmpty(_apiRole))
            {
                request.Headers.TryAddWithoutValidation("X-MS-API-ROLE", _apiRole);
            }


            Debug.WriteLine($"[HTTP] >>> {request.Method} {request.RequestUri}");
            PrintHeaders(">>>", request.Headers);
            await PrintContentAsync(">>>", request.Content);

            HttpResponseMessage response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

            Debug.WriteLine($"[HTTP] <<< {response.StatusCode} {response.ReasonPhrase}");
            PrintHeaders("<<<", response.Headers);
            await PrintContentAsync("<<<", response.Content);

            return response;
        }

        private void PrintHeaders(string prefix, HttpHeaders headers)
        {
            foreach (var header in headers)
            {
                foreach (var hdrVal in header.Value)
                {
                    Debug.WriteLine($"[HTTP] {prefix} {header.Key}: {hdrVal}");
                }
            }
        }

        private async Task PrintContentAsync(string prefix, HttpContent content)
        {
            if (content != null)
            {
                PrintHeaders(prefix, content.Headers);
                Debug.WriteLine($"[HTTP] {prefix} {await content.ReadAsStringAsync().ConfigureAwait(false)}");
            }
        }
    }
}
