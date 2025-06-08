using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using AzureFunctionAppSignalR_Isolated.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Newtonsoft.Json;

namespace AzureFunctionAppSignalR
{
    public class Functions
    {
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

        [Function("broadcast")]
        public async Task<SignalRMessageAction> Broadcast([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req,
            [SignalRConnectionInfoInput(HubName = "serverless")] string connectionInfo)
        {
            var broadcastModel = new BroadcastModel();
            var query = await req.ReadAsStringAsync();

            if (string.IsNullOrEmpty(query))
                throw new ArgumentException("Query cannot be null or empty.");

            broadcastModel = JsonConvert.DeserializeObject<BroadcastModel>(query);

            if (broadcastModel == null || string.IsNullOrEmpty(broadcastModel.Target) || string.IsNullOrEmpty(broadcastModel.Message))
                throw new ArgumentException("Invalid broadcast model.");

            return new SignalRMessageAction(broadcastModel.Target, [broadcastModel.Message]);
        }


        //[function("broadcast")]
        //[signalroutput(hubname = "serverless", connectionstringsetting = "azuresignalrconnectionstring")]
        //public static async task<signalrmessageaction> broadcast([timertrigger("*/5 * * * * *")] timerinfo timerinfo)
        //{
        //    var request = new httprequestmessage(httpmethod.get, "https://api.github.com/repos/azure/azure-signalr");
        //    request.headers.useragent.parseadd("serverless");
        //    request.headers.add("if-none-match", etag);
        //    var response = await httpclient.sendasync(request);
        //    if (response.headers.contains("etag"))
        //    {
        //        etag = response.headers.getvalues("etag").first();
        //    }
        //    if (response.statuscode == httpstatuscode.ok)
        //    {
        //        var result = await response.content.readfromjsonasync<gitresult>();
        //        if (result != null)
        //        {
        //            starcount = result.starcount;
        //        }
        //    }
        //    return new signalrmessageaction("newmessage", new object[] { $"current star count of https://github.com/azure/azure-signalr is: {starcount}" });
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
