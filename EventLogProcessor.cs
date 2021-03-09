using System;
using System.Threading.Tasks;
using heartbeat_function_app.Common;
using heartbeat_function_app.Enums;
using heartbeat_function_app.Messages;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;


namespace heartbeat_function_app
{
    public static class EventLogProcessor
    {
        [FunctionName("EventLogProcessor")]
        public static async Task Run(
            [QueueTrigger(queueName: "%Queue_Name%", Connection = "Queue_Connection_String")] string messageString,
            Binder binder,
            ILogger log)
        {
            #if DEBUG
                log.LogInformation("EventLogProcessor: triggered by queue");
            #endif

            var message = new BaseMessage
            {
                MessageId = Guid.Empty,
                FirmId = "NA",
                FirmName = "NA",
                ComponentId = "NA"
            };

            try
            {
                var messageContainer = messageString.ParseMessage();

                message.MessageId = messageContainer.MessageId;

                message.MessageVersion = messageContainer.MessageVersion;

                var key = messageContainer.ValidationKey;

                //Verify Key, respond with firmId & firmName

                var keyComponents = key.Split("&&"); //Stub to be removed

                var firmId = keyComponents[0];//"120";

                var firmName = keyComponents[1]; //"firm name"

                //Licensing can return FirmCommon.UnknownFirm if needed, if we have a valid id

                message.SetFirm(firmId, firmName);

                //Provision - create a firm if it does not exit or update it if it is outdated

                var firm = await FirmCommon.ProvisionFirm(binder, log, message.FirmId, message.FirmName);

                if (firm.UpdatesPaused) return;

                switch (messageContainer.MessageType)
                {
                    case MessageType.Availability:

                        message = MessageCommon.DeserializeMessage<AvailabilityMessage>(messageContainer.MessagePayload, "Availability");

                        message.SetFirm(firmId, firmName);

                        await MessageCommon.ProcessAvailabilityMessage((AvailabilityMessage)message, binder, log);

                        break;

                    case MessageType.Event:

                        message = MessageCommon.DeserializeMessage<EventMessage>(messageContainer.MessagePayload, "Event");

                        message.SetFirm(firmId, firmName);

                        await MessageCommon.ProcessEventMessage((EventMessage)message, binder, log);

                        break;

                    case MessageType.Fault:

                        message = MessageCommon.DeserializeMessage<FaultMessage>(messageContainer.MessagePayload, "Fault");

                        message.SetFirm(firmId, firmName);

                        await MessageCommon.ProcessFaultMessage((FaultMessage)message, binder, log);

                        break;

                    //MessageType.Unknown will failed during parsing:
                    default:
                        throw new InvalidOperationException("Message format invalid after parse");
                }

                log.LogInformation("EventLogProcessor: complete");
            }
            catch(Exception e)
            {
                await ExceptionCommon.StoreException(binder, log, e, message);

                throw;
            }
        }
    }
} 
