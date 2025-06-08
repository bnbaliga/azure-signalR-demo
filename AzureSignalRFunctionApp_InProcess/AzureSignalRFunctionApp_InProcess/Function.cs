using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace AzureSignalRFunctionApp_InProcess
{
    public static class Function
    {
        private static HttpClient httpClient = new HttpClient();
        private static string Etag = string.Empty;
        private static string StarCount = "0";

        [FunctionName("negotiate")]
        public static SignalRConnectionInfo Negotiate(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req,
            [SignalRConnectionInfo(HubName = "serverless")] SignalRConnectionInfo connectionInfo)
        {
            return connectionInfo;
        }

        [FunctionName("broadcast")]
        public static async Task Broadcast([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req,
        [SignalR(HubName = "serverless")] IAsyncCollector<SignalRMessage> signalRMessages)
        {
            await signalRMessages.AddAsync(
            new SignalRMessage
            {
                Target = "PMCDemo",
                Arguments = ["This is a sample message"]
            });
        }

        [FunctionName("timerbroadcast")]
        public static async Task TimerBroadcast([TimerTrigger("*/5 * * * * *")] TimerInfo myTimer,
        [SignalR(HubName = "serverless")] IAsyncCollector<SignalRMessage> signalRMessages)
        {
            //var request = new HttpRequestMessage(HttpMethod.Get, "https://api.github.com/repos/azure/azure-signalr");
            //request.Headers.UserAgent.ParseAdd("Serverless");
            //request.Headers.Add("If-None-Match", Etag);
            //var response = await httpClient.SendAsync(request);
            //if (response.Headers.Contains("Etag"))
            //{
            //    Etag = response.Headers.GetValues("Etag").First();
            //}
            //if (response.StatusCode == System.Net.HttpStatusCode.OK)
            //{
            //    var result = JsonConvert.DeserializeObject<GitResult>(await response.Content.ReadAsStringAsync());
            //    StarCount = result.StarCount;
            //}

            await signalRMessages.AddAsync(
            new SignalRMessage
            {
                Target = "PMCDemo",
                Arguments = [$"Message from Timer-Broadcast method {DateTime.Now}"]
            });
        }

        private class GitResult
        {
            [JsonRequired]
            [JsonProperty("stargazers_count")]
            public string StarCount { get; set; }
        }
    }
}