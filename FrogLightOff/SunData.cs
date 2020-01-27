using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace FrogLightOff
{
    public class SunData
    {
        [JsonProperty("results")]
        public SunResults Results {get;set;}
        [JsonProperty("status")]
        public string Status {get;set;}
    }

    public class SunResults {
        [JsonProperty("sunrise")]
        public string Sunrise {get;set;}
        [JsonProperty("sunset")]
        public string SunSet {get;set;}
        [JsonProperty("solar_noon")]
        public string SolarNoon {get;set;}
        [JsonProperty("day_length")]
        public string DayLength {get;set;}
        [JsonProperty("civil_twilight_begin")]
        public string CivilTwilightBegin {get; set;}
        [JsonProperty("civil_twilight_end")]
        public string CivilTwilightEnd {get;set;}
        [JsonProperty("nautical_twilight_begin")]
        public string NauticalTwilightStart {get;set;}
        [JsonProperty("nautical_twilight_end")]
        public string NauticalTwilightEnd {get;set;}
        [JsonProperty("astronomical_twilight_begin")]
        public string AstronomicalTwilightStart {get;set;}
        [JsonProperty("astronomical_twilight_end")]
        public string AstronomicalTwilightEnd {get;set;}
    }
}