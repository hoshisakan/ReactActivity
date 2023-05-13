using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;


namespace Infrastructure.StaticFilePathInitializer
{
    public class StaticFilePathInitializer : IStaticFilePathInitializer
    {
        private readonly IConfiguration _config;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<StaticFilePathInitializer> _logger;

        public StaticFilePathInitializer(
            IConfiguration config, IWebHostEnvironment env, ILogger<StaticFilePathInitializer> logger
        )
        {
            _config = config;
            _env = env;
            _logger = logger;
        }

        public void StoragePathInitializer()
        {
            string storagePath = _config.GetSection("StaticFiles:StoragePath").Get<string>() ?? string.Empty;

            if (string.IsNullOrEmpty(storagePath))
            {
                throw new Exception("Storage path is not set.");
            }

            _logger.LogInformation($"Static file storage path: {storagePath}");

            if (!Directory.Exists(storagePath))
            {
                _logger.LogInformation("Static file storage path does not exist. Creating...");
                Directory.CreateDirectory(storagePath);
                _logger.LogInformation("Static file storage path created.");
            }
            else
            {
                _logger.LogInformation("Static file storage path already exists.");
            }
        }
    }
}