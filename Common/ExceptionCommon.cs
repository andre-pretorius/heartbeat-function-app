using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using heartbeat_function_app.Exceptions;
using heartbeat_function_app.Messages;
using heartbeat_function_app.Store;
using heartbeat_function_app.Store.Entities;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace heartbeat_function_app.Common
{
    public static class ExceptionCommon
    {
        public static async Task StoreException(IBinder binder, ILogger log, Exception e, BaseMessage message)
        {
            var title = e.Message;
            var type = e.GetType().ToString();
            var stackTrace = e.StackTrace ?? string.Empty;
            var completeStackTrace = BuildExceptionString(e);
            var time = TimeCommon.GetTimeKey(out _);

            //Message Content
            var messagePayload = string.Empty;

            var messageId = message.MessageId.ToString();
            var messageType = message.MessageType.ToString(); 
            var messageVersion = message.MessageVersion.ToString(); 
            
            //Dal Entities
            var entity = string.Empty;

            if (e is DeserializeObjectExceptionException doe)
            {
                messagePayload = doe.MessagePayload;
            }else if (e is InvalidMessageException ime)
            {
                messagePayload = ime.MessagePayload;
            }else if (e is StoreOperationException soe && soe.Entity != null)
            {
                if (soe.Entity is AvailabilityEntity ae)
                {
                    entity = ae.EntityDescription();
                }
                else if (soe.Entity is EventEntity ee)
                {
                    entity = ee.EntityDescription();
                }else if (soe.Entity is FaultEntity fe)
                {
                    entity = fe.EntityDescription();
                }
            }

            var record = new ExceptionEntity(
                $"{message.FirmId} {message.ComponentId} {messageId}".TrimEnd(),
                time,
                title,
                type,
                stackTrace,
                completeStackTrace,
                messagePayload,
                messageId,
                messageType,
                messageVersion,
                message.FirmId,
                message.FirmName,
                message.ComponentId,
                entity);

            await TableStore.Operation_Insert(binder, log, record, "%Log_Processor_Exception_Table_Name%");
        }

        private static string BuildExceptionString(Exception exception)
        {
            return BuildExceptionString(exception, true, true, true);
        }

        private static void BuildExceptionString(StringBuilder sb, string exceptionContext, Exception exception, bool includeExceptionType, bool includeExceptionMessage)
        {
            if (includeExceptionType)
                sb.AppendLine($"{exception.GetType()} ({exceptionContext})");
            if (includeExceptionMessage)
                sb.AppendLine(exception.Message);
            sb.AppendLine(exception.StackTrace);
        }

        private static string BuildExceptionString(Exception exception, bool includeExceptionType, bool includeExceptionMessage, bool includeInnerExceptions)
        {
            var sb = new StringBuilder();

            var exceptions = new List<Exception> { exception };

            if (includeInnerExceptions)
            {
                exception = exception.InnerException;
                while (exception != null)
                {
                    exceptions.Add(exception);
                    exception = exception.InnerException;
                }

                for (var i = exceptions.Count - 1; i > 0; i--)
                {
                    BuildExceptionString(sb, "inner exception", exceptions[i], includeExceptionType, includeExceptionMessage);
                    sb.AppendLine();
                }
            }

            BuildExceptionString(sb, "exception", exceptions[0], includeExceptionType, includeExceptionMessage);

            return sb.ToString();
        }
    }
}
