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
            [HttpTrigger(AuthorizationLevel.Anonymous, "get","put", Route = null)] HttpRequest req,
            [Table("Employed", Connection = "AzureWebJobsStorage")] CloudTable employedTable,
            ILogger log)
        {

            log.LogInformation("C# HTTP trigger function processed a request.");
            string message = "empleado";
            log.LogInformation(message);

            string filter = TableQuery.GenerateFilterConditionForBool("Consolidated", QueryComparisons.Equal, false);
            TableQuery<EmployedEntity> query = new TableQuery<EmployedEntity>().Where(filter);
            TableQuerySegment<EmployedEntity> employed = await employedTable.ExecuteQuerySegmentedAsync(query, null);
            foreach(EmployedEntity employed1 in employed)
            {
                string filter1 = TableQuery.GenerateFilterConditionForInt("IdEmployed", QueryComparisons.Equal, employed1.IdEmployed);
                TableQuery<EmployedEntity> query1 = new TableQuery<EmployedEntity>().Where(filter1);
                TableQuerySegment<EmployedEntity> employedInformation = await employedTable.ExecuteQuerySegmentedAsync(query, null);

                if (employed.Results == null)
                {
                    message = "No hay empleado";
                    log.LogInformation(message);
                }
                else
                {
                    foreach (EmployedEntity employed2 in employedInformation)
                    {
                        if (employed1.IdEmployed == employed2.IdEmployed)
                        {
                            if (employed1.Type == 0 && employed2.Type == 1)
                            {
                                TableQuery<EmployedEntity> quer = new TableQuery<EmployedEntity>().Where(
                                TableQuery.CombineFilters(
                                    TableQuery.GenerateFilterConditionForInt("IdEmployed", QueryComparisons.Equal, employed2.IdEmployed),
                                    TableOperators.And,
                                    TableQuery.GenerateFilterConditionForBool("Consolidated", QueryComparisons.Equal, false)));
                                     foreach (EmployedEntity entity in await employedTable.ExecuteQuerySegmentedAsync(quer, null))
                                      {
                                    entity.Consolidated = true;
                                    TableOperation updateOperation = TableOperation.Replace(entity);
                                    await employedTable.ExecuteAsync(updateOperation);
                                }

                                    double minutes = OperationDate(employed1.InputOutput, employed2.InputOutput);
                            }
                        }
                    }
                }
            }
            return new OkObjectResult(new Response
            {
                IsSuccess = true,
                Message = message,
                Result = employed
            });
        }

        public static double OperationDate(DateTime initial, DateTime final)
        {
            return (final - initial).TotalMinutes;
        }

        public static void UpdateConsolidated(EmployedEntity employedIn, EmployedEntity employedOut)
        {

        }
    }
}
