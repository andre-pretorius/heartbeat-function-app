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

    }
}
