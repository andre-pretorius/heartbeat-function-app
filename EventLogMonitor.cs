using System;
using System.Threading.Tasks;
using heartbeat_function_app.Common;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace heartbeat_function_app
{
    public static class EventLogMonitor
    {
        [FunctionName("EventLogMonitor")]
        //Every hour at 30 minutes past the hour
        //See https://crontab.cronhub.io/ to adjust the con expression
        public static async Task Run([TimerTrigger("30 * * * *")] TimerInfo myTimer,
            Binder binder,
            ILogger log)
        {
            #if DEBUG
                log.LogInformation($"C# Timer trigger function executed at: {DateTime.UtcNow} UTC");
            #endif

            await AvailabilityCommon.PerformAvailabilityUpdate(binder, log);

            //Todo Report Failure or escalate
        }
    }
}
