using Elsa.EntityFrameworkCore.Modules.Management;
using Elsa.EntityFrameworkCore.Modules.Runtime;
using Elsa.Extensions;
using WorkflowApp.Web;
using WorkflowApp.Web.Workflows;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddRazorPages();

// Add Elsa services.
builder.Services.AddElsa(elsa =>
{
    elsa.UseIdentity(identity =>
    {
        identity.UseAdminUserProvider();
        identity.TokenOptions = tokenOptions => tokenOptions.SigningKey = "my-secret-signing-key";
    });
    elsa.UseDefaultAuthentication();
    elsa.UseWorkflowManagement(management =>
    {
        management.UseEntityFrameworkCore();
        management.AddVariableType<WeatherForecast>(category: "Weather");
    });
    elsa.UseWorkflowRuntime(runtime => runtime.UseEntityFrameworkCore());
    elsa.UseJavaScript();
    elsa.UseLiquid();
    elsa.UseWorkflowsApi();
    elsa.UseHttp(http => http.ConfigureHttpOptions = options => options.BasePath = "/workflows");
    elsa.AddWorkflow<WeatherForecastWorkflow>();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.UseWorkflowsApi();
app.UseWorkflows();
app.MapControllers();
app.MapRazorPages();
app.Run();