using System;
using System.Threading.Tasks;
using heartbeat_function_app.Entities;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace heartbeat_function_app.Common
{
    public static class FirmComponentCommon
    {
        public static async Task<FirmComponentEntity> FindFirmComponent(IBinder binder, ILogger log, string firmId, string componentId)
        {
            return await TableStore.Operation_Read_One<FirmComponentEntity>(binder, log, "%FirmComponent_Table_Name%", firmId, componentId);
        }

        public static async Task CreateFirmComponent(IBinder binder, ILogger log, string firmId, string componentId,
            string componentName, string componentVersion, string firmName)
        {
            var entity = new FirmComponentEntity(
                firmId,
                componentId,
                firmName,
                componentName,
                componentVersion,
                0,
                false,
                DateTime.UtcNow.ToString(),
                false,
                string.Empty);

            await TableStore.Operation_Insert(binder, entity, log, "%FirmComponent_Table_Name%");
        }

        public static async Task UpdateFirmComponent(IBinder binder, ILogger log, FirmComponentEntity entity, string componentName, string componentVersion, string firmName)
        {
            entity.ComponentName = componentName;
            entity.ComponentVersion = componentVersion;
            entity.FirmName = firmName;

            await TableStore.Operation_Update(binder, entity, log, "%FirmComponent_Table_Name%");
        }


        public static bool HasChanged(this FirmComponentEntity entity, string componentName, string componentVersion, string firmName)
        {
            return !(entity.ComponentName.Equals(componentName)
                   && entity.ComponentVersion.Equals(componentVersion)
                   && entity.FirmName.Equals(firmName));
        }
    }
}
