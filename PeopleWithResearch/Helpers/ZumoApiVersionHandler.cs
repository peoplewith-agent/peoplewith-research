using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeopleWithResearch
{
    public class ZumoApiVersionHandler : DelegatingHandler
    {
        private readonly string _clientPrincipal;
        private readonly string _apiRole;

        public ZumoApiVersionHandler(string name, string value)
        {
            _clientPrincipal = name;
            _apiRole = value;
        }
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {

            //new peoplewith connection code

            // Add the custom header to every request
            request.Headers.TryAddWithoutValidation("X-MS-CLIENT-PRINCIPAL", _clientPrincipal);
            request.Headers.TryAddWithoutValidation("X-MS-API-ROLE", _apiRole);



            //old peoplewith research connection code

            //// Remove any existing ZUMO-API-VERSION header
            //if (request.Headers.Contains("ZUMO-API-VERSION"))
            //{
            //    request.Headers.Remove("ZUMO-API-VERSION");
            //}


            //// Add the correct ZUMO-API-VERSION header
            //request.Headers.Add("ZUMO-API-VERSION", "2.0.0");

            //// Check if the request already contains the ZUMO-API-KEY header
            //if (!request.Headers.Contains("X-ZUMO-APPLICATION"))
            //{
            //    // Add the ZUMO-API-KEY header with your API key value
            //    request.Headers.Add("zumo-api-key", "iuwdbiuwdhhe2uh2eiuh2hd29h2e98h2u9h98hd98hhh29eh298h829h89h");
            //}

            return base.SendAsync(request, cancellationToken);
        }
    }
}
