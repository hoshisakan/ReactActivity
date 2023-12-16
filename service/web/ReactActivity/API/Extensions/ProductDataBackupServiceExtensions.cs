using API.Services;
using Quartz;

namespace API.Extensions
{
    public static class ProductDataBackupServiceExtensions
    {
        public static IServiceCollection AddProductDataBackupServices(this IServiceCollection services,
            IConfiguration config)
        {
            bool productionBackupAllowed = config.GetSection("DataBackupSettings:Production:Allowed").Get<bool>();
            string productionBackupIdentity = string.Empty;
            string productionBackupTarget = string.Empty;
            string[] productionBackupScheduleList = new string[] {};
            string productionBackupSchedule = string.Empty;

            string currentDayOfWeek = DateTime.Today.DayOfWeek.ToString();

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
                    // productionBackupSchedule = productionBackupScheduleList[0];
                    productionBackupSchedule = config.GetSection(
                        "DataBackupSettings:Production:CronSchedule:WeekendsCronExpression"
                    ).Get<string>() ?? string.Empty;
                }
                else
                {
                    // productionBackupSchedule = productionBackupScheduleList[1];
                    productionBackupSchedule = config.GetSection(
                        "DataBackupSettings:Production:CronSchedule:WeekdaysCronExpression"
                    ).Get<string>() ?? string.Empty;
                }

                if (string.IsNullOrEmpty(productionBackupSchedule))
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
                        .WithIdentity(productionBackupIdentity)
                        .UsingJobData("backupTarget", productionBackupTarget)
                        .WithCronSchedule(productionBackupSchedule)
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