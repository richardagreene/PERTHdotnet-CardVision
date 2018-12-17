using CardVisionAPI.GoogleVision.Contracts.V1;
using CardVisionAPI.GoogleVision.Google;
using CardVisionAPI.GoogleVision.Magic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CardVisionAPI.GoogleVision.Controllers
{
    [Route("api/[controller]")]
    public class ClassificationController : Controller
    {
        private readonly IVisionAPI _visionAPI;
        private readonly IAutoMLAPI _autoMLAPI;
        private readonly IMagicTheGatheringAPI _magicAPI;
        private readonly IConfiguration _configuration;

        public ClassificationController(IConfiguration configuration,
                IVisionAPI visionAP,
                IAutoMLAPI autoMLAPI,
                IMagicTheGatheringAPI magicAPI
            )
        {
            _configuration = configuration;
            _visionAPI = visionAP;
            _autoMLAPI = autoMLAPI;
            _magicAPI = magicAPI;
        }

        // GET api/values
        [HttpPost]
        [ProducesResponseType(200, Type = typeof(CardDetailResult))]
        [ProducesResponseType(404)]
        public async Task<ActionResult> Classify(IFormFile file)
        {
            if (file == null) return NotFound("No files");

            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                double confidence = 0;
                string label = "";

                // check the card is valid if not return unknown
                Payload autoML = await _autoMLAPI.Execute(stream);
                //if(autoML == null || 
                //   autoML.classification.score < .98)
                //   return Ok(new CardDetailResult()
                //   {
                //       Name = autoML.displayName,
                //       Language = "en",
                //       Description = "Not recognised as Magic Card.",
                //       ImageURL = "img/unknown.png",
                //       Confidence = autoML.classification.score
                //   });
                confidence = autoML.classification.score;
                label = autoML.displayName;

                // search for text description
                var result = await _visionAPI.Execute(stream, API_TYPE.TEXT);

                // first two lines of text are the details we want
                var name = result.Description.Split('\n').FirstOrDefault();

                // if not known we exit
                if (name == "Unknown")
                {
                    return Ok(new CardDetailResult()
                    {
                        Name = name,
                        Language = "en",
                        Description = label,
                        ImageURL = "img/unknown.png",
                        Confidence = confidence
                    });
                }

                // value found so we check the external API
                var apiDetails = await _magicAPI.Execute(name);

                var cardDetails = new Card();
                if (apiDetails != null)
                    cardDetails = apiDetails.cards.FirstOrDefault(c => c.imageUrl != null);
                if (cardDetails == null)
                    return Ok(new CardDetailResult()
                    {
                        Name = name,
                        Language = "en",
                        Description = label,
                        ImageURL = "img/unknown.png",
                        Confidence = confidence
                    });

                return Ok(new CardDetailResult()
                {
                    Name = name,
                    Language = result.Locale,
                    Description = label,
                    ImageURL = cardDetails.imageUrl,
                    Confidence = confidence
                });
            }
        }


        [HttpPost("Location")]
        [ProducesResponseType(200, Type = typeof(CardDetailResult))]
        [ProducesResponseType(404)]
        public async Task<ActionResult> Location(IFormFile file)
        {
            if (file == null) return NotFound("No file");

            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                // search for text description
                var result = await _visionAPI.Execute(stream, API_TYPE.LOCATION);

                // check the card is valid if not return unknown
                Payload autoML = await _autoMLAPI.Execute(stream);

                return Ok(new CardDetailResult()
                {
                    Name = result.Name,
                    Language = result.Locale,
                    Description = autoML?.displayName,
                    ImageURL = "img/found.png",
                    Confidence = autoML?.classification?.score ?? 0
                });
            }
        }

        [HttpPost("Label")]
        [ProducesResponseType(200, Type = typeof(CardDetailResult))]
        [ProducesResponseType(404)]
        public async Task<ActionResult> Label(IFormFile file)
        {
            if (file == null) return NotFound("No file");

            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                // search for text description
                var result = await _visionAPI.Execute(stream, API_TYPE.LABEL);

                // check the card is valid if not return unknown
                Payload autoML = await _autoMLAPI.Execute(stream);

                return Ok(new CardDetailResult()
                {
                    Name = result.Description,
                    Language = result.Locale,
                    Description = autoML?.displayName,
                    ImageURL = "img/found.png",
                    Confidence = autoML?.classification?.score ?? 0
                });
            }
        }

        [HttpPost("OCR")]
        [ProducesResponseType(200, Type = typeof(CardDetailResult))]
        [ProducesResponseType(404)]
        public async Task<ActionResult> OCR(IFormFile file)
        {
            if (file == null) return NotFound("No file");

            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                // search for text description
                var result = await _visionAPI.Execute(stream, API_TYPE.TEXT);

                return Ok(new CardDetailResult()
                {
                    Name = result.Name,
                    Language = result.Locale,
                    Description = result.Description,
                    ImageURL = "img/found.png"
                });
            }
        }

    }
}