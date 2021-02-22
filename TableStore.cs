using System;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.WebJobs;

namespace heartbeat_function_app
{
    public class TableStore
    {
        public static async Task WriteAvailabilityOperation_Insert(IBinder binder, ITableEntity record)
        {
            const string operationDescription = "Insert operation";

            try
            {
                var operation = TableOperation.Insert(record);

                var table = await binder.BindAsync<CloudTable>(new TableAttribute("%Availability_Table_Name%") { Connection = "Table_Connection_String" });

                var result = await table.ExecuteAsync(operation);

                if (result.RequestCharge.HasValue)
                {
                    Console.WriteLine($"{operationDescription}: {result.RequestCharge}");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"{operationDescription}: {e.Message}");

                throw;
            }
        }

        public static async Task WriteFaultOperation_Insert(IBinder binder, ITableEntity record)
        {
            const string operationDescription = "Insert operation";

            try
            {
                var operation = TableOperation.Insert(record);

                var table = await binder.BindAsync<CloudTable>(new TableAttribute("%Fault_Table_Name%") { Connection = "Table_Connection_String" });

                var result = await table.ExecuteAsync(operation);

                if (result.RequestCharge.HasValue)
                {
                    Console.WriteLine($"{operationDescription}: {result.RequestCharge}");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"{operationDescription}: {e.Message}");

                throw;
            }
        }
    }
}
