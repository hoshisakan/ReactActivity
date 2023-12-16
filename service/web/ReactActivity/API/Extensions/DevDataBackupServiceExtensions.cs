using API.Services;
using Quartz;

namespace API.Extensions
{
    public static class DataBackupServiceExtensions
    {
        public static IServiceCollection AddDevDataBackupServices(this IServiceCollection services,
            IConfiguration config)
        {
            bool developmentBackupAllowed = config.GetSection("DataBackupSettings:Development:Allowed").Get<bool>();
            string developmentBackupIdentity = string.Empty;
            string developmentBackupTarget = string.Empty;
            string[] developmentBackupScheduleList = new string[] {};
            string developmentBackupSchedule = string.Empty;
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
                    // developmentBackupSchedule = developmentBackupScheduleList[0];
                    developmentBackupSchedule = config.GetSection(
                        "DataBackupSettings:Development:CronSchedule:WeekendsCronExpression"
                    ).Get<string>() ?? string.Empty;
                }
                else
                {
                    // developmentBackupSchedule = developmentBackupScheduleList[1];
                    developmentBackupSchedule = config.GetSection(
                        "DataBackupSettings:Development:CronSchedule:WeekdaysCronExpression"
                    ).Get<string>() ?? string.Empty;
                }

                if (string.IsNullOrEmpty(developmentBackupSchedule))
                {
                    throw new Exception("Data backup settings are not set.");
                }

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
                services.AddQuartzHostedService(qs => qs.WaitForJobsToComplete = true);
            }
            else
            {
                throw new Exception("Data backup not allowed.");
            }
            return services;
        }
    }
}