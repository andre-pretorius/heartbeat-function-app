using System.Text;
using Microsoft.Azure.Cosmos.Table;

namespace heartbeat_function_app.Store.Entities
{
    public class AvailabilityEntity: TableEntity
    {
        public AvailabilityEntity()
        {
            
        }

        public AvailabilityEntity(
            string firmId,
            string componentId,
            string time,
            int interval,
            string componentName,
            string componentVersion,
            string firmName
        )
        {
            PartitionKey = $"{firmId} {componentId}";
            RowKey = time;
            Interval = interval;
            ComponentId = componentId;
            ComponentName = componentName;
            ComponentVersion = componentVersion;
            FirmId = firmId;
            FirmName = firmName;
        }

        public int Interval { get; set; }
        public string ComponentId { get; set; }
        public string ComponentName { get; set; }
        public string ComponentVersion { get; set; }
        public string FirmId { get; set; }
        public string FirmName { get; set; }

        public string EntityDescription()
        {
            var sb = new StringBuilder();
            sb.Append($"PartitionKey:{PartitionKey}");
            sb.Append($"RowKey:{RowKey}");
            sb.Append($"Interval:{Interval}");
            sb.Append($"ComponentName:{ComponentName}");
            sb.Append($"ComponentVersion:{ComponentVersion}");
            sb.Append($"FirmId:{FirmId}");
            sb.Append($"FirmName:{FirmName}");

            return sb.ToString();
        }
    }
}
