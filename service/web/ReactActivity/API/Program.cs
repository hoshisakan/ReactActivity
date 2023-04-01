using Persistence;
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

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    // app.UseHttpsRedirection();

    app.UseAuthorization();

    app.MapControllers();

    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;

    try
    {
        //TODO Read migration directory records, then according to the migration history table to determine whether to migrate the database.
        var context = services.GetRequiredService<DataContext>();
        await context.Database.MigrateAsync();
        await Seed.SeedData(context);
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