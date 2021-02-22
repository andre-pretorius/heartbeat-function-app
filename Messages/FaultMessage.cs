namespace heartbeat_function_app.Messages
{
    public class FaultMessage
    {
        public string TimeId { get; set; }
        public string FirmId { get; set; }
        public string ComponentId { get; set; }
        public string EventCode { get; set; }
        public string EventTitle { get; set; }
        public string EventDescription { get; set; }
    }
}
