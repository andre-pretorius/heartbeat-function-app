using System;
using heartbeat_function_app.Enums;

namespace heartbeat_function_app.Messages
{
    public class Message
    {
        public Guid MessageId { get; set; }

        public MessageVersion MessageVersion { get; set; }

        public MessageType MessageType { get; set; }

        public string MessagePayload { get; set; }

        public string ValidationKey { get; set; }
    }
}
