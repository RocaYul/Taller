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
using taller.Common.Models;
using taller.Common.Response;
using taller.Functions.Entities;

namespace taller.Functions.Functions
{
    public static class EmployedAPI
    {
        //Created employed
        [FunctionName(nameof(EmployedAPI))]
        public static async Task<IActionResult> CreateEmployed(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "employed")] HttpRequest req,
            [Table("Employed", Connection = "AzureWebJobsStorage")] CloudTable employedTable,
            ILogger log)
        {
            log.LogInformation("Received a new employed");

            string name = req.Query["name"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            Employed employed = JsonConvert.DeserializeObject<Employed>(requestBody); //Read body of message

            if (string.IsNullOrEmpty(employed?.IdEmployed.ToString()))
            {
                return new BadRequestObjectResult(new Response
                {
                    IsSuccess = false,
                    Message = "The request must have a id employed"
                });
            }
            //Input in table
            EmployedEntity employedEntity = new EmployedEntity
            {
                IdEmployed = employed.IdEmployed,
                InputOutput = DateTime.UtcNow, //London time
                Type = 0,
                Consolidated = false,
                ETag = "*",
                PartitionKey = "EMPLOYED"
            };

            //Save the entity
            TableOperation addOperation = TableOperation.Insert(employedEntity);
            await employedTable.ExecuteAsync(addOperation);

            string message = "New employed stored in table";
            log.LogInformation(message);

            return new OkObjectResult(new Response
            {
                IsSuccess = true,
                Message = message,
                Result = employedEntity
            });
        }
    }
}
