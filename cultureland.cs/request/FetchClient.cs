using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace cultureland.cs.request
{
    public class FetchClient
    {
        public RestClient Client;
        public CookieJar CookieJar;

        public FetchClient(string proxyAddress = null, string certificatePath = null)
        {
            CookieJar = new CookieJar();
            var proxy = new WebProxy(proxyAddress)
            {
                UseDefaultCredentials = true,
                BypassProxyOnLocal = false
            };
            var handler = new HttpClientHandler
            {
                AllowAutoRedirect = false,
                Proxy = proxyAddress != null ? proxy : null,
                UseCookies = false,
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                
            };
            if (certificatePath != null)
                handler.ServerCertificateCustomValidationCallback = (message, serverCert, chain, sslPolicyErrors) =>
                {
                    if (sslPolicyErrors == SslPolicyErrors.None)
                        return true;

                    string pem = File.ReadAllText(certificatePath);
                    pem = pem.Replace("-----BEGIN CERTIFICATE-----", "")
                             .Replace("-----END CERTIFICATE-----", "")
                             .Replace("\r", "")
                             .Replace("\n", "");
                    byte[] certBytes = Convert.FromBase64String(pem);
                    X509Certificate2 customCaCert = new X509Certificate2(certBytes);

                    var customChain = new X509Chain();
                    customChain.ChainPolicy.ExtraStore.Add(customCaCert);
                    customChain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllowUnknownCertificateAuthority;
                    customChain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;

                    bool isValid = customChain.Build(serverCert);
                    return isValid;
                };
            Client = new RestClient(new RestClientOptions()
            {
                BaseUrl = new Uri("https://m.cultureland.co.kr"),
                FollowRedirects = false,
                ConfigureMessageHandler = _ => handler,
                UserAgent = "cultureland.cs/3.0.1-dev (+git+https://github.com/mzggr0914/cultureland.cs.git)"
            }); 
            Client.AddDefaultHeader("accept", "*/*");
            Client.AddDefaultHeader("connection", "keep-alive");
        }

        private async Task RequestRedirectUrl(RestResponse response)
        {
            if (!(response.StatusCode == HttpStatusCode.Redirect ||
                response.StatusCode == HttpStatusCode.MovedPermanently ||
                response.StatusCode == HttpStatusCode.SeeOther)) return;
            var locationHeader = (response.Headers.First(h => h.Name == "Location")?.Value) ?? throw new Exception("Redirect location not found");
            var location = locationHeader.ToString();
            var redirectRequest = new RestRequest(location, Method.Get);

            if (response.StatusCode == HttpStatusCode.TemporaryRedirect ||
                response.StatusCode == HttpStatusCode.PermanentRedirect)
            {
                redirectRequest = new RestRequest(location, response.Request.Method);
            }
            redirectRequest.AddHeader("cookie", CookieJar.ToString());
            var redirectResponse = await Client.ExecuteAsync(redirectRequest);
            CookieJar.Add(CookieJar.Parse(redirectResponse.GetHeaderValues("Set-Cookie").ToList()));
            if (redirectResponse.GetHeaderValue("Location") != null)
                await RequestRedirectUrl(redirectResponse);
        }

        public async Task<RestResponse> GetAsync(string resource, Dictionary<string, string> queryStrings = null, Dictionary<string, string> headers = null, bool allowRedirects = true)
        {
            RestRequest getRequest = new RestRequest()
            {
                Resource = resource
            };
            getRequest.AddHeader("cookie", CookieJar.ToString());
            if (queryStrings != null)
            {
                foreach (var queryString in queryStrings)
                {
                    getRequest.AddQueryParameter(queryString.Key, queryString.Value);
                }
            }
            if (headers != null) getRequest.AddHeaders(headers.Select(x => new KeyValuePair<string, string>(x.Key.ToLower(), x.Value)).ToList());
            var response = await Client.ExecuteAsync(getRequest);
            CookieJar.Add(CookieJar.Parse(response.GetHeaderValues("Set-Cookie").ToList()));
            if (allowRedirects) await RequestRedirectUrl(response);
            return response;
        }

        public async Task<RestResponse> PostAsync(string resource, object body = null, Dictionary<string, string> headers = null, bool allowRedirects = true)
        {
            RestRequest postRequest = new RestRequest()
            {
                Method = Method.Post,
                Resource = resource
            };
            if (body != null)
            {
                var dictionary = body as Dictionary<string, string>
                    ?? body.GetType().GetProperties().ToDictionary(x => x.Name, x => (string)x.GetValue(body, null));
                var form = new FormUrlEncodedContent(dictionary);
                string Strbody = await form.ReadAsStringAsync();
                postRequest.AddHeader("content-type", "application/x-www-form-urlencoded");
                postRequest.AddStringBody(Strbody, DataFormat.None);
            }
            postRequest.AddHeader("cookie", CookieJar.ToString());
            if (headers != null) postRequest.AddHeaders(headers.Select(x => new KeyValuePair<string, string>(x.Key.ToLower(), x.Value)).ToList());


            var response = await Client.ExecuteAsync(postRequest);
            CookieJar.Add(CookieJar.Parse(response.GetHeaderValues("Set-Cookie").ToList()));
            if (allowRedirects) await RequestRedirectUrl(response);
            return response;
        }
    }
}