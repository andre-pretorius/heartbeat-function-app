using MessageVersionInterfaceV1 = heartbeat_function_app.Messages.Interfaces.V1;

namespace heartbeat_function_app.Messages
{
    public class EventMessage: BaseMessage, MessageVersionInterfaceV1.IEventMessage
    {
        public string EventCode { get; set; }
        public string EventTitle { get; set; }
        public string EventDescription { get; set; }
    }
}
