using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

var host = builder.Build();
await host.RunAsync().ConfigureAwait(false);
