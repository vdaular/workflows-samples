using System.Net.Http.Headers;
using EmployeeOnboarding.Web.Data;
using EmployeeOnboarding.Web.HostedServices;
using EmployeeOnboarding.Web.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContextFactory<OnboardingDbContext>(options => options.UseSqlite("Data Source=onboarding.db"));
builder.Services.AddHostedService<MigrationsHostedService>();

builder.Services.AddHttpClient<ElsaClient>(async httpClient =>
{
    var url = configuration["Elsa:ServerUrl"]!.TrimEnd('/') + '/';
    httpClient.BaseAddress = new Uri(url);

    // Get access token from Microsoft Entra
    var tenantId = configuration["AzureAd:TenantId"];
    var clientId = configuration["AzureAd:ClientId"];
    var clientSecret = configuration["AzureAd:ClientSecret"];
    var scope = "api://0000-0000-0000/.default";

    var app = ConfidentialClientApplicationBuilder.Create(clientId)
        .WithClientSecret(clientSecret)
        .WithAuthority(new Uri($"https://login.microsoftonline.com/{tenantId}"))
        .Build();

    // Acquire token for the API
    var result = await app.AcquireTokenForClient(new[] { scope }).ExecuteAsync();
    var accessToken = result.AccessToken;

    // Set Bearer token in the Authorization header
    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
