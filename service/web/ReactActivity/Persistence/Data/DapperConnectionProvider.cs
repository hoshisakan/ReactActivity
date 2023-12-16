using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Npgsql;


namespace Persistence.Data
{
    public class DapperConnectionProvider : IDapperConnectionProvider
    {
        private readonly ILogger<DapperConnectionProvider> _logger;
        private string? _connectionString;
        private IDbConnection? _connection;
        private bool _disposed;
        private readonly IConfiguration _config;


        // public DapperConnectionProvider(ILogger<DapperConnectionProvider> logger, string conectionString)
        // {
        //     _logger = logger;
        //     _connectionString = conectionString;

        //     if (string.IsNullOrEmpty(_connectionString))
        //     {
        //         throw new ArgumentNullException("Connection string is null or empty");
        //     }
        // }

        public DapperConnectionProvider(ILogger<DapperConnectionProvider> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    /// If use _connection.State, then it will get connection state from connection pool
                    _connection?.Dispose();
                    _logger.LogDebug("Connection is disposed.");

                    _connection?.Close();
                    _logger.LogDebug("Connection is closed.");

                    _connection = null;
                    _logger.LogDebug("Connection object is set null.");
                }
            }
            _disposed = true;
        }

        public IDbConnection Connect()
        {
            _logger.LogDebug("Connection is created.");

            _connectionString = _config.GetSection("DapperRelatedSettings:SelfNpsqlConnection").Get<string>() ?? string.Empty;

            _logger.LogDebug($"Connection string: {_connectionString}");

            if (string.IsNullOrEmpty(_connectionString))
            {
                throw new ArgumentNullException("Connection string is null or empty");
            }

            _connection = new NpgsqlConnection(_connectionString);

            return _connection;
        }
    }
}