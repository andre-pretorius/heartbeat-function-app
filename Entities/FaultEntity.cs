using Microsoft.Azure.Cosmos.Table;

namespace heartbeat_function_app.Entities
{
    public class FaultEntity : TableEntity
    {
        public FaultEntity()
        {
            
        }

        public FaultEntity(
            string firmId,
            string componentId,
            string time,
            string eventCode,
            string eventTitle,
            string eventDescription
        )
        {
            PartitionKey = $"{firmId} {componentId}";
            RowKey = time;
            EventCode = eventCode;
            EventTitle = eventTitle;
            EventDescription = eventDescription;
            FirmId = firmId;
        }

        public string EventCode { get; set; }
        public string EventTitle { get; set; }
        public string EventDescription { get; set; }
        public string FirmId { get; set; }

    }
}
