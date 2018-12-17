using System;
using System.Collections.Generic;
using System.Text;

namespace CardVisionAPI.GoogleVision.Contracts.V1
{
    public class AutoMLResult
    {
        public List<Payload> Payload { get; set; }
    }

    public class Classification
    {
        public double score { get; set; }
    }

    public class Payload
    {
        public Classification classification { get; set; }
        public string displayName { get; set; }
    }

}
