using System;

namespace heartbeat_function_app.Exceptions
{
    public class InvalidMessageException : Exception
    {
        public string MessagePayload { get; set; }

        public InvalidMessageException(string message, string messagePayload)
            : base(message)
        {
            MessagePayload = messagePayload;
        }
    }
}
