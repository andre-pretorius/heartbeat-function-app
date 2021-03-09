using Microsoft.Azure.Cosmos.Table;

namespace heartbeat_function_app.Store.Entities
{
    public class ExceptionEntity: TableEntity
    {
        public ExceptionEntity()
        {
        }

        public ExceptionEntity(
            string id,
            string time,
            string title,
            string type,
            string stackTrace,
            string completeStackTrace,
            string messagePayload,
            string messageId,
            string messageType,
            string messageVersion,
            string firmId,
            string firmName,
            string componentId,
            string entity
        )
        {
            PartitionKey = id;
            RowKey = time;
            Title = title;
            Type = type;
            StackTrace = stackTrace;
            CompleteStackTrace = completeStackTrace;
            MessagePayload = messagePayload;
            MessageId = messageId;
            MessageType = messageType;
            MessageVersion = messageVersion;
            FirmId = firmId;
            FirmName = firmName;
            ComponentId = componentId;
            Entity = entity;
        }

        public string Title { get; set; }
        public string Type { get; set; }
        public string StackTrace { get; set; }
        public string CompleteStackTrace { get; set; }
        public string MessagePayload { get; set; }
        public string MessageId { get; set; }
        public string MessageType { get; set; }
        public string MessageVersion { get; set; }
        public string FirmId { get; set; }
        public string FirmName { get; set; }
        public string ComponentId { get; set; }
        public string Entity { get; set; }
    }
}
