using Microsoft.EntityFrameworkCore;
using StrangePower.Components;
using StrangePower.Data;
using StrangePower.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddDbContext<TokenDbContext>();

builder.Services.AddHttpClient("EloverblikClient", client => client.BaseAddress = new Uri("https://api.eloverblik.dk"));

builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<ITokenRepository, TokenRepository>();
builder.Services.AddScoped<IMeteringPointsService, MeteringPointsService>();
builder.Services.AddScoped<IMeteringPointRepository, MeteringPointRepository>();
builder.Services.AddScoped<IHttpClientTokenService, HttpClientTokenService>();

var app = builder.Build();

// Apply any pending migrations
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<TokenDbContext>();
    dbContext.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
   .AddInteractiveServerRenderMode();

app.Run();