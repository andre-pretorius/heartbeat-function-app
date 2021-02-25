namespace heartbeat_function_app.Messages
{
    public class FaultMessage
    {
        public string FirmId { get; set; }
        public string ComponentId { get; set; }
        public string FaultCode { get; set; }
        public string FaultTitle { get; set; }
        public string FaultDescription { get; set; }
        public string ApplicationInformation { get; set; }
        public string DatabaseInformation { get; set; }
    }
}
