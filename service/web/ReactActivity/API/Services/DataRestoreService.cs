using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using Application.Module;
using Persistence.Data;
using Dapper;


namespace API.Services
{
    public class DataRestoreService : IDataRestoreService
    {
        public readonly ILogger<DataRestoreService>? _logger;
        private readonly IConfiguration _config;
        private readonly IDapperConnectionProvider _dapperConnectionProvider;
        

        public DataRestoreService(ILogger<DataRestoreService> logger, IConfiguration config, IDapperConnectionProvider dapperConnectionProvider)
        {
            _logger = logger;
            _config = config;
            _dapperConnectionProvider = dapperConnectionProvider;
        }

        private string GetBackupFileFullPath(string backupFileRootPath, string backupDbName)
        {
            string? lastBackupFileFullPath = string.Empty;
            string? backupFullPath = $"{backupFileRootPath}/{backupDbName}";

            _logger?.LogInformation($"Backup full path: {backupFullPath}");

            DirectoryInfo? backupDirectoryInfo = new DirectoryInfo(backupFullPath) ?? null;

            if (backupDirectoryInfo == null)
            {
                throw new Exception($"Can not read backup database directory.");
            }

            int directoryCount = backupDirectoryInfo.GetDirectories().Count();

            _logger?.LogInformation($"Backup directory count: {directoryCount}");

            DirectoryInfo? lastBackupDirectory = backupDirectoryInfo.GetDirectories().OrderByDescending(d => d.LastWriteTime).FirstOrDefault() ?? null;

            if (lastBackupDirectory == null)
            {
                throw new Exception($"Can not read latest directory info of backup database directory.");
            }

            _logger?.LogInformation($"Last backup directory full path: {lastBackupDirectory.FullName}");

            string lastBackupDirectoryFullPath = lastBackupDirectory.FullName;

            lastBackupFileFullPath = lastBackupDirectory.GetFiles().OrderByDescending(f => f.LastWriteTime).FirstOrDefault()?.FullName ?? string.Empty;

            _logger?.LogInformation($"Last backup directory full path: {lastBackupFileFullPath}");

            // lastBackupFileFullPath = lastBackupDirectory.GetFiles().OrderByDescending(f => f.LastWriteTime).FirstOrDefault()?.FullName ?? string.Empty;

            // if (string.IsNullOrEmpty(lastBackupFileFullPath))
            // {
            //     throw new Exception($"Can not read latest file info of backup database directory.");
            // }

            // _logger?.LogInformation($"Last backup file full path: {lastBackupFileFullPath}");

            return lastBackupFileFullPath;
        }

        private bool IsDatabaseExists(string DbName)
        {
            _logger?.LogInformation($"Database name: {DbName}");

            try
            {
                DynamicParameters parameters = new DynamicParameters();

                parameters.Add("@DbName", DbName);
                string query = "SELECT 1 FROM pg_database WHERE datname=@DbName";
                int execResult = _dapperConnectionProvider.Connect().QueryFirst<int>(query, parameters);

                _logger?.LogInformation($"Database {DbName} is exists: {execResult}");

                return execResult == 1;
            } catch (Exception ex)
            {
                _logger?.LogError(ex, ex.Message);
                return false;
            }
        }

        private bool CreateDatabase(string databaseName)
        {
            bool isDatabaseExists = IsDatabaseExists(databaseName);

            _logger?.LogInformation($"Create database {databaseName} isDatabaseExists: {isDatabaseExists}");

            if (isDatabaseExists)
            {
                return false;
            }
            _logger?.LogInformation($"Create database {databaseName}");

            _dapperConnectionProvider.Connect().Execute($"CREATE DATABASE {databaseName}");

            isDatabaseExists = IsDatabaseExists(databaseName);

            _logger?.LogInformation($"Create database {databaseName} isDatabaseExists: {isDatabaseExists}");

            return isDatabaseExists;
        }

        public async Task RestoreData(bool isDevelopment)
        {
            try
            {
                _logger?.LogInformation("Restore job start.");

                string? postgresUser = Environment.GetEnvironmentVariable("DOTNET_POSTGRES_USER");
                string? postgresHost = Environment.GetEnvironmentVariable("DOTNET_POSTGRES_HOST_IP");
                string? postgresPort = Environment.GetEnvironmentVariable("DOTNET_POSTGRES_PORT");

                if (string.IsNullOrEmpty(postgresUser) || string.IsNullOrEmpty(postgresHost) || string.IsNullOrEmpty(postgresPort))
                {
                    throw new Exception($"Restore job failed. Environment variables are not set.");
                }

                string? backupDbName = string.Empty;

                if (isDevelopment)
                {
                    _logger?.LogInformation($"Restore job start. Development environment");
                    backupDbName = _config.GetSection("DataBackupSettings:Development:Target").Get<string>() ?? string.Empty;
                }
                else
                {
                    _logger?.LogInformation($"Restore job start. Production environment");
                    backupDbName = _config.GetSection("DataBackupSettings:Production:Target").Get<string>() ?? string.Empty;
                }
                
                string? backupFileFullPath = Environment.GetEnvironmentVariable("POSTGRES_DATA_BACKUP_PATH");

                if (string.IsNullOrEmpty(backupFileFullPath) || string.IsNullOrEmpty(backupDbName))
                {
                    throw new Exception($"backupFileFullPath or backupDbName is null.");
                }

                bool isCreated = CreateDatabase(backupDbName);

                if (!isCreated)
                {
                    throw new Exception($"Restore job failed. Database not created.");
                }

                //TODO Set backup file name and path and command.
                string restoreTargetFile = GetBackupFileFullPath(backupFileFullPath, backupDbName);

                _logger?.LogInformation($"Restore target file: {restoreTargetFile}");

                if (!FileTool.CheckFileExists(restoreTargetFile))
                {
                    throw new Exception($"Restore job failed. Backup file not found.");
                }

                _logger?.LogInformation($"Restore job start. {backupFileFullPath} - {backupDbName}");

                string restoreCommand = $" -U {postgresUser} -h {postgresHost} -p {postgresPort} -d {backupDbName} -f {restoreTargetFile}";

                _logger?.LogInformation($"Restore command: {restoreCommand}");

                await Task.Run(() =>
                {
                    ProcessStartInfo processStartInfo = new()
                    {
                        FileName = "psql",
                        Arguments = restoreCommand,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    };

                    Process process = new()
                    {
                        StartInfo = processStartInfo
                    };
                    process.Start();

                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();

                    process.WaitForExit();

                    if (process.ExitCode != 0)
                    {
                        throw new Exception($"Restore job failed. {error}");
                    }
                });

                _logger?.LogInformation("Restore job finish.");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, ex.Message);
                throw;
            }
        }
    }
}