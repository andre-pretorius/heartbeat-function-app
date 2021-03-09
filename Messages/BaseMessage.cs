using System;
using heartbeat_function_app.Enums;
using MessageVersionInterfaceV1 = heartbeat_function_app.Messages.Interfaces.V1;

namespace heartbeat_function_app.Messages
{
    public class BaseMessage: MessageVersionInterfaceV1.IBaseMessage
    {
        public Guid MessageId { get; set; }

        public MessageType MessageType { get; set; }

        public MessageVersion MessageVersion { get; set; }

        public string FirmId { get; set; }

        public string FirmName { get; set; }

        public string ComponentId { get; set; }

        public void SetFirm(string firmId, string firmName)
        {
            FirmId = firmId;

            FirmName = firmName;
        }
    }
}
