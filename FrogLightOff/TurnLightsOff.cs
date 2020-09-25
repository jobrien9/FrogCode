using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace FrogLightOff
{
    public static class TurnLightsOff
    {
        private const string BASE_URL = "https://api.particle.io/v1/devices/<DEVICEID>/";
        private const string BASE_SUNRISE_URL = "https://api.sunrise-sunset.org/";
        private const string IS_LIGHT_FUNCTION = "isLightOn";
        private const string TOGGLE_FUNCTION = "toggleLight";
        private const string TURN_OFF_NIGHTLIGHT_URL = "changeBright";
        private const string ACCESS_TOKEN = "ACCESS_TOKEN";
        private const string APPLICATION_JSON = "application/json";
        private const int MAX_RETRY_COUNT = 5;
        private const string SIX_AM = "0 0 6 * * *";
        private const string FIVE_SECOND_INTERVAL = "*/5 * * * * *"; //for testing
        private const string NINE_PM = "0 0 21 * * *";
        private const string FIVE_PM = "0 0 17 * * *";

        [FunctionName("TurnLightsOff")]
        public static async void Run([TimerTrigger(FIVE_PM)]TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
            var priorState = IsLightOn().Result;
            if (priorState)
            {
                var millisRemaining = await FetchSunriseSet(false);
                log.LogInformation($"Millis until sunset: {millisRemaining}");
                var attempts = await ToggleLight(priorState, millisRemaining, log);
                //turn off
                log.LogInformation($"Turned off after {attempts} attempts");
            }
        }

        [FunctionName("TurnLightsOn")]
        public static async void RunLightsOn([TimerTrigger(SIX_AM)]TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
            var priorState = IsLightOn().Result;
            log.LogInformation($"Is Prior State: {priorState}");
            if (!priorState)
            {
                var millisRemaining = await FetchSunriseSet(true);
                log.LogInformation($"Millis until sunrise: {millisRemaining}");
                var attempts = await ToggleLight(priorState, millisRemaining, log);
                //turn on
                log.LogInformation($"Turned on after {attempts} attempts");
            }
        }

        //this turns off the blue light on the aquarium for the night
        [FunctionName("TurnOffNightLight")]
        public static async void NightLightOff([TimerTrigger(NINE_PM)]TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            using (var client = new HttpClient())
            {
                var result = await client.PostAsync($"{BASE_URL}{TURN_OFF_NIGHTLIGHT_URL}?access_token={ACCESS_TOKEN}",
                    new StringContent("{\"newBrightness\" : \"0\"}", Encoding.UTF8, APPLICATION_JSON));

                if (result.IsSuccessStatusCode)
                {
                    log.LogInformation($"Turned off for the night");
                    //todo: build in some resiliency to try again if it fails
                }
            }
        }

        private async static Task<double> FetchSunriseSet(bool isSunRise = true)
        {
            double millisRemaining = 0; // milliseconds remaining until sunrise/sunset
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(BASE_SUNRISE_URL);
                var result = await client.GetAsync("json?lat=32.552212&lng=-84.895098&formatted=0");
                if (result.IsSuccessStatusCode)
                {
                    var resultAsJson = JsonConvert.DeserializeObject<SunData>(result.Content.ReadAsStringAsync().Result);
                    //todo: pull out sunrise/set from string and calculate Eastern Time Zone times
                    var sunResults = resultAsJson.Results;
                    var asTime = isSunRise ? DateTime.Parse(sunResults.Sunrise) : DateTime.Parse(sunResults.SunSet);
                    millisRemaining = (asTime - DateTime.Now).TotalMilliseconds;
                }
            }

            return millisRemaining;
        }

        /// <summary>
        /// Turns the light from off to on or on to off
        /// </summary>
        /// <param name="priorState">Pass in true if the light was on previously. False if off</param>
        /// <param name="retries">Number of times the command has been attempted</param>
        /// <returns>Returns the number of attempts it took to do this</returns>
        private async static Task<int> ToggleLight(bool priorState, double millisRemaining, ILogger log, int retries = 0)
        {
            var finalRetryCount = retries;
            if (retries < MAX_RETRY_COUNT)
            {
                using (var client = new HttpClient())
                {
                    var millisPost = new MillisPost()
                    {
                        millisRemaining = millisRemaining.ToString()
                    };

                    //this should toggle the light from on to off or vice versa
                    var result = client.PostAsync($"{BASE_URL}{TOGGLE_FUNCTION}?access_token={ACCESS_TOKEN}",
                        new StringContent(JsonConvert.SerializeObject(millisPost), Encoding.UTF8, APPLICATION_JSON)).Result;
                    log.LogInformation($"Result of ToggleLight: {result}");
                    //if (result.IsSuccessStatusCode)
                    //{
                    //    var resultAsJson = JsonConvert.DeserializeObject<RedBearReturn>(result.Content.ReadAsStringAsync().Result);
                    //    //check to make sure that the toggle was successful
                    //    var isLightOnNow = resultAsJson.ReturnValue == 1 ? true : false;
                    //    //if the lights didn't change for some weird reason, try again
                    //    if (isLightOnNow == priorState)
                    //    {
                    //        finalRetryCount = await ToggleLight(priorState, millisRemaining, log, ++retries);
                    //    }
                    //}
                    //else
                    //{
                    //    finalRetryCount = await ToggleLight(priorState, millisRemaining, log, ++retries);
                    //}
                }
            }
            else
            {
                throw new Exception($"Light Toggle Failed after {MAX_RETRY_COUNT} attempts!");
            }

            return finalRetryCount;
        }

        private async static Task<bool> IsLightOn()
        {
            using (var client = new HttpClient())
            {
                var result = await client.PostAsync($"{BASE_URL}{IS_LIGHT_FUNCTION}?access_token={ACCESS_TOKEN}",
                    new StringContent("", Encoding.UTF8, APPLICATION_JSON));

                if (result.IsSuccessStatusCode)
                {
                    var resultAsJson = JsonConvert.DeserializeObject<RedBearReturn>(result.Content.ReadAsStringAsync().Result);
                    return resultAsJson.ReturnValue == 1 ? true : false;
                }
            }

            //if the code reaches this point, there is an error
            throw new Exception("Error checking light status!");
        }
    }
}