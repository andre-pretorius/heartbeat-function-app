using System;
using System.Text.Json;
using System.Threading.Tasks;
using heartbeat_function_app.Enums;
using heartbeat_function_app.Exceptions;
using heartbeat_function_app.Messages;
using heartbeat_function_app.Store;
using heartbeat_function_app.Store.Entities;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace heartbeat_function_app.Common
{
    public static class MessageCommon
    {
        public static Message ParseMessage(this string message)
        {
            const string separator = "|||";
            const int partsCount = 7; //Includes end separator

            if (string.IsNullOrWhiteSpace(message) || !message.StartsWith(separator) || !message.EndsWith(separator))
                throw new InvalidMessageException("Message format invalid", message);

            var parts = message.Split(separator);

            var partsFound = parts.Length;

            if (partsFound < partsCount || partsFound > partsCount)
                throw new InvalidMessageException("Message format invalid", message);

            //Validation Key

            var validationKey = parts[1];

            if (string.IsNullOrWhiteSpace(validationKey))
                throw new InvalidMessageException("Message has no key", message);

            //Message Id

            var messageId = parts[2];

            if (string.IsNullOrWhiteSpace(messageId))
                throw new InvalidMessageException("Message has no id", message);

            //Message Version

            var messageVersion = parts[3];

            if (string.IsNullOrWhiteSpace(messageVersion))
                throw new InvalidMessageException("Message has no version", message);

            //Message Type

            var messageType = parts[4];

            if (string.IsNullOrWhiteSpace(messageType))
                throw new InvalidMessageException("Message has no type", message);

            //Message

            var messagePayload = parts[5];

            if (string.IsNullOrWhiteSpace(messagePayload))
                throw new InvalidMessageException("Message has no payload", message);

            //Processing Id

            var foundId = Guid.TryParse(messageId, out var messageIdValue);

            if (!foundId || messageIdValue == Guid.Empty)
                throw new InvalidMessageException("Message id invalid", message);

            //Processing Version

            var foundVersion = Enum.TryParse(typeof(MessageVersion), messageType, out var messageVersionValue);

            if (!foundVersion)
                throw new InvalidMessageException("Message version invalid", message);

            //Processing Type

            var foundType = Enum.TryParse(typeof(MessageType), messageType, out var messageTypeValue);

            if (!foundType)
                throw new InvalidMessageException("Message type invalid", message);

            //Processing General

            var messageVersionEnum = (MessageVersion) messageVersionValue;

            var messageTypeEnum = (MessageType) messageTypeValue;

            if (messageTypeEnum == MessageType.Unknown)
                throw new InvalidMessageException("Message type is unset or invalid", message);

            return new Message
            {
                MessageId = messageIdValue,
                MessageVersion = messageVersionEnum,
                MessageType = messageTypeEnum,
                MessagePayload = messagePayload,
                ValidationKey = validationKey
            };
        }

        public static async Task ProcessAvailabilityMessage(AvailabilityMessage message, IBinder binder, ILogger log)
        {
            var updateComponentPaused = await FirmComponentCommon.UpdateOrCreateComponent(binder, log, message.FirmId, message.ComponentId,
                MessageType.Availability, message.ComponentName, message.ComponentVersion, message.FirmName);

            if (updateComponentPaused) return;

            var time = TimeCommon.GetTimeKey(out var interval);

            var entity  = new AvailabilityEntity(
                message.FirmId,
                message.ComponentId,
                time,
                interval,
                message.ComponentName,
                message.ComponentVersion,
                message.FirmName);

            await TableStore.Operation_Insert(binder, log, entity, "%Availability_Table_Name%");
        }

        public static async Task ProcessEventMessage(EventMessage message, IBinder binder, ILogger log)
        {
            var updateComponentPaused = await FirmComponentCommon.UpdateOrCreateComponent(binder, log, message.FirmId, message.ComponentId,
                MessageType.Event, FirmComponentCommon.UnknownComponent, FirmComponentCommon.UnknownVersion, message.FirmName);

            if (updateComponentPaused) return;

            var time = TimeCommon.GetTimeKey(out _);

            var entity = new EventEntity(
                message.FirmId,
                message.ComponentId,
                time,
                message.EventCode,
                message.EventTitle,
                message.EventDescription,
                message.FirmName);

            await TableStore.Operation_Insert(binder, log, entity, "%Event_Table_Name%");
        }

        public static async Task ProcessFaultMessage(FaultMessage message, IBinder binder, ILogger log)
        {
            var updateComponentPaused = await FirmComponentCommon.UpdateOrCreateComponent(binder, log, message.FirmId, message.ComponentId,
                MessageType.Fault, FirmComponentCommon.UnknownComponent, FirmComponentCommon.UnknownVersion, message.FirmName);

            if (updateComponentPaused) return;

            var time = TimeCommon.GetTimeKey(out _);

            var entity = new FaultEntity(
                message.FirmId,
                message.ComponentId,
                time,
                message.FaultCode,
                message.FaultTitle,
                message.FaultDescription,
                message.ApplicationInformation,
                message.DatabaseInformation,
                message.FirmName);

            await TableStore.Operation_Insert(binder, log, entity,"%Fault_Table_Name%");
        }

        public static TOut DeserializeMessage<TOut>(string payload, string messageType)
        {
            try
            {
                return JsonSerializer.Deserialize<TOut>(payload);
            }
            catch (Exception e)
            {
                throw new DeserializeObjectExceptionException(payload, messageType, e);
            }
        }
    }
}
