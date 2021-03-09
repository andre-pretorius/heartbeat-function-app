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

            var firmComponent = await GetFirmComponent(binder, log, firmId, componentId);

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
       
        public static async Task<FirmComponentEntity> GetFirmComponent(IBinder binder, ILogger log, string firmId, string componentId)
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

        public static async Task PerformResetFlagsForAllFirms(Binder binder, ILogger log)
        {
            var firmComponents = new List<FirmComponentEntity>();

            var modifiedFirmComponents = new List<FirmComponentEntity>();

            //Fetch all firms
            var activeFirmIdList = await FirmTableStore.FetchFirmIds(binder, log);

            if (!activeFirmIdList.Any()) return;

            //Fetch all firm components
            foreach (var activeFirmId in activeFirmIdList)
            {
                var firmComponentBatch = await FirmComponentTableStore.FetchFirmComponentsByFirm(binder, log, activeFirmId);

                firmComponents.AddRange(firmComponentBatch);
            }

            //Update all firm components
            foreach (var firmComponent in firmComponents)
            {
                var changed = false;

                var availabilityMissedForLastInterval = false;
                var availabilityMissedSinceCleared = false;
                var errorCountSinceCleared = 0;

                changed = !(firmComponent.AvailabilityMissedForLastInterval == availabilityMissedForLastInterval &&
                            firmComponent.AvailabilityMissedSinceCleared == availabilityMissedSinceCleared &&
                            firmComponent.ErrorCountSinceCleared == errorCountSinceCleared);

                if (changed)
                {
                    firmComponent.AvailabilityMissedForLastInterval = availabilityMissedForLastInterval;
                    firmComponent.AvailabilityMissedSinceCleared = availabilityMissedSinceCleared;
                    firmComponent.ErrorCountSinceCleared = errorCountSinceCleared;

                    modifiedFirmComponents.Add(firmComponent);
                }

            }

            if (!modifiedFirmComponents.Any()) return;

            //Update Components per firm
            await PerformBatchUpdateForFirmComponents(binder, log, activeFirmIdList, modifiedFirmComponents);
        }

        public static async Task PerformResetFlagsForAFirm(Binder binder, ILogger log, string firmId)
        {

            var modifiedFirmComponents = new List<FirmComponentEntity>();

            var firmComponents = await FirmComponentTableStore.FetchFirmComponentsByFirm(binder, log, firmId);

            //Update all firm components
            foreach (var firmComponent in firmComponents)
            {
                var changed = false;

                var availabilityMissedForLastInterval = false;
                var availabilityMissedSinceCleared = false;
                var errorCountSinceCleared = 0;

                changed = !(firmComponent.AvailabilityMissedForLastInterval == availabilityMissedForLastInterval &&
                            firmComponent.AvailabilityMissedSinceCleared == availabilityMissedSinceCleared &&
                            firmComponent.ErrorCountSinceCleared == errorCountSinceCleared);

                if (changed)
                {
                    firmComponent.AvailabilityMissedForLastInterval = availabilityMissedForLastInterval;
                    firmComponent.AvailabilityMissedSinceCleared = availabilityMissedSinceCleared;
                    firmComponent.ErrorCountSinceCleared = errorCountSinceCleared;

                    modifiedFirmComponents.Add(firmComponent);
                }

            }

            if (!modifiedFirmComponents.Any()) return;

            //Update Components per firm
            await PerformBatchUpdateForFirmComponents(binder, log, new List<string> { firmId }, modifiedFirmComponents);
        }

        public static async Task PerformResetFlagsForAFirmComponent(Binder binder, ILogger log, string firmId, string componentId)
        {
            var firmComponent = await FirmComponentCommon.GetFirmComponent(binder, log, firmId, componentId);

            var changed = false;

            var availabilityMissedForLastInterval = false;
            var availabilityMissedSinceCleared = false;
            var errorCountSinceCleared = 0;

            changed = !(firmComponent.AvailabilityMissedForLastInterval == availabilityMissedForLastInterval &&
                        firmComponent.AvailabilityMissedSinceCleared == availabilityMissedSinceCleared &&
                        firmComponent.ErrorCountSinceCleared == errorCountSinceCleared);

            if (changed)
            {
                firmComponent.AvailabilityMissedForLastInterval = availabilityMissedForLastInterval;
                firmComponent.AvailabilityMissedSinceCleared = availabilityMissedSinceCleared;
                firmComponent.ErrorCountSinceCleared = errorCountSinceCleared;

                await TableStore.Operation_Update(binder, log, firmComponent, "%FirmComponent_Table_Name%");
            }
        }

        /// <summary>
        /// Perform a batch update of firm components
        /// A Batch can only be performed for a specific firm at a time. Storage Partition key must be the same for a batch.
        /// </summary>
        /// <param name="binder"></param>
        /// <param name="log"></param>
        /// <param name="firmIds"></param>
        /// <param name="firmComponents"></param>
        /// <returns></returns>
        public static async Task PerformBatchUpdateForFirmComponents(Binder binder, ILogger log, List<string> firmIds, List<FirmComponentEntity> firmComponents)
        {
            //Update Components per firm

            foreach (var firmId in firmIds)
            {
                var componentsBatch = firmComponents
                    .Where(x => x.PartitionKey == firmId)
                    .ToList();

                if (componentsBatch.Any())
                {
                    await FirmComponentTableStore.BatchUpdateFirmComponents(binder, log, componentsBatch);
                }
            }
        }
    }
}
