using Radzen;
using AzureSignalRClientWithRadzenBlazor.Components;
using AzureSignalRClientWithRadzenBlazor.SignalR;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
      .AddInteractiveServerComponents().AddHubOptions(options => options.MaximumReceiveMessageSize = 10 * 1024 * 1024);

builder.Services.AddControllers();
builder.Services.AddRadzenComponents();
builder.Services.AddSingleton<NotificationService>();

builder.Services.AddRadzenCookieThemeService(options =>
{
    options.Name = "AzureSignalRClientWithRadzenBlazorTheme";
    options.Duration = TimeSpan.FromDays(365);
});
builder.Services.AddHttpClient();
builder.Services.AddSingleton<ISignalRConnector, SignalRConnector>();

builder.Services.AddSignalR();
//builder.Services.AddSignalR()
//    .AddAzureSignalR(options =>
//    {
//        // Configure Azure SignalR Service connection string
//        options.ConnectionString = builder.Configuration.GetConnectionString("AzureSignalRConnectionString");
//    });

builder.Services.AddHttpClient("SignalRApi", client =>
{
    // Configure the HttpClient for SignalR negotiation
    client.BaseAddress = new Uri(builder.Configuration["SignalRBaseUrl"] ?? "http://localhost:7041/");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

var app = builder.Build();


var forwardingOptions = new ForwardedHeadersOptions()
{
    ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor | Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto
};
forwardingOptions.KnownNetworks.Clear();
forwardingOptions.KnownProxies.Clear();

app.UseForwardedHeaders(forwardingOptions);
    

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.MapControllers();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
   .AddInteractiveServerRenderMode();

app.Run();