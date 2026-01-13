// Step 4: (Begin) Multitenancy middleware import
using CleanArchitecture.Extensions.Multitenancy.AspNetCore.Middleware;
// Step 4: (End) Multitenancy middleware import
// Step 6: (Begin) Tenant requirement routing helpers
using CleanArchitecture.Extensions.Multitenancy.AspNetCore.Routing;
// Step 6: (End) Tenant requirement routing helpers
using CleanArchitecture.Extensions.Samples.Multitenancy.HeaderAndRouteResolution.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.AddKeyVaultIfConfigured();
builder.AddApplicationServices();
builder.AddInfrastructureServices();
builder.AddWebServices();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    await app.InitialiseDatabaseAsync();
}
else
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHealthChecks("/health");
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseSwaggerUi(settings =>
{
    settings.Path = "/api";
    settings.DocumentPath = "/api/specification.json";
});


// Step 7: (Begin) Enable exception handlers for ProblemDetails responses
app.UseExceptionHandler();
// Step 7: (End) Enable exception handlers for ProblemDetails responses

// Step 4: (Begin) Add multitenancy middleware between routing and auth
app.UseRouting();
app.UseCleanArchitectureMultitenancy();
app.UseAuthentication();
app.UseAuthorization();
// Step 4: (End) Add multitenancy middleware between routing and auth

// Step 6: (Begin) Allow tenant-less access for public endpoints
app.Map("/", () => Results.Redirect("/api"))
    .AddTenantEnforcement()
    .AllowAnonymousTenant();
// Step 6: (End) Allow tenant-less access for public endpoints

app.MapEndpoints();

app.Run();

public partial class Program { }
