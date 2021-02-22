using System;
using System.Text.Json;
using System.Threading.Tasks;
using heartbeat_function_app.Common;
using heartbeat_function_app.Entities;
using heartbeat_function_app.Enums;
using heartbeat_function_app.Messages;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;


namespace heartbeat_function_app
{
    public static class EventLogProcessor
    {
        [FunctionName("EventLogProcessor")]
        public static async Task Run(
            [QueueTrigger(queueName: "%Queue_Name%", Connection = "Queue_Connection_String")] string messageContainerString,
            Binder binder,
            ILogger log)
        {
            log.LogInformation("EventLogProcessor triggered by queue");

            //Verify Key

            var firmName = "firm name";

            await RouteMessage(new MessageContainer(messageContainerString), binder, firmName);
        }

        private static async Task RouteMessage(MessageContainer messageContainer, IBinder binder, string firmName)
        {
            var messageType = messageContainer.MessageType;

            var time = AvailabilityCommon.GetTimeKey(out var interval);

            switch (messageType)
            {
                case MessageType.Availability:

                    var availabilityMessage = JsonSerializer.Deserialize<AvailabilityMessage>(messageContainer.MessagePayload);

                    var availabilityEntity = new AvailabilityEntity(
                        availabilityMessage.FirmId,
                        availabilityMessage.ComponentId,
                        time,
                        interval,
                        availabilityMessage.ComponentName,
                        availabilityMessage.ComponentVersion,
                        firmName);

                    await TableStore.WriteAvailabilityOperation_Insert(binder, availabilityEntity);

                    break;

                case MessageType.Fault:

                    var faultMessage = JsonSerializer.Deserialize<FaultMessage>(messageContainer.MessagePayload);

                    var fault = new FaultEntity(
                        faultMessage.FirmId,
                        faultMessage.ComponentId,
                        time,
                        faultMessage.EventCode,
                        faultMessage.EventTitle,
                        faultMessage.EventDescription);

                    await TableStore.WriteFaultOperation_Insert(binder, fault);

                    break;

                case MessageType.Unknown:
                default:
                    //Write to Poison Queue
                    break;
            }
        }

        
    }
}
