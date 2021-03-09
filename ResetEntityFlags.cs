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
        /// <summary>
        /// Function parameters are as follows
        /// ?type=a - Perform Reset Flags For All Firms
        /// ?type=f&firm=0 - Perform Reset Flags For A Firm
        /// ?type=c&firm=0&component=0 - Perform Reset Flags For A Firm Component
        /// </summary>
        /// <returns>
        /// 200 Ok - If no problems encountered
        /// 400 Bad request - If invalid parameters provided
        /// 400 Bad request - If errors encountered
        /// </returns>
        [FunctionName("ResetEntityFlags")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = null)] HttpRequest req,
            Binder binder,
            ILogger log)
        {
            #if DEBUG
                log.LogInformation("C# HTTP trigger ResetEntityFlags.");
            #endif

            string operation = req.Query["type"];

            string firmId = req.Query["firm"];

            string firmComponentId = req.Query["component"];

            if (string.IsNullOrWhiteSpace(operation))
                return new BadRequestResult();

            if (operation.Equals("a", StringComparison.CurrentCultureIgnoreCase))
            {
                if(!string.IsNullOrWhiteSpace(firmId) || !string.IsNullOrWhiteSpace(firmComponentId))
                    return new BadRequestResult();

                await FirmComponentCommon.PerformResetFlagsForAllFirms(binder, log);

                return new OkResult();
            }
            else if (operation.Equals("f", StringComparison.CurrentCultureIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(firmId) || !string.IsNullOrWhiteSpace(firmComponentId))
                    return new BadRequestResult();

                await FirmComponentCommon.PerformResetFlagsForAFirm(binder, log, firmId);

                return new OkResult();
            }
            else if (operation.Equals("c", StringComparison.CurrentCultureIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(firmId) || string.IsNullOrWhiteSpace(firmComponentId))
                    return new BadRequestResult();

                await FirmComponentCommon.PerformResetFlagsForAFirmComponent(binder, log, firmId, firmComponentId);

                return new OkResult();
            }

            #if DEBUG
                log.LogInformation("ResetEntityFlags request type invalid");
            #endif

            //Todo Report Failure or escalate

            return new BadRequestResult();
        }
    }
}
