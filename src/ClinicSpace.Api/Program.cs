using ClinicSpace.Api.Controllers;
using ClinicSpace.Application;
using ClinicSpace.Infrastructure;
using ClinicSpace.Infrastructure.Identity;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = AuthController.AntiforgeryHeaderName;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict;
});
builder.Services.AddControllers(options =>
    // Every POST/PUT/PATCH/DELETE must carry the antiforgery header; GETs are exempt.
    // Opt an action out with [IgnoreAntiforgeryToken].
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute()));
// AddControllers() leaves out the view features, which register the filter that
// AutoValidateAntiforgeryTokenAttribute resolves from DI. This adds them without Razor.
builder.Services.AddMvcCore().AddViews();
builder.Services.AddOpenApi();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    using var scope = app.Services.CreateScope();
    await scope.ServiceProvider.GetRequiredService<DevDataSeeder>().SeedAsync();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

// Exposed for WebApplicationFactory in ClinicSpace.Api.Tests.
public partial class Program;
