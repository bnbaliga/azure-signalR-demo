using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace AzureFunctionAppSignalR
{
    public class Functions
    {
        private static readonly HttpClient HttpClient = new();
        private static string Etag = string.Empty;
        private static int StarCount = 0;

        [Function("index")]
        public static HttpResponseData GetHomePage([HttpTrigger(AuthorizationLevel.Anonymous)] HttpRequestData req)
        {
            var response = req.CreateResponse(HttpStatusCode.OK);
            response.WriteString(File.ReadAllText("content/index.html"));
            response.Headers.Add("Content-Type", "text/html");
            return response;
        }

        [Function("negotiate")]
        public async Task<HttpResponseData> Negotiate([HttpTrigger(AuthorizationLevel.Anonymous)] HttpRequestData req,
            [SignalRConnectionInfoInput(HubName = "serverless")] string connectionInfo)
        {
            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "application/json");
            await response.WriteStringAsync(connectionInfo);

            return response;
        }



        //[Function("broadcast")]
        //[SignalROutput(HubName = "serverless", ConnectionStringSetting = "AzureSignalRConnectionString")]
        //public static async Task<SignalRMessageAction> Broadcast([TimerTrigger("*/5 * * * * *")] TimerInfo timerInfo)
        //{
        //    var request = new HttpRequestMessage(HttpMethod.Get, "https://api.github.com/repos/azure/azure-signalr");
        //    request.Headers.UserAgent.ParseAdd("Serverless");
        //    request.Headers.Add("If-None-Match", Etag);
        //    var response = await HttpClient.SendAsync(request);
        //    if (response.Headers.Contains("Etag"))
        //    {
        //        Etag = response.Headers.GetValues("Etag").First();
        //    }
        //    if (response.StatusCode == HttpStatusCode.OK)
        //    {
        //        var result = await response.Content.ReadFromJsonAsync<GitResult>();
        //        if (result != null)
        //        {
        //            StarCount = result.StarCount;
        //        }
        //    }
        //    return new SignalRMessageAction("newMessage", new object[] { $"Current star count of https://github.com/Azure/azure-signalr is: {StarCount}" });
        //}

        private class GitResult
        {
            [JsonPropertyName("stargazers_count")]
            public int StarCount { get; set; }
        }
    }

}


//[OpenApiOperation(operationId: "greeting", tags: new[] { "greeting" }, Summary = "Greetings", Description = "This shows a welcome message.", Visibility = OpenApiVisibilityType.Important)]
//[OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
//[OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Summary = "The response", Description = "This returns the response")]
//[Function("Run")]
//public IActionResult Run([Microsoft.Azure.Functions.Worker.HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req)
//{
//    _logger.LogInformation("C# HTTP trigger function processed a request.");
//    return new OkObjectResult("Welcome to Azure Functions!");
//}
