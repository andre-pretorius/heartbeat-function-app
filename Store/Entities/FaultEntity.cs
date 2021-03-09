using System.Text;
using Microsoft.Azure.Cosmos.Table;

namespace heartbeat_function_app.Store.Entities
{
    public class FaultEntity: TableEntity
    {
        public FaultEntity()
        {
            
        }

        public FaultEntity(
            string firmId,
            string componentId,
            string time,
            string faultCode,
            string faultTitle,
            string faultDescription,
            string applicationInformation,
            string databaseInformation,
            string firmName
        )
        {
            PartitionKey = $"{firmId} {componentId}";
            RowKey = time;
            FaultCode = faultCode;
            FaultTitle = faultTitle;
            FaultDescription = faultDescription;
            ApplicationInformation = applicationInformation;
            DatabaseInformation = databaseInformation;
            FirmId = firmId;
            FirmName = firmName;
        }

        public string FaultCode { get; set; }
        public string FaultTitle { get; set; }
        public string FaultDescription { get; set; }
        public string ApplicationInformation { get; set; }
        public string DatabaseInformation { get; set; }
        public string FirmId { get; set; }
        public string FirmName { get; set; }

        public string EntityDescription()
        {
            var sb = new StringBuilder();
            sb.Append($"PartitionKey:{PartitionKey}");
            sb.Append($"RowKey:{RowKey}");
            sb.Append($"FaultCode:{FaultCode}");
            sb.Append($"FaultTitle:{FaultTitle}");
            sb.Append($"FaultDescription:{FaultDescription}");
            sb.Append($"ApplicationInformation:{ApplicationInformation}");
            sb.Append($"DatabaseInformation:{DatabaseInformation}");
            sb.Append($"FirmId:{FirmId}");
            sb.Append($"FirmName:{FirmName}");

            return sb.ToString();
        }
    }
}
