using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using heartbeat_function_app.Exceptions;
using heartbeat_function_app.Store.Entities;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace heartbeat_function_app.Store
{
    public class TableStore
    {
        public const string UnknownFirm = "Unknown firm";

        public const string UnknownComponent = "Unknown component";

        public const string UnknownVersion = "Unknown version";

        public static async Task Operation_Insert(IBinder binder, ILogger log, ITableEntity entity, string tableName)
        {
            var operationDescription = $"Insert operation on {tableName}";

            try
            {
                var operation = TableOperation.Insert(entity);

                var table = await binder.BindAsync<CloudTable>(new TableAttribute(tableName) { Connection = "Table_Connection_String" });

                await table.ExecuteAsync(operation);
            }
            catch (Exception e)
            {
                log.LogInformation($"EventLogProcessor: Exception performing {operationDescription}");

                throw new StoreOperationException(operationDescription, e, entity);
            }
        }

        public static async Task Operation_Update(IBinder binder, ILogger log, ITableEntity entity, string tableName)
        {
            var operationDescription = $"Update operation on {tableName}";

            try
            {
                var operation = TableOperation.Replace(entity);

                var table = await binder.BindAsync<CloudTable>(new TableAttribute(tableName) { Connection = "Table_Connection_String" });

                await table.ExecuteAsync(operation);
            }
            catch (Exception e)
            {
                log.LogInformation($"EventLogProcessor: Exception performing {operationDescription}");

                throw new StoreOperationException(operationDescription, e, entity);
            }
        }

        public static async Task<T> Operation_Read_One<T>(IBinder binder, ILogger log, 
            string tableName, string partitionKey, string rowKey) 
            where T : ITableEntity
        {
            var operationDescription = $"Read one operation on {tableName}";

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
                log.LogInformation($"EventLogProcessor: Exception performing {operationDescription}");

                throw new StoreOperationException(operationDescription, e ,null);
            }
        }
    }

    public class FirmTableStore
    {
        public static async Task<List<string>> FetchFirmIds(IBinder binder, ILogger log)
        {
            var tableName = "%Firm_Table_Name%";

            var operationDescription = $"Fetch operation on {tableName}";

            try
            {
                var table = await binder.BindAsync<CloudTable>(new TableAttribute(tableName)
                { Connection = "Table_Connection_String" });

                var filterPartA =
                    TableQuery.GenerateFilterCondition(nameof(FirmEntity.PartitionKey), QueryComparisons.Equal, "ACL");

                var filterPartB =
                    TableQuery.GenerateFilterCondition(nameof(FirmEntity.FirmName), QueryComparisons.NotEqual, TableStore.UnknownFirm);

                var filterPartC =
                    TableQuery.GenerateFilterConditionForBool(nameof(FirmEntity.UpdatesPaused), QueryComparisons.NotEqual, true);

                var prefilter1 = TableQuery.CombineFilters(filterPartA, TableOperators.And, filterPartB);

                var filter = TableQuery.CombineFilters(prefilter1, TableOperators.And, filterPartC);

                var query = new TableQuery<FirmEntity>()
                    .Select(new List<string> { nameof(FirmEntity.RowKey) })
                    .Where(filter);

                var items = table.ExecuteQuery(query);

                return items.Select(x => x.RowKey).ToList();
            }
            catch (Exception e)
            {
                log.LogInformation($"EventLogProcessor: Exception performing {operationDescription}");

                throw new StoreOperationException(operationDescription, e, null);
            }
        }
    }

    public class FirmComponentTableStore
    {
        /// <summary>
        /// Fetch a list of firm components (all columns) that are not paused and not unknown by firm Id
        /// </summary>
        /// <param name="binder"></param>
        /// <param name="log"></param>
        /// <param name="firmId"></param>
        /// <returns></returns>
        public static async Task<List<FirmComponentEntity>> FetchFirmComponentsByFirm(IBinder binder, ILogger log, string firmId)
        {
            var tableName = "%FirmComponent_Table_Name%";

            var operationDescription = "FetchFirmComponentsByFirm";

            try
            {
                var table = await binder.BindAsync<CloudTable>(new TableAttribute(tableName)
                { Connection = "Table_Connection_String" });

                var filterPartA =
                    TableQuery.GenerateFilterCondition(nameof(FirmComponentEntity.PartitionKey), QueryComparisons.Equal, firmId);

                var filterPartB =
                    TableQuery.GenerateFilterConditionForBool(nameof(FirmComponentEntity.UpdatesPaused), QueryComparisons.NotEqual, true);

                var filterPartC =
                    TableQuery.GenerateFilterCondition(nameof(FirmComponentEntity.ComponentName), QueryComparisons.NotEqual, TableStore.UnknownComponent);

                var filterPre = TableQuery.CombineFilters(filterPartA, TableOperators.And, filterPartB);

                var filter = TableQuery.CombineFilters(filterPre, TableOperators.And, filterPartC);

                var query = new TableQuery<FirmComponentEntity>()
                    .Where(filter);

                return table.ExecuteQuery(query)
                    .ToList();
            }
            catch (Exception e)
            {
                log.LogInformation($"EventLogProcessor: Exception performing {operationDescription}");

                throw new StoreOperationException(operationDescription, e, null);
            }
        }

        public static async Task
            BatchUpdateFirmComponents(IBinder binder, ILogger log, List<FirmComponentEntity> firmComponents)
        {
            var tableName = "%FirmComponent_Table_Name%";

            var operationDescription = $"Batched insert or update operation on {tableName}";

            try
            {
                var table = await binder.BindAsync<CloudTable>(new TableAttribute(tableName)
                    { Connection = "Table_Connection_String" });

                var batchOperation = new TableBatchOperation();

                foreach (var firmComponent in firmComponents)
                {
                    batchOperation.InsertOrReplace(firmComponent);
                }

                table.ExecuteBatch(batchOperation);
            }
            catch (Exception e)
            {
                log.LogInformation($"EventLogProcessor: Exception performing {operationDescription}");

                throw new StoreOperationException(operationDescription, e, null);
            }
        }
    }

    public class AvailabilityTableStore
    {
        /// <summary>
        /// Fetch a list of Availability component Ids from the time provided
        /// </summary>
        /// <param name="binder"></param>
        /// <param name="log"></param>
        /// <param name="fromTime"></param>
        /// <returns></returns>
        public static async Task<List<string>> FetchAvailabilityIdsForLastInterval(IBinder binder, ILogger log, DateTime fromDate)
        {
            var tableName = "%Availability_Table_Name%";

            var operationDescription = "FetchAvailabilityIdsForLastInterval";

            try
            {
                var table = await binder.BindAsync<CloudTable>(new TableAttribute(tableName)
                { Connection = "Table_Connection_String" });

                var rowKeyMin = GetTimeKey(fromDate);

                var filter = TableQuery.GenerateFilterCondition(nameof(AvailabilityEntity.RowKey), QueryComparisons.GreaterThanOrEqual, rowKeyMin);

                var query = new TableQuery<AvailabilityEntity>()
                    .Select(new List<string> { nameof(AvailabilityEntity.ComponentId) })
                    .Where(filter);

                return table.ExecuteQuery(query)
                    .Select(x => x.ComponentId)
                    .ToList();
            }
            catch (Exception e)
            {
                log.LogInformation($"EventLogProcessor: Exception performing {operationDescription}");

                throw new StoreOperationException(operationDescription, e, null);
            }
        }

        public static string GetTimeKey(DateTime date)
        {
            var dateTime = date.ToUniversalTime();

            return dateTime.Ticks.ToString();
        }
    }
}
