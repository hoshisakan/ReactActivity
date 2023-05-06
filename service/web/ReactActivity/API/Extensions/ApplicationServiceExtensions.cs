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
                    config.GetConnectionString("DefaultConnection"),
                    x => x.MigrationsHistoryTable(
                        HistoryRepository.DefaultTableName,
                        config.GetSection("PostgreSQLConfigure:Schema").Get<string>()
                    )
                )
            );

            //TODO: Add Redis cache service.
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = config.GetConnectionString("RedisConnection");
                options.InstanceName = config.GetSection("RedisSettings:InstanceName").Get<string>();
            });

            string[] allowCorsOrigin = config.GetSection("CorsSettings:Origins").Get<string[]>() ?? new string[] {};
            string policyName = config.GetSection("CorsSettings:PolicyName").Get<string>() ?? string.Empty;

            if (allowCorsOrigin.Length > 0 && !allowCorsOrigin.Any(a => String.IsNullOrEmpty(a)) && !string.IsNullOrEmpty(policyName))
            {
                services.AddCors(options =>
                {
                    options.AddPolicy(policyName, policy =>
                    {
                        policy
                            .AllowAnyMethod()
                            .AllowAnyHeader()
                            .AllowCredentials()
                            .WithExposedHeaders("WWW-Authenticate", "Pagination")
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