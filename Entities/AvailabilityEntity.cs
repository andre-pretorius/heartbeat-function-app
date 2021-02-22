using Microsoft.Azure.Cosmos.Table;

namespace heartbeat_function_app.Entities
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
            ComponentName = componentName;
            ComponentVersion = componentVersion;
            FirmId = firmId;
            FirmName = firmName;
        }

        public int Interval { get; set; }
        public string ComponentName { get; set; }
        public string ComponentVersion { get; set; }
        public string FirmId { get; set; }
        public string FirmName { get; set; }

    }
}
