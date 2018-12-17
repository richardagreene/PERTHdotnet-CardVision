using CardVisionAPI.GoogleVision.Contracts.V1;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Cloud.Vision.V1;
using Grpc.Auth;
using Grpc.Core;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardVisionAPI.GoogleVision.Google
{
    public enum API_TYPE { TEXT=0, LOCATION, LABEL }

    /// <summary>
    /// Wrapper for visioin API
    /// </summary>
    public interface IVisionAPI
    {
        Task<GoogleVisionAPIResult> Execute(Stream stream, API_TYPE apiType);
    }

    /// <summary>
    /// Wrapper for Vison API 
    /// </summary>
    public class VisionAPI : IVisionAPI
    {
        IConfiguration _configuration;

        public VisionAPI(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<GoogleVisionAPIResult> Execute(Stream stream, API_TYPE apiType)
        {
            Channel channel = new Channel(
                ImageAnnotatorClient.DefaultEndpoint.Host, ImageAnnotatorClient.DefaultEndpoint.Port, GetGoogleCredential().ToChannelCredentials());
            ImageAnnotatorClient gclient = ImageAnnotatorClient.Create(channel);
            stream.Seek(0, SeekOrigin.Begin);
            var image = await Image.FromStreamAsync(stream);

            switch(apiType)
            {
                case API_TYPE.TEXT:
                    var ocr = await gclient.DetectTextAsync(image);
                    if (ocr.Any())
                    {
                        var rec = ocr.FirstOrDefault();
                        return new GoogleVisionAPIResult() { Description = CleanDescription(rec.Description), Locale = rec.Locale };
                    }
                    break;
                case API_TYPE.LOCATION:
                    var landmarks = await gclient.DetectLandmarksAsync(image);
                    if (landmarks.Any())
                    {
                        var rec = landmarks.FirstOrDefault();
                        var lat = rec.Locations[0].LatLng.Latitude;
                        var lng = rec.Locations[0].LatLng.Longitude;
                        var maps = $"https://www.google.com.au/maps/@{lat},{lng},15z";
                        return new GoogleVisionAPIResult() { Name = CleanDescription(rec.Description), Locale = maps };
                    }
                    break;
                case API_TYPE.LABEL:
                    var labels = await gclient.DetectLabelsAsync(image);
                    if (labels.Any())
                    {
                        var text = string.Join(", ", labels.Select(x=>x.Description).ToArray());
                        return new GoogleVisionAPIResult() { Description = text, Locale = "" };
                    }
                    break;
            }

            // not found
            return new GoogleVisionAPIResult() {
                Name = "Unknown", 
                Description = "Unknown",
                Locale = "en",
                ImageUrl = "img/unknown.png"
            };
        }



        /// <summary>
        /// remove any trailing chars as they sometimes creep into the results
        /// </summary>
        /// <param name="description"></param>
        /// <returns></returns>
        private string CleanDescription(string description)
        {
            var title = description.Split("\n").First();
            var words = title.Split(" ").ToList();
            
            // one char at the end can be removed
            if (words.Last().Length == 1)
                words.Remove(words.Last());

            // last letter of last word sometimes take as a capital so we convert to proper case 
            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
            return words.Aggregate((word, remaining) => word + " " + remaining).Trim();
        }

        private ServiceAccountCredential GetServiceAccountCredential(string serviceAccountEmail, string key)
        {
            // Create credentials
            var credential = new ServiceAccountCredential(
                new ServiceAccountCredential.Initializer(serviceAccountEmail)
                {
                    Scopes = new[] { "https://vision.googleapis.com/v1/images:annotate" }
                }.FromPrivateKey(key.Replace("\\n", "\n")));

            // Create the service
            var initializer = new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "CardVisionAPI",
                GZipEnabled = true,
            };
            return credential;
        }

        private GoogleCredential GetGoogleCredential()
        {
            // get the values from config
            var key = new StringBuilder();
            key.AppendFormat($"'type' : '{_configuration["Auth:Google.type"] }', ");
            key.AppendFormat($"'project_id' : '{_configuration["Auth:Google.project_id"] }', ");
            key.AppendFormat($"'private_key_id' : '{_configuration["Auth:Google.private_key_id"] }', ");
            key.AppendFormat($"'private_key' : '{_configuration["Auth:Google.private_key"] }', ");
            key.AppendFormat($"'client_email' : '{_configuration["Auth:Google.client_email"] }', ");
            key.AppendFormat($"'client_id' : '{_configuration["Auth:Google.client_id"] }', ");
            key.AppendFormat($"'auth_uri' : '{_configuration["Auth:Google.auth_uri"] }', ");
            key.AppendFormat($"'token_uri' : '{_configuration["Auth:Google.token_uri"] }', ");
            key.AppendFormat($"'auth_provider_x509_cert_url' : '{_configuration["Auth:Google.auth_provider_x509_cert_url"] }', ");
            key.AppendFormat($"'client_x509_cert_url' : '{_configuration["Auth:Google.client_x509_cert_url"] }' ");
            return GoogleCredential.FromJson("{" + key.ToString() + "}").CreateScoped(ImageAnnotatorClient.DefaultScopes); 
        }
    }
}
