using AzureSignalRClientWithRadzenBlazor.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace AzureSignalRClientWithRadzenBlazor.SignalR
{
    public class SignalRConnector : ISignalRConnector
    {
        private HttpClient HttpClient;
        private HubConnection _hubConnection;
        private string connectionState = "Not connected";

        public SignalRConnector(IHttpClientFactory httpClientFactory)
        {
            HttpClient = httpClientFactory.CreateClient("SignalRApi");
        }

        public async Task StartConnection()
        {
            try
            {
                // 1. Negotiate with SignalR
                var negotiationResult = await NegotiateAsync();

                // 2. Establish Hub Connection
                if (negotiationResult != null && negotiationResult.Url != null)
                {
                    _hubConnection = new HubConnectionBuilder()
                        .WithUrl(negotiationResult.Url, options =>
                        {
                            options.AccessTokenProvider = () => Task.FromResult(negotiationResult.AccessToken);
                        })
                        .WithAutomaticReconnect()
                        .Build();

                    // 3. Start the Hub Connection
                    await _hubConnection.StartAsync();
                    connectionState = "Connected";
                }
                else
                {
                    connectionState = "Negotiation failed";
                }
            }
            catch (Exception ex)
            {
                connectionState = "Connection error: " + ex.Message;
            }
        }

        private async Task<NegotiationResult> NegotiateAsync()
        {
            try
            {
                var response = await HttpClient.PostAsync("/api/negotiate", null); // Or use GET
                response.EnsureSuccessStatusCode(); // Or handle different status codes

                // Deserialize the response (assuming JSON)
                var json = await response.Content.ReadAsStringAsync();
                var negotiationResult = JsonSerializer.Deserialize<NegotiationResult>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return negotiationResult;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Negotiation error: " + ex.Message);
                return null;
            }
        }
    }
}
