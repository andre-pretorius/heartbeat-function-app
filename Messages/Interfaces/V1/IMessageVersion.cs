using System;
using heartbeat_function_app.Enums;

namespace heartbeat_function_app.Messages.Interfaces.V1
{
    public interface IBaseMessage
    {
        public MessageVersion Version => MessageVersion.V1;
        public Guid MessageId { get; set; }
        public string FirmId { get; set; }
        public string FirmName { get; set; }
        public string ComponentId { get; set; }
    }

    public interface IAvailableMessage: IBaseMessage
    {
        public string ComponentName { get; set; }
        public string ComponentVersion { get; set; }
    }

    public interface IEventMessage : IBaseMessage
    {
        public string EventCode { get; set; }
        public string EventTitle { get; set; }
        public string EventDescription { get; set; }
    }

    public interface IFaultMessage : IBaseMessage
    {
        public string FaultCode { get; set; }
        public string FaultTitle { get; set; }
        public string FaultDescription { get; set; }
        public string ApplicationInformation { get; set; }
        public string DatabaseInformation { get; set; }
    }
}
