using AzureSignalRClientWithRadzenBlazor.Models;
using Microsoft.AspNetCore.SignalR.Client;
using Radzen;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AzureSignalRClientWithRadzenBlazor.SignalR
{
    public class SignalRConnector : ISignalRConnector
    {
        private HttpClient HttpClient;
        private HubConnection _hubConnection;
        private string connectionState = "Not connected";
        private readonly NotificationService _notificationService;

        public SignalRConnector(IHttpClientFactory httpClientFactory, NotificationService notificationService)
        {
            HttpClient = httpClientFactory.CreateClient("SignalRApi");
            _notificationService = notificationService;
        }

        public async Task StartConnection()
        {
            if (_hubConnection != null && _hubConnection.State == HubConnectionState.Connected)
            {
                connectionState = "Already connected";
                return;
            }

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
                        .AddJsonProtocol(cfg =>
                        {
                            var jsonOptions = new JsonSerializerOptions
                            {
                                PropertyNameCaseInsensitive = true,
                            };
                            jsonOptions.Converters.Add(new JsonStringEnumConverter());

                            cfg.PayloadSerializerOptions = jsonOptions;
                        })
                        .WithAutomaticReconnect()
                        .Build();

                    // 3. Start the Hub Connection
                    await _hubConnection.StartAsync();
                    connectionState = "Connected";

                    _hubConnection.On<string>("PMCDemo", (message) =>
                    {
                        
                        var notificaitonMessage =
                            new NotificationMessage
                            {
                                Severity = NotificationSeverity.Info,
                                Summary = "A new Message for PMCDemo was received",
                                Detail = message,
                                Duration = 2000
                            };

                        _notificationService.Notify(notificaitonMessage);
                        Console.WriteLine($"Message received: {message}");
                        // Handle the message as needed
                    });

                    _hubConnection.On<string>("PulteDemo", (message) =>
                    {

                        var notificaitonMessage =
                            new NotificationMessage
                            {
                                Severity = NotificationSeverity.Success,
                                Summary = "A new Message for PulteDemo received",
                                Detail = message,
                                Duration = 2000
                            };

                        _notificationService.Notify(notificaitonMessage);
                        Console.WriteLine($"Message received: {message}");
                        // Handle the message as needed
                    });
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
