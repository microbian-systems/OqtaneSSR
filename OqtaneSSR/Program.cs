using OqtaneSSR.Client.Components.Router;
using OqtaneSSR.Components;
using OqtaneSSR.Extensions;
using Microsoft.AspNetCore.Components;
using OqtaneSSR.Client.Services;
using OqtaneSSR.Client.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<SiteState>(); 
builder.Services.AddScoped<ITextService, OqtaneSSR.Services.TextService>();
builder.Services.AddScoped<IHttpClientService, HttpClientService>();

// Server Side Blazor doesn't register HttpClient by default
if (!builder.Services.Any(x => x.ServiceType == typeof(HttpClient)))
{
    // Setup HttpClient for server side in a client side compatible fashion
    builder.Services.AddScoped<HttpClient>(s =>
    {
        // Creating the NavigationManager needs to wait until the JS Runtime is initialized, so defer it.
        var navigationManager = s.GetRequiredService<NavigationManager>();
        return new HttpClient
        {
            BaseAddress = new Uri(navigationManager.BaseUri)
        };
    });
}

builder.Services.AddControllers();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapControllers();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(SiteRouter).Assembly);

app.MapFallback();

app.Run();
