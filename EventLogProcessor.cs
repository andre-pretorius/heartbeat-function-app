using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace heartbeat_function_app
{
    public static class EventLogProcessor
    {
        [FunctionName("EventLogProcessor")]
        public static void Run(
            [QueueTrigger(queueName: "%Queue_Name%", Connection = "Queue_Connection_String")] string myQueueItem, 
            ILogger log)
        {
            log.LogInformation($"C# Queue trigger function processed: {myQueueItem}");
        }
    }
}
