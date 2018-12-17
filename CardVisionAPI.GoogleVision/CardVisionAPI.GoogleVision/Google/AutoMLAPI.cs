using CardVisionAPI.GoogleVision.Contracts.V1;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Cloud.Vision.V1;
using Grpc.Auth;
using Grpc.Core;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace CardVisionAPI.GoogleVision.Google
{
    /// <summary>
    /// Wrapper for visioin API
    /// </summary>
    public interface IAutoMLAPI
    {
        Task<Payload> Execute(MemoryStream stream);
    }


    /// <summary>
    /// Wrapper for Vison API 
    /// </summary>
    public class AutoMLAPI : IAutoMLAPI
    {
        IConfiguration _configuration;
        const string MODEL_URL = "<AutoMLAPI-URL>";


        public AutoMLAPI(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<Payload> Execute(MemoryStream stream)
        {
            var result = await CheckImageAgainstModel(stream);
            // check levels           
            if (result == null) return null;
            return result.Payload.FirstOrDefault();
        }

        private async Task<AutoMLResult> CheckImageAgainstModel(MemoryStream stream)
        {
            // authenicate
            var cred = GetServiceAccountCredential(_configuration["Auth:Google.client_email"], _configuration["Auth:Google.private_key"]);
            var token = cred.GetAccessTokenForRequestAsync().GetAwaiter().GetResult();

            try
            {
                stream.Seek(0, SeekOrigin.Begin);
                byte[] imageBytes = stream.ToArray();
                string base64String = Convert.ToBase64String(imageBytes);
                var request = new { payload = new { image = new { imageBytes = base64String } } };
                var content = JsonConvert.SerializeObject(request);
                var stringContent = new StringContent(content);

                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    var response = await client.PostAsync(MODEL_URL, stringContent);
                    var contents = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<AutoMLResult>(contents);
                }
            }
            catch (WebException ex)
            {
                return null;
            }
        }

        private ServiceAccountCredential GetServiceAccountCredential(string serviceAccountEmail, string key)
        {
            // Create credentials
            var credential = new ServiceAccountCredential(
                new ServiceAccountCredential.Initializer(serviceAccountEmail)
                {
                    Scopes = new[] {
                        "https://www.googleapis.com/auth/cloud-vision",
                        "https://www.googleapis.com/auth/cloud-platform"
                    }
                }.FromPrivateKey(key.Replace("\\n", "\n")));

            // Create the service
            var initializer = new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "CardViewer",
                GZipEnabled = true,
            };
            return credential;
        }
    }
}
