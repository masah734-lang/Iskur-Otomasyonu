using System.Data.Common;
using Microsoft.EntityFrameworkCore;

namespace Iskur.Api;

public static class Backup
{
    public static async Task<string> Create(AppDb db, string directory)
    {
        if (!Path.IsPathFullyQualified(directory))
            throw new ArgumentException("SQL Server tarafından erişilebilen mutlak bir yedek dizini belirtin.");

        var connection = db.Database.GetDbConnection();
        await db.Database.OpenConnectionAsync();
        try
        {
            var database = connection.Database;
            var safeFile = string.Concat(database.Select(c => char.IsLetterOrDigit(c) || c is '_' or '-' ? c : '_'));
            var name = $"{safeFile}_{DateTimeOffset.UtcNow:yyyyMMdd_HHmmss_fff}_{Guid.NewGuid():N}.bak";
            var destination = Path.Combine(directory, name);
            var identifier = "[" + database.Replace("]", "]]") + "]";
            await Execute(connection, $"BACKUP DATABASE {identifier} TO DISK = @file WITH COPY_ONLY, CHECKSUM;", destination);
            await Execute(connection, "RESTORE VERIFYONLY FROM DISK = @file WITH CHECKSUM;", destination);
            return destination;
        }
        finally
        {
            await db.Database.CloseConnectionAsync();
        }
    }

    static async Task Execute(DbConnection connection, string sql, string destination)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.CommandTimeout = 600;
        var file = command.CreateParameter();
        file.ParameterName = "@file";
        file.Value = destination;
        command.Parameters.Add(file);
        await command.ExecuteNonQueryAsync();
    }
}
