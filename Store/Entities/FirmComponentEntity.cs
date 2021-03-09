using System.Text;
using Microsoft.Azure.Cosmos.Table;

namespace heartbeat_function_app.Store.Entities
{
    public class FirmComponentEntity: TableEntity
    {
        public FirmComponentEntity()
        :base()
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
            bool availabilityMissedForLastInterval,
            string lastCleared,
            bool updatesPaused,
            string updatesPausedReason
        )
        {
            PartitionKey = firmId;
            RowKey = componentId;
            FirmId = firmId;
            FirmName = firmName;
            ComponentId = componentId;
            ComponentName = componentName;
            ComponentVersion = componentVersion;
            ErrorCountSinceCleared = errorCountSinceCleared;
            AvailabilityMissedSinceCleared = availabilityMissedSinceCleared;
            AvailabilityMissedForLastInterval = availabilityMissedForLastInterval;
            LastCleared = lastCleared;
            UpdatesPaused = updatesPaused;
            UpdatesPausedReason = updatesPausedReason;
        }

        public string FirmId { get; set; }
        public string FirmName { get; set; }
        public string ComponentId { get; set; }
        public string ComponentName { get; set; }
        public string ComponentVersion { get; set; }
        public int ErrorCountSinceCleared { get; set; }
        public bool AvailabilityMissedSinceCleared { get; set; }
        public bool AvailabilityMissedForLastInterval { get; set; }
        public string LastCleared { get; set; }
        public bool UpdatesPaused { get; set; }
        public string UpdatesPausedReason { get; set; }

        public string EntityDescription()
        {
            var sb = new StringBuilder();
            sb.Append($"PartitionKey:{PartitionKey}");
            sb.Append($"RowKey:{RowKey}");
            sb.Append($"FirmId:{FirmId}");
            sb.Append($"FirmName:{FirmName}");
            sb.Append($"ComponentId:{ComponentId}");
            sb.Append($"ComponentName:{ComponentName}");
            sb.Append($"ComponentVersion:{ComponentVersion}");
            sb.Append($"ErrorCountSinceCleared:{ErrorCountSinceCleared}");
            sb.Append($"AvailabilityMissedSinceCleared:{AvailabilityMissedSinceCleared}");
            sb.Append($"AvailabilityMissedForLastInterval:{AvailabilityMissedForLastInterval}");
            sb.Append($"LastCleared:{LastCleared}");
            sb.Append($"UpdatesPaused:{UpdatesPaused}");
            sb.Append($"UpdatesPausedReason:{UpdatesPausedReason}");

            return sb.ToString();
        }
    }
}
