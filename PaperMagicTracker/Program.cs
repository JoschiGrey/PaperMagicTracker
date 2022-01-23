using Blazor.Extensions.Logging;
using Blazor.Extensions.Storage;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using PaperMagicTracker;
using PaperMagicTracker.Classes;
using Syncfusion.Blazor;
using Syncfusion.Licensing;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");
builder.Services.AddSyncfusionBlazor();
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddLogging(logger => logger.AddBrowserConsole().SetMinimumLevel(LogLevel.Trace));

builder.Services.AddStorage();

builder.Services.AddSingleton<AppState>();

var client = new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)};

var licenseKey = client.GetStringAsync(@"sample-data\/SyncfusionLicense.txt");
SyncfusionLicenseProvider.RegisterLicense(await licenseKey);

await builder.Build().RunAsync();