using System;
using Microsoft.Azure.Cosmos.Table;

namespace heartbeat_function_app.Exceptions
{
    public class StoreOperationException : Exception
    {
        public ITableEntity Entity { get; set; }

        public StoreOperationException(string message, Exception exception, ITableEntity entity)
            : base(message, exception)
        {
            Entity = entity;
        }
    }
}
