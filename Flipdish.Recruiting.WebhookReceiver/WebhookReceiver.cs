using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using Flipdish.Recruiting.WebHookReceiver.Models;

namespace Flipdish.Recruiting.WebHookReceiver
{
    public static class WebHookReceiver
    {
        
        [FunctionName("WebHookReceiver")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req, ILogger log, ExecutionContext context) 
        {
            try
            {
                log.LogInformation("C# HTTP trigger function processed a request.");

                OrderCreatedWebHook orderCreatedWebHook;
                var requestBody = await RequestBody(req);

                // move this to a test project create mock request based on the json file
                // can you run integration test azure function?
                string test = req.Query["test"];
                if(req.Method == "GET" && !string.IsNullOrEmpty(test))
                {

                    var templateFilePath = Path.Combine(context.FunctionAppDirectory, "TestWebhooks", test);
                    var testWebhookJson = new StreamReader(templateFilePath).ReadToEnd();

                    orderCreatedWebHook = JsonConvert.DeserializeObject<OrderCreatedWebHook>(testWebhookJson);
                }
                else if (req.Method == "POST" && !string.IsNullOrEmpty(requestBody))
                {
                   
                    orderCreatedWebHook = JsonConvert.DeserializeObject<OrderCreatedWebHook>(requestBody);
                }
                else
                {
                    return new ContentResult { Content = $"No body found or test param.", ContentType = "text/html" };
                }

                var orderCreatedEvent = orderCreatedWebHook.Body;
                var orderId = orderCreatedEvent.Order.OrderId;
                string[] storeIdParams = req.Query["storeId"].ToArray();
                var storeIds = ParseStoreIds(storeIdParams);
                if (!storeIds.Any()){
                        log.LogInformation($"No Store ID Skipping order #{orderId}");
                        return new ContentResult { Content = $"Skipping order #{orderId}", ContentType = "text/html" };
                }
                
                var currency = CurrencyToUpper(req.Query["currency"].FirstOrDefault());
                var barcodeMetadataKey = req.Query["metadataKey"].First() ?? "eancode";

                try
                {
                    var renderer = new EmailRenderer(orderCreatedEvent.Order, orderCreatedEvent.AppId, barcodeMetadataKey, context.FunctionAppDirectory, log, (Currency)currency);
                    await EmailService.Send("", req.Query["to"], $"New Order #{orderId}", renderer.RenderEmailOrder(), renderer._imagesWithNames);
                    return new ContentResult { Content = renderer.RenderEmailOrder(), ContentType = "text/html" };

                }
                catch (Exception ex)
                {
                    log.LogError(ex, $"Error sending email for order #{orderId}");
                }
            }
            catch(Exception ex)
            {

                log.LogError(ex, $"Error occured during processing of Rquest ");
                throw ex;
            }

            return new ContentResult { Content = $"Error when trying to render email", ContentType = "text/html" };
        }

        private static async Task<string> RequestBody(HttpRequest req)
        {
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            return requestBody;
        }

        private static Currency? CurrencyToUpper(string currencyString)
        {
            if (!string.IsNullOrEmpty(currencyString) &&
                Enum.TryParse(typeof(Currency), currencyString.ToUpper(), out object currencyUpper))
            {
                return (Currency)currencyUpper;
            }

            return null;
        }

        private static List<int> ParseStoreIds(string[] storeIdParams)
        {
            var storeIds = new List<int>();
            foreach (var storeIdString in storeIdParams)
            {
                int storeId = 0;
                try
                {
                    storeId = int.Parse(storeIdString);
                }
                catch (Exception) { }

                storeIds.Add(storeId);
            }

            return storeIds;
        }
    }
}
