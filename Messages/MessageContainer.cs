using heartbeat_function_app.Enums;

namespace heartbeat_function_app.Messages
{
    public class MessageContainer
    {
        public string ValidationKey { get; set; }

        public MessageType MessageType { get; set; }

        public string MessagePayload { get; set; }
    }
}
