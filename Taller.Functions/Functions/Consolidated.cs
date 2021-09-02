using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.WindowsAzure.Storage.Table;
using Taller.Functions.Entities;
using Taller.Common.Response;
using Taller.Common.Models;

namespace Taller.Functions.Functions
{
    public static class Consolidated
    {
        [FunctionName("Consolidated")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            [Table("Employed", Connection = "AzureWebJobsStorage")] CloudTable employedTable,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            TableQuery<EmployedEntity> query = new TableQuery<EmployedEntity>();
            TableQuerySegment<EmployedEntity> employed = await employedTable.ExecuteQuerySegmentedAsync(query, null);
            foreach(EmployedEntity employed1 in employed)
            {

            }
        }
    }
}
