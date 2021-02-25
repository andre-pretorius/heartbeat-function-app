using Microsoft.Azure.Cosmos.Table;

namespace heartbeat_function_app.Entities
{
    public class EventEntity : TableEntity
    {
        public EventEntity()
        {
            
        }

        public EventEntity(
            string firmId,
            string componentId,
            string time,
            string eventCode,
            string eventTitle,
            string eventDescription,
            string firmName
        )
        {
            PartitionKey = $"{firmId} {componentId}";
            RowKey = time;
            EventCode = eventCode;
            EventTitle = eventTitle;
            EventDescription = eventDescription;
            FirmId = firmId;
            FirmName = firmName;
        }

        public string EventCode { get; set; }
        public string EventTitle { get; set; }
        public string EventDescription { get; set; }
        public string FirmId { get; set; }
        public string FirmName { get; set; }

    }
}
