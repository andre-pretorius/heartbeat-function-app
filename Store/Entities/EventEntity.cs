using System.Text;
using Microsoft.Azure.Cosmos.Table;

namespace heartbeat_function_app.Store.Entities
{
    public class EventEntity: TableEntity
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

        public string EntityDescription()
        {
            var sb = new StringBuilder();
            sb.Append($"PartitionKey:{PartitionKey}");
            sb.Append($"RowKey:{RowKey}");
            sb.Append($"EventCode:{EventCode}");
            sb.Append($"EventTitle:{EventTitle}");
            sb.Append($"EventDescription:{EventDescription}");
            sb.Append($"FirmId:{FirmId}");
            sb.Append($"FirmName:{FirmName}");

            return sb.ToString();
        }
    }
}
