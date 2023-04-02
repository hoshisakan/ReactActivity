using Persistence.DbInitializer;
using API.Extensions;


using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Serilog;
using Serilog.Events;
using MediatR;


Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext()
    );

    // Add services to the container.

    builder.Services.AddControllers();
    builder.Services.AddApplicationServices(builder.Configuration);

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    string policyName = builder.Configuration.GetSection("CorsSettings:LocalTest:PolicyName").Get<string>() ?? string.Empty;
    app.UseCors(policyName);

    // app.UseHttpsRedirection();

    app.UseAuthorization();

    app.MapControllers();

    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;

    try
    {
        IDbInitializer dbInitializer = services.GetRequiredService<IDbInitializer>();
        await dbInitializer.SeedData();
    }
    catch (Exception ex)
    {
        Log.Error($"An error occurred while migrating or seeding the database: {ex.Message}");
    }

    app.Run();
}
catch (Exception ex)
{
    Log.Error($"An error occurred while starting the application: {ex.Message}");
}
finally
{
    Log.CloseAndFlush();
}