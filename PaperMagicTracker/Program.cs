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

builder.Services.AddSingleton<AppState>();

var client = new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)};

GlobalStaticResources.Client = client;

var licenseKey = client.GetStringAsync(@"sample-data\/SyncfusionLicense.txt");

var idDicTask = IdToOracleId.FormIdToOracleIdDic(client);
var allCardDicTask = AllOracleCards.FormAllOracleCardDictionary(client);

SyncfusionLicenseProvider.RegisterLicense(await licenseKey);

await Task.WhenAll(idDicTask, allCardDicTask);

await builder.Build().RunAsync();
