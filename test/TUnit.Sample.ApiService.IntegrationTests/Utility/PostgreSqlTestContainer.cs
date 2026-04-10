using Testcontainers.PostgreSql;
using TUnit.Core.Interfaces;

namespace TUnit.Sample.ApiService.IntegrationTests.Utility;

public sealed class PostgreSqlTestContainer : IAsyncInitializer, IAsyncDisposable
{
    public PostgreSqlContainer Container { get; } = new PostgreSqlBuilder("postgres:17-alpine")
        .Build();

    public async Task InitializeAsync() => await Container.StartAsync();
    public async ValueTask DisposeAsync() => await Container.DisposeAsync();
}