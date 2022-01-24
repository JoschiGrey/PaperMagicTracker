using Blazor.Extensions.Logging;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using PaperMagicTracker;
using PaperMagicTracker.Classes;
using Syncfusion.Blazor;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");
builder.Services.AddSyncfusionBlazor();
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddLogging(logger => logger.AddBrowserConsole().SetMinimumLevel(LogLevel.Trace));

builder.Services.AddBlazoredLocalStorage();

builder.Services.AddSingleton<AppState>();

await builder.Build().RunAsync();