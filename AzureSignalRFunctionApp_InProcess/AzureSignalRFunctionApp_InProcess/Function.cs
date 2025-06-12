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
        public static async Task TimerBroadcast([TimerTrigger("*/10 * * * * *")] TimerInfo myTimer,
        [SignalR(HubName = "serverless")] IAsyncCollector<SignalRMessage> signalRMessages)
        {
            await signalRMessages.AddAsync(
            new SignalRMessage
            {
                Target = "PMCDemo",
                Arguments = [$"This message is for PMCDemo broadcast Target - Message from Timer-Broadcast method {DateTime.Now}"]
            });
        }

        [FunctionName("timerbroadcastv2")]
        public static async Task TimerBroadcastV2([TimerTrigger("*/15 * * * * *")] TimerInfo myTimer,
        [SignalR(HubName = "serverless")] IAsyncCollector<SignalRMessage> signalRMessages)
        {
            await signalRMessages.AddAsync(
            new SignalRMessage
            {
                Target = "PulteDemo",
                Arguments = [$"This message is for PulteDemo broadcast Target - Message from Timer-Broadcast V2 method {DateTime.Now}"]
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