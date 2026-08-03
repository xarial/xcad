using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Web.Blazor;
using Xarial.XCad.Base;
using Xarial.XCad.Utils.Diagnostics;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddSingleton<IXLogger>(new TraceLogger("XCad.Blazor.Wasm"));

await builder.Build().RunAsync();
