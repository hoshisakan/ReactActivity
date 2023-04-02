using Persistence;
using Persistence.Repository;
using Persistence.Repository.IRepository;
using Persistence.DbInitializer;


using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Serilog;
using Serilog.Events;

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
    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    //TODO Enable Legacy Timestamp Behavior for PostgreSQL.
    AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
    //TODO Enable DateTime Infinity Conversions for writable timestamp with time zone DateTime to PostgreSQL database.
    AppContext.SetSwitch("Npgsql.DisableDateTimeInfinityConversions", true);

    //TODO Add PostgreSQL database context and connection settting and change default migration save table from 'public' to specific schema name.
    builder.Services.AddDbContext<DataContext>(
        options => options.UseNpgsql(
            builder.Configuration.GetConnectionString("LocalTestConnecton"),
            x => x.MigrationsHistoryTable(
                HistoryRepository.DefaultTableName,
                builder.Configuration.GetSection("PostgreSQLConfigure:Schema").Get<string>()
            )
        )
    );

    string allowCorsOrigin = builder.Configuration.GetSection("CorsSettings:LocalTest:Origins").Get<string[]>()?[0] ?? string.Empty;
    string policyName = builder.Configuration.GetSection("CorsSettings:LocalTest:PolicyName").Get<string>() ?? string.Empty;

    if (!string.IsNullOrEmpty(allowCorsOrigin) && !string.IsNullOrEmpty(policyName))
    {
        builder.Services.AddCors(options =>
        {
            options.AddPolicy(policyName, policy =>
            {
                policy.AllowAnyHeader().AllowAnyMethod().WithOrigins(allowCorsOrigin);
            });
        });
    }

    builder.Services.AddScoped<IDbInitializer, DbInitializer>();
    builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

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