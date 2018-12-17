using System;

namespace CardVisionAPI.GoogleVision.Contracts.V1
{
    /// <summary>
    /// Results for card classification
    /// </summary>
    public class CardDetailResult
    {
        public string Name { get; set; }
        public string Language { get; set; }
        public string Description { get; set; }
        public string ImageURL { get; set; }
        public double Confidence { get; set; }
    }
}
