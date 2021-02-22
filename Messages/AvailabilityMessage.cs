
namespace heartbeat_function_app.Messages
{
    public class AvailabilityMessage
    {
        public string TimeId { get; set; }
        public int Interval { get; set; }
        public string FirmId { get; set; }
        public string ComponentId { get; set; }
        public string ComponentName { get; set; }
        public string ComponentVersion { get; set; }
    }
}
