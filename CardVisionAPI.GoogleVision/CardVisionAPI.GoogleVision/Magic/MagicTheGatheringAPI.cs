using CardVisionAPI.GoogleVision.Contracts.V1;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace CardVisionAPI.GoogleVision.Magic
{
    /// <summary>
    /// Wrapper for visioin API
    /// </summary>
    public interface IMagicTheGatheringAPI
    {
        Task<MagicAPIResults> Execute(string titleText);
    }

    /// <summary>
    /// Wrapper for Magic API 
    /// </summary>
    public class MagicTheGatheringAPI : IMagicTheGatheringAPI
    {
        IConfiguration _configuration;
 
        public MagicTheGatheringAPI(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<MagicAPIResults> Execute(string titleText)
        {
            var path = $"https://api.magicthegathering.io/v1/cards?name={titleText}";
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var response = await client.GetStringAsync(path);
                    return JsonConvert.DeserializeObject<MagicAPIResults>(response);
                }
            }
            catch (WebException ex)
            {
                return null;
            }
        }
    }
}
