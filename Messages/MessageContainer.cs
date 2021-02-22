using System;
using heartbeat_function_app.Enums;

namespace heartbeat_function_app.Messages
{
    public class MessageContainer
    {
        public MessageType MessageType { get; set; }

        public string MessagePayload { get; set; }

        public MessageContainer(string message)
        {
            ParseMessageContainerString(message);
        }

        private void ParseMessageContainerString(string message)
        {
            const char separator = '|';

            if (!message.StartsWith(separator)) return;

            var beginIndex = message.IndexOf(separator);

            var endIndex = message.IndexOf(separator, startIndex: beginIndex + 1);

            var value = message.Substring(beginIndex + 1, endIndex - beginIndex - 1);

            var found = Enum.TryParse(MessageType.GetType(), value, out var messageTypeValue);

            if (found)
                MessageType = (MessageType)messageTypeValue;

            message = message.Substring(endIndex + 1);

            MessagePayload = message;
        }
    }
}
