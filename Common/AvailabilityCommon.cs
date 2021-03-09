using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using heartbeat_function_app.Store;
using heartbeat_function_app.Store.Entities;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace heartbeat_function_app.Common
{
    public static class AvailabilityCommon
    {
        public static async Task PerformAvailabilityUpdate(Binder binder, ILogger log)
        {
            var firmComponents = new List<FirmComponentEntity>();

            var availableComponentIds = new List<string>();

            var modifiedFirmComponents = new List<FirmComponentEntity>();

            //kickoff time = Determine latest availability report start time (with a 10 minute grace period for clock skews)
            var utcNow = DateTime.UtcNow;

            var kickoffTime = new DateTime(utcNow.Year, utcNow.Month, utcNow.Day, utcNow.Hour, 0, 0).AddMinutes(-10.0);

            //Select all firm Ids
            //where updates not paused AND
            //firm name does not equal "Unknown firm"
            var activeFirmIdList = await FirmTableStore.FetchFirmIds(binder, log);

            if (!activeFirmIdList.Any()) return;

            //Select all firm component Ids
            //where updates not paused AND
            //component name does not equal "Unknown firm component"

            foreach (var activeFirmId in activeFirmIdList)
            {
                var firmComponentBatch = await FirmComponentTableStore.FetchFirmComponentsByFirm(binder, log, activeFirmId);

                firmComponents.AddRange(firmComponentBatch);
            }

            //Select all availability record componentId's,AvailabilityMissedForLastInterval Where time > kickoff time

            var availabilityBatch = await AvailabilityTableStore.FetchAvailabilityIdsForLastInterval(binder, log, kickoffTime);

            availableComponentIds.AddRange(availabilityBatch);

            //for reporting components, flag AvailabilityMissedForLastInterval as false;
            //for non-report components, flag AvailabilityMissedSinceCleared and AvailabilityMissedForLastInterval as true;

            foreach (var firmComponent in firmComponents)
            {
                var changed = false;

                var availabilityMissedForLastInterval = false;

                var availabilityMissedSinceCleared = false;

                var report = availableComponentIds.FirstOrDefault(x => x == firmComponent.ComponentId);

                var reportFound = report != null;

                //Report received
                if (reportFound)
                {
                    availabilityMissedForLastInterval = false;
                    availabilityMissedSinceCleared = firmComponent.AvailabilityMissedSinceCleared;

                    changed = !firmComponent.AvailabilityMissedForLastInterval == availabilityMissedForLastInterval;
                }
                //Report not received
                else
                {
                    availabilityMissedForLastInterval = true;
                    availabilityMissedSinceCleared = true;

                    changed = !(firmComponent.AvailabilityMissedForLastInterval == availabilityMissedForLastInterval &&
                                firmComponent.AvailabilityMissedSinceCleared == availabilityMissedSinceCleared);
                }

                if (changed)
                {
                    firmComponent.AvailabilityMissedForLastInterval = availabilityMissedForLastInterval;
                    firmComponent.AvailabilityMissedSinceCleared = availabilityMissedSinceCleared;

                    modifiedFirmComponents.Add(firmComponent);
                }

            }

            if (!modifiedFirmComponents.Any()) return;

            //Update Components per firm
            await FirmComponentCommon.PerformBatchUpdateForFirmComponents(binder, log, activeFirmIdList, modifiedFirmComponents);
        }
    }
}
