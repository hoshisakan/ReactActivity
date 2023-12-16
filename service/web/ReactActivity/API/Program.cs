using Persistence.DbInitializer;
using Infrastructure.StaticFilePathInitializer;
using API.Extensions;
using API.Middleware;
using API.SignalR;

using Serilog;
using Serilog.Events;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.DependencyInjection;
using API.Services;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .CreateLogger();

try
{
    WebApplicationBuilder? builder = WebApplication.CreateBuilder(args);
    
    builder.Services.AddControllers(opt =>
    {
        AuthorizationPolicy policy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
        opt.Filters.Add(new AuthorizeFilter(policy));
    });

    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext()
    );

    // Add services to the container.
    builder.Services.AddControllers();
    builder.Services.AddApplicationServices(builder.Configuration);
    builder.Services.AddIdentityServices(builder.Configuration);

    bool isDevelopment = builder.Environment.IsDevelopment();

    if (isDevelopment)
    {
        Console.WriteLine("Development environment");
        builder.Services.AddDevDataBackupServices(builder.Configuration);
    }
    else
    {
        Console.WriteLine("Production environment");
        builder.Services.AddProductDataBackupServices(builder.Configuration);
    }

    // builder.WebHost.UseKestrel(options =>
    // {
    //     options.ListenAnyIP(builder.Configuration.GetSection("KestrelSettings:Endpoints:Http:Port").Get<int>());
    //     options.Limits.MaxRequestBodySize = int.MaxValue;
    // });

    builder.WebHost.UseKestrel(options =>
    {
        options.ListenAnyIP(builder.Configuration.GetSection("KestrelSettings:Endpoints:Http:Port").Get<int>());
        options.ListenAnyIP(builder.Configuration.GetSection("KestrelSettings:Endpoints:Https:Port").Get<int>(), listenOptions =>
        {
            listenOptions.UseHttps(
                builder.Configuration["KestrelSettings:Certificates:Default:Path"],
                builder.Configuration["KestrelSettings:Certificates:Default:Password"]
            );
        });
        options.Limits.MaxRequestBodySize = int.MaxValue;
    });

    WebApplication? app = builder.Build();

    // Configure the HTTP request pipeline.
    app.UseMiddleware<ExceptionMiddleware>();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    string policyName = builder.Configuration.GetSection("CorsSettings:PolicyName").Get<string>() ?? string.Empty;
    app.UseCors(policyName);

    // app.UseHttpsRedirection();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();
    app.MapHub<ChatHub>("/chat");

    using IServiceScope? scope = app.Services.CreateScope();
    IServiceProvider? services = scope.ServiceProvider;

    try
    {
        IDataRestoreService dataRestoreService = services.GetRequiredService<IDataRestoreService>();
        IDbInitializer dbInitializer = services.GetRequiredService<IDbInitializer>();
        bool isDatabaseExist = await dbInitializer.SeedData();
        IStaticFilePathInitializer staticFilePathInitializer = services.GetRequiredService<IStaticFilePathInitializer>();
        staticFilePathInitializer.StoragePathInitializer();
        if (!isDatabaseExist)
        {
            await dataRestoreService.RestoreData(isDevelopment);
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"An error occurred while migrating or seeding the database or initializing the static file path: {ex.Message}");
        Log.Error($"An error occurred while migrating or seeding the database or initializing the static file path: {ex.Message}");
    }

    app.Run();
}
catch (Exception ex)
{
    Console.WriteLine($"An error occurred while starting the application\nMessage: {ex.Message}\nStack trace: {ex.StackTrace}");
    Log.Error($"An error occurred while starting the application\nMessage: {ex.Message}\nStack trace: {ex.StackTrace}");
}
finally
{
    Log.CloseAndFlush();
}