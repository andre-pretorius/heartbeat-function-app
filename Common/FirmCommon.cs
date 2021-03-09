using System;
using System.Threading.Tasks;
using heartbeat_function_app.Store;
using heartbeat_function_app.Store.Entities;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace heartbeat_function_app.Common
{
    public static class FirmCommon
    {
        public static string UnknownFirm => TableStore.UnknownFirm;

        public static async Task<FirmEntity> ProvisionFirm(IBinder binder, ILogger log, string firmId, string firmName)
        {
            var firm = await FindFirm(binder, log, firmId);

            if (firm == null)
            {
                await CreateFirm(binder, log, firmId, firmName);

                //Set firm defaults for created firm

                return new FirmEntity(firmId, firmName, string.Empty, false, string.Empty);
            }

            //Rename firm if changed

            if (firm.HasChanged(firmName))
            {
                await UpdateFirm(binder, log, firm, firmName);
            }

            firm.FirmName = firmName;

            return firm;
        }

        private static async Task<FirmEntity> FindFirm(IBinder binder, ILogger log, string firmId)
        {
            return await TableStore.Operation_Read_One<FirmEntity>(binder, log, "%Firm_Table_Name%", "ACL", firmId);
        }

        private static async Task CreateFirm(IBinder binder, ILogger log, string firmId, string firmName)
        {
            var entity = new FirmEntity(
                firmId,
                firmName,
                DateTime.UtcNow.ToString(),
                false,
                string.Empty);

            await TableStore.Operation_Insert(binder, log, entity,"%Firm_Table_Name%");
        }

        private static async Task UpdateFirm(IBinder binder, ILogger log, FirmEntity entity, string firmName)
        {
            entity.FirmName = firmName;

            await TableStore.Operation_Update(binder, log, entity, "%Firm_Table_Name%");
        }

        private static bool HasChanged(this FirmEntity entity, string firmName)
        {
            var entityFirmName = entity?.FirmName ?? string.Empty;

            return !entityFirmName.Equals(firmName);
        }
    }
}
