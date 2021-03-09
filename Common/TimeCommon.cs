using System;

namespace heartbeat_function_app.Common
{
    public static class TimeCommon
    {
        public static string GetTimeKey(out int interval)
        {
            var dateTime = DateTime.UtcNow;

            interval = dateTime.Hour;

            return dateTime.Ticks.ToString();
        }
    }
}
