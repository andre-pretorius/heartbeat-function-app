using MessageVersionInterfaceV1 = heartbeat_function_app.Messages.Interfaces.V1;

namespace heartbeat_function_app.Messages
{
    public class FaultMessage: BaseMessage, MessageVersionInterfaceV1.IFaultMessage
    {
        public string FaultCode { get; set; }
        public string FaultTitle { get; set; }
        public string FaultDescription { get; set; }
        public string ApplicationInformation { get; set; }
        public string DatabaseInformation { get; set; }
    }
}
