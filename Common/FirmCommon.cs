using System;
using System.Threading.Tasks;
using heartbeat_function_app.Entities;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace heartbeat_function_app.Common
{
    public static class FirmCommon
    {
        public static async Task<FirmEntity> FindFirm(IBinder binder, ILogger log, string firmId)
        {
            return await TableStore.Operation_Read_One<FirmEntity>(binder, log, "%Firm_Table_Name%", "ACL", firmId);
        }

        public static async Task CreateFirm(IBinder binder, ILogger log, string firmId, string firmName)
        {
            var entity = new FirmEntity(
                firmId,
                firmName,
                DateTime.UtcNow.ToString(),
                false,
                string.Empty);

            await TableStore.Operation_Insert(binder, entity, log, "%Firm_Table_Name%");
        }

        public static async Task UpdateFirm(IBinder binder, ILogger log, FirmEntity entity, string firmName)
        {
            entity.FirmName = firmName;

            await TableStore.Operation_Update(binder, entity, log, "%Firm_Table_Name%");
        }

        public static bool HasChanged(this FirmEntity entity, string firmName)
        {
            return !entity.FirmName.Equals(firmName);
        }
    }
}
