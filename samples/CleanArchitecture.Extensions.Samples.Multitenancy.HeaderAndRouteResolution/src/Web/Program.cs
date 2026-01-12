// Step 4: (Begin) Multitenancy middleware import
using CleanArchitecture.Extensions.Multitenancy.AspNetCore.Middleware;
// Step 4: (End) Multitenancy middleware import
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


app.UseExceptionHandler(options => { });

// Step 4: (Begin) Add multitenancy middleware between routing and auth
app.UseRouting();
app.UseCleanArchitectureMultitenancy();
app.UseAuthentication();
app.UseAuthorization();
// Step 4: (End) Add multitenancy middleware between routing and auth

app.Map("/", () => Results.Redirect("/api"));

app.MapEndpoints();

app.Run();

public partial class Program { }
