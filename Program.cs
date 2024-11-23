using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using OnlineBank_FE;
using OnlineBank_FE.Services;
using Blazored.LocalStorage;


var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");

// Configure the HttpClient
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("https://localhost:7095/api/") });

// Register AuthService
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddScoped<AuthService>();

await builder.Build().RunAsync();
