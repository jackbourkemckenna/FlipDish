using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Flipdish.Recruiting.WebHookReceiver;
using Flipdish.Recruiting.WebHookReceiver.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Flipdish.Recrutting.WebhookReciver.Tests
{
    [TestFixture]
    public class Tests
    {


        [SetUp]
        public void Setup()
        {


          
        }

        [Test]
        public async Task Post_Missing_Body_Request_Returns_Error()
        {
            //Arrange
            var loggerMock = new Mock<ILogger>();
            ExecutionContext executionContext = new ExecutionContext();
            var request = new DefaultHttpRequest(new DefaultHttpContext())
            {
                Method = "POST",
                ContentType = "application/json",

            };

            //Act
            var result = (ContentResult)await WebHookReceiver.Run(request, loggerMock.Object, executionContext);

            //Assert
            Assert.AreEqual(result.Content, "No body found or test param.");
        }

        [Test]

        public async Task Post_Missing_StoreID_Returns_Error()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            ExecutionContext executionContext = new ExecutionContext();
            string workingDirectory = Environment.CurrentDirectory;
            var filePath =  Directory.GetParent(workingDirectory).Parent.Parent.FullName;

            var testWebHookJson = new StreamReader(filePath + "/JsonFiles/payload.json").ReadToEnd();

            //pulling orderId from json data
            var jsonDataOrderNumber  = JsonConvert.DeserializeObject<OrderCreatedWebHook>(testWebHookJson).Body.Order.OrderId;
            
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(testWebHookJson));

            var mockDict = new Dictionary<string, StringValues>
            {
                { "storeId", "null" }
            };
            var request = new DefaultHttpRequest(new DefaultHttpContext())
            {
                Method = "POST",
                ContentType = "application/json",
                Query = new QueryCollection(mockDict),
                Body = stream
            };

            //Act
            var result = (ContentResult)await WebHookReceiver.Run(request, loggerMock.Object, executionContext);

            //Assert
            Assert.AreEqual(result.Content, "No Store ID Skipping order #"+ jsonDataOrderNumber);
        }

    }
}