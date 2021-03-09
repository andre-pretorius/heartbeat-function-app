using MessageVersionInterfaceV1 = heartbeat_function_app.Messages.Interfaces.V1;

namespace heartbeat_function_app.Messages
{
    public class AvailabilityMessage: BaseMessage, MessageVersionInterfaceV1.IAvailableMessage
    {
        public string ComponentName { get; set; }
        public string ComponentVersion { get; set; }
    }
}
