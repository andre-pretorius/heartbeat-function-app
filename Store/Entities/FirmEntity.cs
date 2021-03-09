using System.Text;
using Microsoft.Azure.Cosmos.Table;

namespace heartbeat_function_app.Store.Entities
{
    public class FirmEntity: TableEntity
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

        public string EntityDescription()
        {
            var sb = new StringBuilder();
            sb.Append($"PartitionKey:{PartitionKey}");
            sb.Append($"RowKey:{RowKey}");
            sb.Append($"FirmName:{FirmName}");
            sb.Append($"LastCleared:{LastCleared}");
            sb.Append($"UpdatesPaused:{UpdatesPaused}");
            sb.Append($"UpdatesPausedReason:{UpdatesPausedReason}");

            return sb.ToString();
        }
    }
}
