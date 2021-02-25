using Microsoft.Azure.Cosmos.Table;

namespace heartbeat_function_app.Entities
{
    public class FirmComponentEntity : TableEntity
    {
        public FirmComponentEntity()
        {
            
        }

        public FirmComponentEntity(
            string firmId,
            string componentId,
            string firmName,
            string componentName,
            string componentVersion,
            int errorCountSinceCleared,
            bool availabilityMissedSinceCleared,
            string lastCleared,
            bool updatesPaused,
            string updatesPausedReason
        )
        {
            PartitionKey = firmId;
            RowKey = componentId;
            FirmName = firmName;
            ComponentName = componentName;
            ComponentVersion = componentVersion;
            ErrorCountSinceCleared = errorCountSinceCleared;
            AvailabilityMissedSinceCleared = availabilityMissedSinceCleared;
            LastCleared = lastCleared;
            UpdatesPaused = updatesPaused;
            UpdatesPausedReason = updatesPausedReason;
        }

        public string FirmName { get; set; }
        public string ComponentName { get; set; }
        public string ComponentVersion { get; set; }
        public int ErrorCountSinceCleared { get; set; }
        public bool AvailabilityMissedSinceCleared { get; set; }
        public string LastCleared { get; set; }
        public bool UpdatesPaused { get; set; }
        public string UpdatesPausedReason { get; set; }
    }
}
