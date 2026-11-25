using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Iskur.Api.Migrations;

namespace Iskur.Api;

public static class DatabaseUpgrade
{
    public static async Task Run(AppDb db)
    {
        if (await db.Database.CanConnectAsync())
            await AdoptVizeSchema(db);
        await db.Database.MigrateAsync();
    }

    static async Task AdoptVizeSchema(AppDb db)
    {
        var connection = db.Database.GetDbConnection();
        await db.Database.OpenConnectionAsync();
        try
        {
            await using var check = connection.CreateCommand();
            check.CommandText = """
                SELECT CASE
                    WHEN OBJECT_ID(N'dbo.__EFMigrationsHistory', N'U') IS NOT NULL
                     OR NOT EXISTS (SELECT 1 FROM sys.tables WHERE is_ms_shipped = 0) THEN 0
                    WHEN OBJECT_ID(N'dbo.Plans', N'U') IS NOT NULL
                     AND COL_LENGTH('dbo.Plans', 'Version') = 8
                     AND OBJECT_ID(N'dbo.Students', N'U') IS NOT NULL
                     AND COL_LENGTH('dbo.Students', 'NationalId') = 22 THEN 1
                    ELSE -1 END;
                """;
            var kind = (int)(await check.ExecuteScalarAsync())!;
            if (kind == 0) return;
            if (kind < 0)
                throw new InvalidOperationException("Bu veritabanı vize paketinin şemasıyla eşleşmiyor.");

            var baseline = new VizeBaseline();
            var tables = baseline.UpOperations.OfType<Microsoft.EntityFrameworkCore.Migrations.Operations.CreateTableOperation>()
                .ToArray();
            var actual = new Dictionary<(string, string), HashSet<string>>();
            await using (var metadata = connection.CreateCommand())
            {
                metadata.CommandText = """
                    SELECT s.name, t.name, c.name FROM sys.tables AS t
                    JOIN sys.schemas AS s ON s.schema_id = t.schema_id
                    JOIN sys.columns AS c ON c.object_id = t.object_id;
                    """;
                await using var reader = await metadata.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var key = (reader.GetString(0), reader.GetString(1));
                    if (!actual.TryGetValue(key, out var columns))
                        actual.Add(key, columns = []);
                    columns.Add(reader.GetString(2));
                }
            }

            if (tables.Any(t => !actual.TryGetValue((t.Schema ?? "dbo", t.Name), out var columns) ||
                !columns.SetEquals(t.Columns.Select(c => c.Name))))
                throw new InvalidOperationException("Vize şemasındaki tablo veya sütunlar değişmiş. Yükseltme durduruldu.");

            var id = db.GetService<IMigrationsAssembly>().FindMigrationId(nameof(VizeBaseline))
                ?? throw new InvalidOperationException("Başlangıç migration kaydı bulunamadı.");
            await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
            await db.Database.ExecuteSqlInterpolatedAsync($"""
                DECLARE @result int;
                EXEC @result = sys.sp_getapplock @Resource = 'Iskur.VizeBaseline',
                    @LockMode = 'Exclusive', @LockOwner = 'Transaction', @LockTimeout = 10000;
                IF @result < 0 THROW 51000, N'Veritabanı şeması başka bir işlem tarafından güncelleniyor.', 1;
                IF OBJECT_ID(N'dbo.__EFMigrationsHistory', N'U') IS NULL
                BEGIN
                    CREATE TABLE dbo.__EFMigrationsHistory (
                        MigrationId nvarchar(150) NOT NULL PRIMARY KEY,
                        ProductVersion nvarchar(32) NOT NULL
                    );
                    INSERT INTO dbo.__EFMigrationsHistory (MigrationId, ProductVersion) VALUES ({id}, N'10.0.14');
                END;
                """);
            await transaction.CommitAsync();
        }
        finally
        {
            await db.Database.CloseConnectionAsync();
        }
    }
}
