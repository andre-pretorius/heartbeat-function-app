using System;
using System.Threading.Tasks;
using heartbeat_function_app.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace heartbeat_function_app
{
    public static class ResetEntityFlags
    {
        [FunctionName("ResetEntityFlags")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            Binder binder,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger ResetEntityFlags.");

            string operation = req.Query["type"];

            string firmId = req.Query["firmId"];

            string firmComponentId = req.Query["firmComponentId"];

            if (string.IsNullOrWhiteSpace(operation))
                return new BadRequestResult();

            if (operation.Equals("a", StringComparison.CurrentCultureIgnoreCase))
            {
                if(!string.IsNullOrWhiteSpace(firmId) || !string.IsNullOrWhiteSpace(firmComponentId))
                    return new BadRequestResult();

                await FirmComponentCommon.PerformResetFlagsForAllFirms(binder, log);
            }
            else if (operation.Equals("f", StringComparison.CurrentCultureIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(firmId) || !string.IsNullOrWhiteSpace(firmComponentId))
                    return new BadRequestResult();

                await FirmComponentCommon.PerformResetFlagsForAFirm(binder, log, firmId);
            }
            else if (operation.Equals("c", StringComparison.CurrentCultureIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(firmId) || string.IsNullOrWhiteSpace(firmComponentId))
                    return new BadRequestResult();

                await FirmComponentCommon.PerformResetFlagsForAFirmComponent(binder, log, firmId, firmComponentId);
            }

            return new BadRequestResult();
        }
    }
}
