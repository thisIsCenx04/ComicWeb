using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace ComicWeb.Infrastructure.Data;

public sealed class DatabaseBootstrapper
{
    private readonly string _connectionString;
    private readonly ILogger<DatabaseBootstrapper> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseBootstrapper"/> class.
    /// </summary>
    public DatabaseBootstrapper(IConfiguration configuration, ILogger<DatabaseBootstrapper> logger)
    {
        _connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("Missing Default connection string.");
        _logger = logger;
    }

    /// <summary>
    /// Ensures the database schema is created by executing the given SQL file.
    /// </summary>
    public async Task EnsureSchemaAsync(string sqlPath)
    {
        if (!File.Exists(sqlPath))
        {
            _logger.LogWarning("Schema file not found at {Path}", sqlPath);
            return;
        }

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var checkCmd = new NpgsqlCommand("SELECT to_regclass('public.users')::text", connection);
        var exists = await checkCmd.ExecuteScalarAsync();
        if (exists != DBNull.Value && exists != null)
        {
            return;
        }

        var sql = await File.ReadAllTextAsync(sqlPath);
        await using var cmd = new NpgsqlCommand(sql, connection);
        await cmd.ExecuteNonQueryAsync();
    }
}
