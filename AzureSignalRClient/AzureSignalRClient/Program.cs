using AzureSignalRClient.Components;

namespace AzureSignalRClient
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();

            builder.Services.AddSignalR()
                .AddAzureSignalR(options =>
                {
                    // Configure Azure SignalR Service connection string
                    options.ConnectionString = builder.Configuration.GetConnectionString("AzureSignalRConnectionString");
                });

            builder.Services.AddHttpClient<SignalRConnector>(client =>
            {
                // Configure the HttpClient for SignalR negotiation
                client.BaseAddress = new Uri(builder.Configuration["SignalRBaseUrl"] ?? "http://localhost:7041/api/negotiate");
                client.DefaultRequestHeaders.Add("Accept", "application/json");
            });

            var app = builder.Build();

            var access = new SignalRConnector(builder.Services.BuildServiceProvider().GetRequiredService<HttpClient>());

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseStaticFiles();
            app.UseAntiforgery();

            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

            app.Run();
        }

    }
}
