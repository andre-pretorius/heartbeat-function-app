using System;

namespace heartbeat_function_app.Exceptions
{
    public class DeserializeObjectExceptionException : Exception
    {
        public string MessagePayload { get; set; }

        public DeserializeObjectExceptionException(string messagePayload, string messageType, Exception exception)
            : base($"Unable to deserialize object of type '{messageType}'", innerException: exception)
        {
            MessagePayload = messagePayload;
        }
    }
}
