using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace FrogLightOff
{
    public static class TurnLightsOff
    {
        [FunctionName("TurnLightsOff")]
        public static void Run([TimerTrigger("0 0 19 * * *")]TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
        }
    }
}