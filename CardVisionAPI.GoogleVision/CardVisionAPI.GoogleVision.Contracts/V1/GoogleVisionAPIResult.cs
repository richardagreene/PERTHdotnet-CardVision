using System;
using System.Collections.Generic;
using System.Text;

namespace CardVisionAPI.GoogleVision.Contracts.V1
{
    public class GoogleVisionAPIResult
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Locale { get; set; }
        public string ImageUrl { get; set; }
    }
}
