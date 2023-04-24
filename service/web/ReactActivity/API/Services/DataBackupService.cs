using Application.Module;

using System.Diagnostics;
using Quartz;

namespace API.Services
{
    public class DataBackupService : IJob
    {
        private readonly ILogger<DataBackupService>? _logger;

        public DataBackupService(ILogger<DataBackupService> logger)
        {
            _logger = logger;
        }

        public Task Execute(IJobExecutionContext context)
        {
            _logger?.LogInformation("Backup job is running.");
            try
            {
                string id = Guid.NewGuid().ToString();
                JobKey key = context.JobDetail.Key;
                JobDataMap dataMap = context.Trigger.JobDataMap;

                string? scheduleUTCExecuteTime = context.ScheduledFireTimeUtc.ToString() ?? DateTime.UtcNow.ToString();
                string? scheduleLocalExecuteFormatTime = DateTime.Parse(scheduleUTCExecuteTime).ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss");
                // string? backupFileDateTime = DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss");
                string? backupFileTime = DateTime.Now.ToString("HH_mm_ss");
                string backupFileDate = DateTime.Now.ToString("yyyy_MM_dd");

                //TODO Read variables from job data map.
                string? backupDbName = dataMap.GetString("backupTarget");

                //TODO Read variables from environment variables.
                string? backupStoragePath = Environment.GetEnvironmentVariable("POSTGRES_DATA_BACKUP_PATH");
                string? postgresUser = Environment.GetEnvironmentVariable("DOTNET_POSTGRES_USER");
                string? postgresHost = Environment.GetEnvironmentVariable("DOTNET_POSTGRES_HOST_IP");
                string? postgresPort = Environment.GetEnvironmentVariable("DOTNET_POSTGRES_PORT");

                if (string.IsNullOrEmpty(backupStoragePath) || string.IsNullOrEmpty(postgresUser) || string.IsNullOrEmpty(postgresHost) || string.IsNullOrEmpty(postgresPort))
                {
                    throw new Exception($"Backup job failed. Environment variables are not set at {scheduleLocalExecuteFormatTime}.");
                }

                if (string.IsNullOrEmpty(backupDbName) || string.IsNullOrEmpty(backupStoragePath))
                {
                    throw new Exception($"backupDbName or backupStoragePath is null that event occurred at {scheduleLocalExecuteFormatTime}.");
                }

                //TODO Set backup file name and path and command.
                string backupFileName = $"{backupFileTime}_{backupDbName}_db_backup.dump";
                string backupDirectory = $"{backupStoragePath}/{backupDbName}/{backupFileDate}";
                string backupFullPath = Path.Combine(backupDirectory, backupFileName);
                string backupCommand = $" -U {postgresUser} -h {postgresHost} -p {postgresPort} -d {backupDbName} -f {backupFullPath}";

                //TODO Create backup directory if not exists.
                if (!FileTool.CheckDirExists(backupDirectory))
                {
                    FileTool.CreateDirectory(backupDirectory);
                }

                Process? process = new Process();
                ProcessStartInfo? startInfo = new ProcessStartInfo();
                startInfo.FileName = "pg_dump";
                startInfo.Arguments = backupCommand;
                startInfo.UseShellExecute = false;
                startInfo.RedirectStandardOutput = true;
                startInfo.RedirectStandardError = true;
                startInfo.CreateNoWindow = true;
                process.StartInfo = startInfo;
                process.Start();
                process.WaitForExit();
                process.Close();

                //TODO Check backup whether success or not by checking file exists.
                if (FileTool.CheckFileExists(backupFullPath))
                {
                    _logger?.LogInformation($"Backup job {id} - {backupDbName} - {backupFullPath} successfully at {scheduleLocalExecuteFormatTime}.");
                }
                else
                {
                    _logger?.LogError($"Backup job {id} - {backupDbName} execute command pg_dump {backupCommand} failed at {scheduleLocalExecuteFormatTime}.");
                    throw new Exception($"Backup job {id} - {backupDbName} - {backupFullPath} failed at {scheduleLocalExecuteFormatTime}.");
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex.Message);
            }
            _logger?.LogInformation("Backup job is finish.");
            return Task.CompletedTask;
        }
    }
}