using Microsoft.Azure.Cosmos.Table;

namespace heartbeat_function_app.Entities
{
    public class FirmEntity : TableEntity
    {
        public FirmEntity()
        {
            
        }

        public FirmEntity(
            string firmId,
            string firmName,
            string lastCleared,
            bool updatesPaused,
            string updatesPausedReason
        )
        {
            PartitionKey = "ACL";
            RowKey = firmId;
            FirmName = firmName;
            LastCleared = lastCleared;
            UpdatesPaused = updatesPaused;
            UpdatesPausedReason = updatesPausedReason;
        }

        public string FirmName { get; set; }
        public string LastCleared { get; set; }
        public bool UpdatesPaused { get; set; }
        public string UpdatesPausedReason { get; set; }
    }
}
