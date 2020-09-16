using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace FrogLightOff
{
    public class RedBearReturn
    {
        public string Id { get; set; }

        [JsonProperty("last_app")]
        public string LastApp { get; set; }

        public bool Connected { get; set; }

        [JsonProperty("return_value")]
        public int ReturnValue { get; set; }
    }

    public class MillisPost
    {
        [JsonProperty("millisRemaining")]
        public double millisRemaining {get; set;}
    }
}