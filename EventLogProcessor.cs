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

            var messageContainer = messageContainerString.ParseMessage();

            var updateFirmPaused = false;

            //Verify Key, respond with firmId & firmName

            var key = messageContainer.ValidationKey;

            var firmId = "120";

            var firmName = "firm name 4";

            //Provision

            var firm = await FirmCommon.FindFirm(binder, log, firmId);

            if (firm == null)
            {
                await FirmCommon.CreateFirm(binder, log, firmId, firmName);
            }
            else
            {
                updateFirmPaused = firm.UpdatesPaused;

                //Rename firm if changed

                if (firm.HasChanged(firmName))
                {
                    await FirmCommon.UpdateFirm(binder, log, firm, firmName);
                }
            }

            if (updateFirmPaused) return;

            //Process Message

            await RouteMessage(messageContainer, binder, log, firmName);
        }

        private static async Task RouteMessage(MessageContainer messageContainer, IBinder binder, ILogger log, string firmName)
        {
            var updateComponentPaused = false;

            var messageType = messageContainer.MessageType;

            var payload = messageContainer.MessagePayload;

            var time = AvailabilityCommon.GetTimeKey(out var interval);

            switch (messageType)
            {
                case MessageType.Availability:

                    var availabilityMessage = JsonSerializer.Deserialize<AvailabilityMessage>(payload);

                    var firmId = availabilityMessage.FirmId;

                    var componentId = availabilityMessage.ComponentId;

                    var componentName = availabilityMessage.ComponentName;

                    var componentVersion = availabilityMessage.ComponentVersion;

                    var firmComponent = await FirmComponentCommon.FindFirmComponent(binder, log, firmId, componentId);

                    if (firmComponent == null)
                    {
                        await FirmComponentCommon.CreateFirmComponent(binder, log, firmId, componentId, componentName, componentVersion, firmName);
                    }
                    else
                    {
                        updateComponentPaused = firmComponent.UpdatesPaused;

                        //Update component if changed
                        //Rename firm on component if changed

                        if (firmComponent.HasChanged(componentName, componentVersion, firmName))
                        {
                            await FirmComponentCommon.UpdateFirmComponent(binder, log, firmComponent, componentName, componentVersion, firmName);
                        }
                    }

                    if (updateComponentPaused) return;

                    var availabilityEntity = new AvailabilityEntity(
                        availabilityMessage.FirmId,
                        availabilityMessage.ComponentId,
                        time,
                        interval,
                        availabilityMessage.ComponentName,
                        availabilityMessage.ComponentVersion,
                        firmName);

                    await TableStore.Operation_Insert(binder, availabilityEntity, log, "%Availability_Table_Name%");

                    break;

                case MessageType.Event:

                    var eventMessage = JsonSerializer.Deserialize<EventMessage>(payload);

                    var eventEntity = new EventEntity(
                        eventMessage.FirmId,
                        eventMessage.ComponentId,
                        time,
                        eventMessage.EventCode,
                        eventMessage.EventTitle,
                        eventMessage.EventDescription,
                        firmName);

                    await TableStore.Operation_Insert(binder, eventEntity, log, "%Event_Table_Name%");

                    break;

                case MessageType.Fault:

                    var faultMessage = JsonSerializer.Deserialize<FaultMessage>(payload);

                    var faultEntity = new FaultEntity(
                        faultMessage.FirmId,
                        faultMessage.ComponentId,
                        time,
                        faultMessage.FaultCode,
                        faultMessage.FaultTitle,
                        faultMessage.FaultDescription,
                        faultMessage.ApplicationInformation,
                        faultMessage.DatabaseInformation,
                        firmName);

                    await TableStore.Operation_Insert(binder, faultEntity, log, "%Fault_Table_Name%");

                    break;

                case MessageType.Unknown:
                default:
                    //Todo Write to Poison Queue
                    log.LogInformation("Message Rejected");
                    break;
            }
        }

        

        
    }
} 
