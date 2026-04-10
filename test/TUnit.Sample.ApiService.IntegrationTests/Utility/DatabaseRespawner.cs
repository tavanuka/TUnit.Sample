using Npgsql;
using Respawn;

namespace TUnit.Sample.ApiService.IntegrationTests.Utility;

public sealed class DatabaseRespawner
{
    [ClassDataSource<PostgreSqlTestContainer>(Shared = SharedType.PerTestSession)]
    public PostgreSqlTestContainer PostgreSqlTestContainer { get; init; } = null!;

    private Respawner? _respawner;

    public async Task ResetAsync()
    {
        await using var conn = new NpgsqlConnection(PostgreSqlTestContainer.Container.GetConnectionString());
        await conn.OpenAsync();

        _respawner ??= await Respawner.CreateAsync(conn);

        await _respawner.ResetAsync(conn);
    }
}