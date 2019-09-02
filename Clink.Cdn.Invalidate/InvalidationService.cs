using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Clink.Cdn.Invalidate
{
    public class InvalidationService : IInvalidationService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        private const string Secret = "Clink CDN secret  here";
        private const string KeyId = "Clink CDN key Id";//numeric id

        public InvalidationService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<string> InvalidateService(CommandArgsModel model)
        {
            //TODO add your group id (from api key section) and service group
            var relPath = $"invalidations/v1.0/enter group id/enter service group/{model.NetworkId}";

            var url = GetUrl(model, relPath);

            string responseContent;
            using (var client = _httpClientFactory.CreateClient("clink"))
            {

                using (var request = new HttpRequestMessage(HttpMethod.Post, url))
                {
                    using (var content = CreateHttpContent(model.Paths))
                    {

                        var headers = GetClinkMessageHeaders(relPath);
                        request.Content = content;
                        request.Headers.Add("Authorization", $"MPA {KeyId}:{headers.Item1}");
                        request.Headers.Add("Host", "ws.level3.com");
                        request.Headers.Add("Date", headers.Item2);

                        using (var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false))
                        {
                            responseContent = await response.Content.ReadAsStringAsync();
                            if (response.IsSuccessStatusCode)
                            {
                                return "Success";
                            }
                        }

                    }
                }
            }

            return responseContent;
        }

        private static Uri GetUrl(CommandArgsModel model, string relPath)
        {
            var uriBuilder = new UriBuilder($"https://ws.level3.com/{relPath}");
            var parameters = HttpUtility.ParseQueryString(string.Empty);

            if (!string.IsNullOrEmpty(model.Email))
            {
                parameters["notification"] = model.Email;
            }

            parameters["force"] = "true";
            parameters["ignoreCase"] = "true";
            uriBuilder.Query = parameters.ToString();
            var url = uriBuilder.Uri;
            return url;
        }

        private static Tuple<string, string> GetClinkMessageHeaders(string relPath)
        {
            //RFC pattern date
            var date = DateTime.UtcNow.ToString("r");
            //the rel path includes /and contents after host, ignore query string
            var signatureString = $"{date}\n/{relPath}\napplication/json\nPOST\n";
            var secretHash = new HMACSHA1(Encoding.ASCII.GetBytes(Secret));
            var signatureHashWithSecret = secretHash.ComputeHash(Encoding.ASCII.GetBytes(signatureString));
            var base64SignatureHash = Convert.ToBase64String(signatureHashWithSecret);
            return new Tuple<string, string>(base64SignatureHash, date);
        }


        private static HttpContent CreateHttpContent(string[] paths)
        {
            var obj = new { Paths = paths };
            var ms = new MemoryStream();
            SerializeJsonIntoStream(obj, ms);
            ms.Seek(0, SeekOrigin.Begin);
            HttpContent httpContent = new StreamContent(ms);
            httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            return httpContent;
        }

        private static void SerializeJsonIntoStream(object content, Stream stream)
        {

            using (var sw = new StreamWriter(stream, new UTF8Encoding(false), 1024, true))
            using (var jtw = new JsonTextWriter(sw) { Formatting = Formatting.None })
            {
                var js = new JsonSerializer
                {
                    ContractResolver = new DefaultContractResolver() { NamingStrategy = new CamelCaseNamingStrategy() }
                };
                js.Serialize(jtw, content);
                jtw.Flush();
            }
        }
    }

    public interface IInvalidationService
    {
        Task<string> InvalidateService(CommandArgsModel commandArgsModel);

    }
}
