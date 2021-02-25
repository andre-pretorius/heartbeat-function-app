using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace heartbeat_function_app
{
    public class TableStore
    {
        //Todo Rework action
        public static async Task Operation_Insert(IBinder binder, ITableEntity record, ILogger log, string tableName)
        {
            const string operationDescription = "Insert operation";

            try
            {
                var operation = TableOperation.Insert(record);

                var table = await binder.BindAsync<CloudTable>(new TableAttribute(tableName) { Connection = "Table_Connection_String" });

                var result = await table.ExecuteAsync(operation);

                if (result.RequestCharge.HasValue)
                {
                    Console.WriteLine($"{operationDescription}: {result.RequestCharge}");
                }
            }
            catch (Exception e)
            {
                log.LogInformation($"{operationDescription}: {e.Message}");

                //Todo Fail action

                throw;
            }
        }

        public static async Task Operation_Update(IBinder binder, ITableEntity record, ILogger log, string tableName)
        {
            const string operationDescription = "Update operation";

            try
            {
                var operation = TableOperation.Replace(record);

                var table = await binder.BindAsync<CloudTable>(new TableAttribute(tableName) { Connection = "Table_Connection_String" });

                var result = await table.ExecuteAsync(operation);

                if (result.RequestCharge.HasValue)
                {
                    Console.WriteLine($"{operationDescription}: {result.RequestCharge}");
                }
            }
            catch (Exception e)
            {
                log.LogInformation($"{operationDescription}: {e.Message}");

                //Todo Fail action

                throw;
            }
        }

        public static async Task<T> Operation_Read_One<T>(IBinder binder, ILogger log, 
            string tableName, string partitionKey, string rowKey) 
            where T : ITableEntity
        {
            const string operationDescription = "Read operation";

            try
            {
                var operation = TableOperation.Retrieve<T>(partitionKey, rowKey);

                var table = await binder.BindAsync<CloudTable>(new TableAttribute(tableName) 
                                    { Connection = "Table_Connection_String"});

                var response = await table.ExecuteAsync(operation);

                return (T) response.Result;
            }
            catch (Exception e)
            {
                log.LogInformation($"{operationDescription}: {e.Message}");

                //Todo Fail action

                throw;
            }
        }
    }
}
