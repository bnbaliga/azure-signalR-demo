using AzureSignalRClient.Models;
using Microsoft.AspNetCore.SignalR.Client;
using System.Text.Json;

namespace AzureSignalRClient
{
    public class SignalRConnector
    {
        private HttpClient HttpClient;
        private HubConnection? _hubConnection;
        private string connectionState = "Not connected";

        public SignalRConnector(HttpClient httpClient)
        {
            HttpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
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
                        .WithUrl(negotiationResult.Url) // Use the negotiated URL
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

        private async Task<NegotiationResult?> NegotiateAsync()
        {
            try
            {
                var response = await HttpClient.PostAsync("/signalr/negotiate", null); // Or use GET
                response.EnsureSuccessStatusCode(); // Or handle different status codes

                // Deserialize the response (assuming JSON)
                var json = await response.Content.ReadAsStringAsync();
                var negotiationResult = JsonSerializer.Deserialize<NegotiationResult>(json);
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
