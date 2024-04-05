using Org.BouncyCastle.Asn1.Cmp;
using Org.BouncyCastle.Asn1.Crmf;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace LoginWWW
{
    public class RequestRestsharp
    {
        private RestClient _restClient;
        public RequestRestsharp(string userAgent = "", string proxy = "")
        {
            if (string.IsNullOrEmpty(userAgent))
                userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/74.0.3729.131 Safari/537.36";
            var options = new RestClientOptions()
            {
                MaxTimeout = -1,
                UserAgent = userAgent
            };
            if (!string.IsNullOrEmpty(proxy))
            {
                switch (proxy.Split(':').Count())
                {
                    case 2:
                        {
                            options.Proxy = new WebProxy(proxy.Split(':')[0], int.Parse(proxy.Split(':')[1]));
                            break;
                        }
                    case 4:
                        {
                            WebProxy webProxy = new WebProxy(proxy.Split(':')[0], int.Parse(proxy.Split(':')[1]));
                            webProxy.Credentials = new NetworkCredential(proxy.Split(':')[2], proxy.Split(':')[3]);
                            options.Proxy = webProxy;
                            break;
                        }
                }
            }
            _restClient = new RestClient(options);
        }
        public async Task<RestResponse> SendAsync(string url, Method method, Dictionary<string, string>? headers = null, Dictionary<string, string>? paramers = null)
        {
            var request = new RestRequest(url, method);
            request.AlwaysMultipartFormData = true;
            if (headers != null)
                foreach (var header in headers)
                    request.AddHeader(header.Key, header.Value);

            if (paramers != null)
                foreach (var param in paramers)
                    request.AddParameter(param.Key, param.Value);


            var response = await _restClient.ExecuteAsync(request);

            do
            {
                response = await _restClient.ExecuteAsync(request);
                await Task.Delay(500);
            } while (string.IsNullOrEmpty(response.Content));

            return response;
        }
        public string Send(string url, Method method)
        {
            var request = new RestRequest(url, method);

            var response = _restClient.ExecuteAsync(request);

            do
            {
                response = _restClient.ExecuteAsync(request);
                Task.Delay(500);
            } while (string.IsNullOrEmpty(response.Result.Content));

            return response.Result.Content;
        }
        public string Send(string url, Method method, Dictionary<string, string>? headers = null, Dictionary<string, string>? paramers = null)
        {
            var request = new RestRequest(url, method);
            request.AlwaysMultipartFormData = true;
            if (headers != null)
                foreach (var header in headers)
                    request.AddHeader(header.Key, header.Value);

            if (paramers != null)
                foreach (var param in paramers)
                    request.AddParameter(param.Key, param.Value);


            var response =  _restClient.ExecuteAsync(request);

            do
            {
                //response = _restClient.ExecuteAsync(request);
                Task.Delay(500);
            } while (string.IsNullOrEmpty(response.Result.Content));

            return response.Result.Content;
        }
        public string GetCookie(CookieCollection cookies)
        {
            string cookieString = "";

            foreach (Cookie cookie in cookies)
            {
                cookieString += $"{cookie.Name}={cookie.Value}; ";
            }
            return cookieString;
        }
    }
}
