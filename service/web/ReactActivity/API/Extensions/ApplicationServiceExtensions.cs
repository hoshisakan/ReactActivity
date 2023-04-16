using Persistence;
using Persistence.DbInitializer;
using Application.Activities;
using Application.Core;
using Application.Interfaces;
using Infrastructure.Security;
using Infrastructure.Photos;


using Microsoft.EntityFrameworkCore;
using MediatR;
using Microsoft.EntityFrameworkCore.Migrations;
using FluentValidation;
using FluentValidation.AspNetCore;

namespace API.Extensions
{
    public static class ApplicationServiceExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services,
            IConfiguration config)
        {
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();

            //TODO: Enable Legacy Timestamp Behavior for PostgreSQL.
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
            //TODO: Enable DateTime Infinity Conversions for writable timestamp with time zone DateTime to PostgreSQL database.
            AppContext.SetSwitch("Npgsql.DisableDateTimeInfinityConversions", true);

            //TODO: Add PostgreSQL database context and connection setting and change default migration save table from 'public' to specific schema name.
            services.AddDbContext<DataContext>(
                options => options.UseNpgsql(
                    config.GetConnectionString("LocalTestConnection"),
                    x => x.MigrationsHistoryTable(
                        HistoryRepository.DefaultTableName,
                        config.GetSection("PostgreSQLConfigure:Schema").Get<string>()
                    )
                )
            );

            string allowCorsOrigin = config.GetSection("CorsSettings:LocalTest:Origins").Get<string[]>()?[0] ?? string.Empty;
            string policyName = config.GetSection("CorsSettings:LocalTest:PolicyName").Get<string>() ?? string.Empty;

            if (!string.IsNullOrEmpty(allowCorsOrigin) && !string.IsNullOrEmpty(policyName))
            {
                services.AddCors(options =>
                {
                    options.AddPolicy(policyName, policy =>
                    {
                        policy
                            .AllowAnyMethod()
                            .AllowAnyHeader()
                            .AllowCredentials()
                            .WithOrigins(allowCorsOrigin);
                    });
                });
            }

            services.AddScoped<IDbInitializer, DbInitializer>();
            services.AddMediatR(typeof(List.Handler).Assembly);
            services.AddAutoMapper(typeof(MappingProfiles).Assembly);
            services.AddFluentValidationAutoValidation();
            services.AddValidatorsFromAssemblyContaining<Create>();
            services.AddHttpContextAccessor();
            services.AddScoped<IUserAccessor, UserAccessor>();
            services.AddScoped<IPhotoAccessor, PhotoAccessor>();
            services.Configure<CloudinarySettings>(config.GetSection("Cloudinary"));
            services.AddSignalR();

            return services;
        }
    }
}