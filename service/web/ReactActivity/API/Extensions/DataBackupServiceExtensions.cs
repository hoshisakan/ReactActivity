using API.Services;
using Quartz;

namespace API.Extensions
{
    public static class DataBackupServiceExtensions
    {
        public static IServiceCollection AddDataBackupServices(this IServiceCollection services,
            IConfiguration config)
        {
            bool developmentBackupAllowed = config.GetSection("DataBackupSettings:Development:Allowed").Get<bool>();
            string developmentBackupIdentity = string.Empty;
            string developmentBackupTarget = string.Empty;
            string[] developmentBackupScheduleList = new string[] {};
            string developmentBackupSchedule = string.Empty;

            bool productionBackupAllowed = config.GetSection("DataBackupSettings:Production:Allowed").Get<bool>();
            string productionBackupIdentity = string.Empty;
            string productionBackupTarget = string.Empty;
            string[] productionBackupScheduleList = new string[] {};
            string productionBackupSchedule = string.Empty;

            string currentDayOfWeek = DateTime.Today.DayOfWeek.ToString();

            if (developmentBackupAllowed)
            {
                developmentBackupIdentity = config.GetSection("DataBackupSettings:Development:Identity").Get<string>() ?? string.Empty;
                developmentBackupTarget = config.GetSection("DataBackupSettings:Development:Target").Get<string>() ?? string.Empty;
                developmentBackupScheduleList = config.GetSection(
                    "DataBackupSettings:Development:CronSchedule"
                ).Get<string[]>() ?? new string[] {};

                if (string.IsNullOrEmpty(developmentBackupIdentity)
                    || string.IsNullOrEmpty(developmentBackupTarget)
                    || developmentBackupScheduleList.Length == 0
                    || developmentBackupScheduleList.Any(a => string.IsNullOrEmpty(a))
                )
                {
                    throw new Exception("Data backup settings are not set.");
                }

                if (currentDayOfWeek == "Saturday" || currentDayOfWeek == "Sunday")
                {
                    developmentBackupSchedule = developmentBackupScheduleList[0];
                }
                else
                {
                    developmentBackupSchedule = developmentBackupScheduleList[1];
                }

                if (string.IsNullOrEmpty(developmentBackupSchedule))
                {
                    throw new Exception("Data backup settings are not set.");
                }

                
            }

            if (productionBackupAllowed)
            {
                productionBackupIdentity = config.GetSection("DataBackupSettings:Production:Identity").Get<string>() ?? string.Empty;
                productionBackupTarget = config.GetSection("DataBackupSettings:Production:Target").Get<string>() ?? string.Empty;
                productionBackupScheduleList = config.GetSection(
                    "DataBackupSettings:Production:CronSchedule"
                ).Get<string[]>() ?? new string[] {};

                if (string.IsNullOrEmpty(productionBackupIdentity)
                    || string.IsNullOrEmpty(productionBackupTarget)
                    || productionBackupScheduleList.Length == 0
                    || productionBackupScheduleList.Any(a => string.IsNullOrEmpty(a))
                )
                {
                    throw new Exception("Data backup settings are not set.");
                }

                if (currentDayOfWeek == "Saturday" || currentDayOfWeek == "Sunday")
                {
                    productionBackupSchedule = productionBackupScheduleList[0];
                }
                else
                {
                    productionBackupSchedule = productionBackupScheduleList[1];
                }

                if (string.IsNullOrEmpty(productionBackupSchedule))
                {
                    throw new Exception("Data backup settings are not set.");
                }
            }

            if (!developmentBackupAllowed && !productionBackupAllowed)
            {
                throw new Exception("Data backup not allowed.");
            }
            else if (developmentBackupAllowed && productionBackupAllowed)
            {
                services.AddQuartz(q =>
                {
                    q.UseMicrosoftDependencyInjectionJobFactory();
                    q.UseSimpleTypeLoader();
                    q.UseInMemoryStore();
                    q.UseDefaultThreadPool(tp =>
                    {
                        tp.MaxConcurrency = 10;
                    });
                    q.ScheduleJob<DataBackupService>(trigger => trigger
                        .WithIdentity(developmentBackupIdentity)
                        .UsingJobData("backupTarget", developmentBackupTarget)
                        .WithCronSchedule(developmentBackupSchedule)
                    );
                    q.ScheduleJob<DataBackupService>(trigger => trigger
                        .WithIdentity(productionBackupIdentity)
                        .UsingJobData("backupTarget", productionBackupTarget)
                        .WithCronSchedule(productionBackupSchedule)
                    );
                });
            }
            else if (developmentBackupAllowed && !productionBackupAllowed)
            {
                services.AddQuartz(q =>
                {
                    q.UseMicrosoftDependencyInjectionJobFactory();
                    q.UseSimpleTypeLoader();
                    q.UseInMemoryStore();
                    q.UseDefaultThreadPool(tp =>
                    {
                        tp.MaxConcurrency = 10;
                    });
                    q.ScheduleJob<DataBackupService>(trigger => trigger
                        .WithIdentity(developmentBackupIdentity)
                        .UsingJobData("backupTarget", developmentBackupTarget)
                        .WithCronSchedule(developmentBackupSchedule)
                    );
                });
            }
            else if (!developmentBackupAllowed && productionBackupAllowed)
            {
                services.AddQuartz(q =>
                {
                    q.UseMicrosoftDependencyInjectionJobFactory();
                    q.UseSimpleTypeLoader();
                    q.UseInMemoryStore();
                    q.UseDefaultThreadPool(tp =>
                    {
                        tp.MaxConcurrency = 10;
                    });
                    q.ScheduleJob<DataBackupService>(trigger => trigger
                        .WithIdentity(productionBackupIdentity)
                        .UsingJobData("backupTarget", productionBackupTarget)
                        .WithCronSchedule(productionBackupSchedule)
                    );
                });
            }

            if (developmentBackupAllowed || productionBackupAllowed)
            {
                services.AddQuartzHostedService(qs => qs.WaitForJobsToComplete = true);
            }
            return services;
        }
    }
}