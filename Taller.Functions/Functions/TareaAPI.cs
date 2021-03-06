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
using Taller.Common.Models;
using Taller.Common.Response;
using Taller.Functions.Entities;

namespace Taller.Functions.Functions
{
    public static class TareaAPI
    {
        //Created of employed
        [FunctionName(nameof(TareaAPI))]
        public static async Task<IActionResult> CreateEmployed(
             [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "employedIn")] HttpRequest req,
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
                PartitionKey = "EMPLOYED",
                RowKey = Guid.NewGuid().ToString()
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

        //Update a Task, this is with put because is update a register  
        [FunctionName(nameof(TareaUpdate))]
        public static async Task<IActionResult> TareaUpdate(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "employedIn/{id}")] HttpRequest req,
            [Table("Employed", Connection = "AzureWebJobsStorage")] CloudTable employedTable,
            string id,
            ILogger log)
        {
            log.LogInformation($"Update for empleado:{id}, received");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            Employed employed = JsonConvert.DeserializeObject<Employed>(requestBody); //Read body of message

            //Validate employed id
            TableOperation findOperation = TableOperation.Retrieve<EmployedEntity>("EMPLOYED", id);
            TableResult findResult = await employedTable.ExecuteAsync(findOperation);
            if (findResult.Result == null)
            {
                return new BadRequestObjectResult(new Response
                {
                    IsSuccess = false,
                    Message = "Employed not found."
                });
            }

            //Update  tarea
            EmployedEntity employedEntity = (EmployedEntity)findResult.Result;
            employedEntity.IdEmployed = employed.IdEmployed;
            if (!string.IsNullOrEmpty(employed?.IdEmployed.ToString()))
            {
                employedEntity.IdEmployed = employed.IdEmployed;
                employedEntity.Consolidated = employed.Consolidated;
            }

            //Guardar la entidad
            TableOperation addOperation = TableOperation.Replace(employedEntity);
            await employedTable.ExecuteAsync(addOperation);

            string message = $"Employed: {id}, update in table ";
            log.LogInformation(message);

            return new OkObjectResult(new Response
            {
                IsSuccess = true,
                Message = message,
                Result = employedEntity
            });
        }

        //Recuperate task- get all task
        [FunctionName(nameof(GetAllEmployed))]
        public static async Task<IActionResult> GetAllEmployed(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "employedIn")] HttpRequest req,
            [Table("Employed", Connection = "AzureWebJobsStorage")] CloudTable employedTable,
            ILogger log)
        //Siempre inyectar request aunque no se necesite
        {
            log.LogInformation("Get all employed received");

            TableQuery<EmployedEntity> query = new TableQuery<EmployedEntity>();
            TableQuerySegment<EmployedEntity> employed = await employedTable.ExecuteQuerySegmentedAsync(query, null);

            string message = "Retrieved all employed";
            log.LogInformation(message);

            return new OkObjectResult(new Response
            {
                IsSuccess = true,
                Message = message,
                Result = employed
            });
        }

        [FunctionName(nameof(DeleteEmployed))]
        public static async Task<IActionResult> DeleteEmployed(
           [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "employedIn/{id}")] HttpRequest req,
           [Table("Employed", "EMPLOYED", "{id}", Connection = "AzureWebJobsStorage")] EmployedEntity employedEntity,
           [Table("Employed", Connection = "AzureWebJobsStorage")] CloudTable employedTable,
           string id,
           ILogger log)
        //Siempre inyectar request aunque no se necesite
        {
            log.LogInformation($"Delete employed: {id}, received");

            if (employedEntity == null)
            {
                return new BadRequestObjectResult(new Response
                {
                    IsSuccess = false,
                    Message = "Employed not found."
                });
            }

            await employedTable.ExecuteAsync(TableOperation.Delete(employedEntity));
            string message = $"Task delete: {employedEntity.RowKey}, retreived";
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
