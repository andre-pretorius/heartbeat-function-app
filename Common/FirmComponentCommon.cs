using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using heartbeat_function_app.Enums;
using heartbeat_function_app.Store;
using heartbeat_function_app.Store.Entities;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace heartbeat_function_app.Common
{
    public static class FirmComponentCommon
    {
        public static string UnknownComponent => TableStore.UnknownComponent;

        public static string UnknownVersion => TableStore.UnknownVersion;

        /* Messages will create a firm component if it doesn't exist
         * Messages other than availability will create "unknown" components
         * Availability messages will create a component with the correct name
         * Availability messages will update component name, version and firm name if changed
         * Fault messages will adjust a component's fault count
         */
        public static async Task<bool> UpdateOrCreateComponent(IBinder binder, ILogger log, string firmId, string componentId, MessageType messageType, string componentName, string componentVersion, string firmName)
        {
            var errorCountSinceCleared = 0;

            var availabilityMissedSinceCleared = false;

            var availabilityMissedForLastInterval = false;

            var updateComponentPaused = false;

            var firmComponent = await FindFirmComponent(binder, log, firmId, componentId);

            if (firmComponent == null)
            {
                if (messageType == MessageType.Fault)
                {
                    errorCountSinceCleared = 1;
                }

                await CreateFirmComponent(binder, log, firmId, componentId, componentName, componentVersion, firmName,
                    errorCountSinceCleared, availabilityMissedSinceCleared, availabilityMissedForLastInterval);
            }
            else
            {
                if (messageType == MessageType.Fault)
                {
                    errorCountSinceCleared = firmComponent.ErrorCountSinceCleared + 1;

                    //Update component if changed
                    // - Update firm component error count since cleared
                    await UpdateFirmComponent(binder, log, firmComponent, firmComponent.ComponentName, firmComponent.ComponentVersion, firmName,
                        errorCountSinceCleared, firmComponent.AvailabilityMissedSinceCleared);
                }
                else if (messageType == MessageType.Availability)
                {
                    //Update component if changed
                    // - Rename firm on component if changed

                    if (firmComponent.HasChanged(componentName, componentVersion, firmName))
                    {
                        await UpdateFirmComponent(binder, log, firmComponent, componentName, componentVersion, firmName,
                            firmComponent.ErrorCountSinceCleared, firmComponent.AvailabilityMissedSinceCleared);
                    }
                }

                updateComponentPaused = firmComponent.UpdatesPaused;
            }

            return updateComponentPaused;
        }
       
        private static async Task<FirmComponentEntity> FindFirmComponent(IBinder binder, ILogger log, string firmId, string componentId)
        {
            return await TableStore.Operation_Read_One<FirmComponentEntity>(binder, log, "%FirmComponent_Table_Name%", firmId, componentId);
        }

        private static async Task CreateFirmComponent(IBinder binder, ILogger log, string firmId, string componentId,
            string componentName, string componentVersion, string firmName, int errorCountSinceCleared, bool availabilityMissedSinceCleared, bool availabilityMissedForLastInterval)
        {
            var entity = new FirmComponentEntity(
                firmId,
                componentId,
                firmName,
                componentName,
                componentVersion,
                errorCountSinceCleared,
                availabilityMissedSinceCleared,
                availabilityMissedForLastInterval,
                DateTime.UtcNow.ToString(),
                false,
                string.Empty);

            await TableStore.Operation_Insert(binder, log, entity, "%FirmComponent_Table_Name%");
        }

        private static async Task UpdateFirmComponent(IBinder binder, ILogger log, FirmComponentEntity entity, 
            string componentName, string componentVersion, string firmName, int errorCountSinceCleared, bool availabilityMissedSinceCleared)
        {
            entity.ComponentName = componentName;
            entity.ComponentVersion = componentVersion;
            entity.FirmName = firmName;
            entity.ErrorCountSinceCleared = errorCountSinceCleared;
            entity.AvailabilityMissedSinceCleared = availabilityMissedSinceCleared;

            await TableStore.Operation_Update(binder, log, entity, "%FirmComponent_Table_Name%");
        }

        private static bool HasChanged(this FirmComponentEntity entity, string componentName, string componentVersion, string firmName)
        {
            var entityComponentName = entity?.ComponentName ?? string.Empty;

            var entityComponentVersion = entity?.ComponentVersion ?? string.Empty;

            var entityFirmName = entity?.FirmName ?? string.Empty;

            return !(entityComponentName.Equals(componentName)
                     && entityComponentVersion.Equals(componentVersion)
                     && entityFirmName.Equals(firmName));
        }

        private static async Task<List<FirmComponentEntity>> Find(IBinder binder, ILogger log)
        {
            const string operationDescription = "Find operation";

            var tableName = "%Firm_Table_Name%";

            Expression<Func<FirmComponentEntity, bool>> expression = i => i.AvailabilityMissedSinceCleared;

            try
            {
                var table = await binder.BindAsync<CloudTable>(new TableAttribute(tableName)
                    { Connection = "Table_Connection_String" });

                return table
                    .CreateQuery<FirmComponentEntity>()
                    .AsQueryable()
                    .Where(expression)
                    .ToList();
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
