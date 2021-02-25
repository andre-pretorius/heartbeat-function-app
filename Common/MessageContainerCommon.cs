using System;
using heartbeat_function_app.Enums;
using heartbeat_function_app.Exceptions;
using heartbeat_function_app.Messages;

namespace heartbeat_function_app.Common
{
    public static class MessageContainerCommon
    {
        public static MessageContainer ParseMessage(this string message)
        {
            const char separator = '|';

            if (!message.StartsWith(separator))
                throw new InvalidMessageException();

            //Validation Key

            var beginKeyIndex = message.IndexOf(separator);

            var endKeyIndex = message.IndexOf(separator, startIndex: beginKeyIndex + 1);

            var validationKey = message.Substring(beginKeyIndex + 1, endKeyIndex - beginKeyIndex - 1);

            //Message Type

            var startMessageIndex = endKeyIndex;

            var endMessageIndex = message.IndexOf(separator, startIndex: startMessageIndex + 1);

            var messageType = message.Substring(startMessageIndex + 1, endMessageIndex - startMessageIndex - 1);

            //Message

            var messagePayload = message.Substring(endMessageIndex + 1);

            //Processing

            var found = Enum.TryParse(typeof(MessageType), messageType, out var messageTypeValue);

            if (!found)
                throw new InvalidMessageException();

            return new MessageContainer
            {
                MessageType = (MessageType) messageTypeValue,
                MessagePayload = messagePayload,
                ValidationKey = validationKey
            };
        }
    }
}
