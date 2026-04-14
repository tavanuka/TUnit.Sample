using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using TUnit.AspNetCore;
using TUnit.Core.Helpers;
using TUnit.Sample.Infrastructure;
using TUnit.Sample.Infrastructure.Data.Factories;

namespace TUnit.Sample.ApiService.IntegrationTests.Utility;

[ParallelLimiter<ProcessorCountParallelLimit>]
public abstract class CoreIntegrationTestBase : WebApplicationTest<WebApplicationFactory, Program>
{
    [ClassDataSource<PostgreSqlTestContainer>(Shared = SharedType.PerTestSession)]
    public PostgreSqlTestContainer PostgreSqlTestContainer { get; init; } = null!;

    protected string SchemaName { get; private set; } = null!;

    protected override async Task SetupAsync()
    {
        SchemaName = GetIsolatedName("schema");
        var connectionString = PostgreSqlTestContainer.Container.GetConnectionString();

        // Create the schema via raw SQL
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();
        await using var cmd = connection.CreateCommand();
        //language=PostgreSQL
        cmd.CommandText = $"""
                           CREATE SCHEMA IF NOT EXISTS "{SchemaName}"
                           """;
        await cmd.ExecuteNonQueryAsync();

        // Use EF Core to create tables in the new schema
        var options = new DbContextOptionsBuilder<CoreDbContext>()
            .UseNpgsql(connectionString)
            .ReplaceService<IModelCacheKeyFactory, SchemaModelCacheKeyFactory>()
            .Options;

        await using var dbContext = new CoreDbContext(options) { SchemaName = SchemaName };
        var dbCreator = (RelationalDatabaseCreator)dbContext.Database.GetService<IDatabaseCreator>();
        await dbCreator.CreateTablesAsync();
    }

    protected override void ConfigureTestConfiguration(IConfigurationBuilder config)
    {
        config.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Database:Schema"] = SchemaName
        });
    }

    [After(Test)]
    public async Task CleanupSchema()
    {
        if (string.IsNullOrEmpty(SchemaName))
            return;

        await using var connection = new NpgsqlConnection(PostgreSqlTestContainer.Container.GetConnectionString());
        await connection.OpenAsync();

        await using var cmd = connection.CreateCommand();
        // language=PostgreSQL
        cmd.CommandText = $"""DROP SCHEMA IF EXISTS "{SchemaName}" CASCADE""";
        await cmd.ExecuteNonQueryAsync();
    }

    protected AsyncServiceScope CreateDbScope(out CoreDbContext dbContext)
    {
        var scope = Factory.Services.CreateAsyncScope();
        dbContext = scope.ServiceProvider.GetRequiredService<CoreDbContext>();
        return scope;
    }

    protected async Task PopulateSchemaWithData()
    {
        await using var scope = CreateDbScope(out var dbContext);
        await CoreDbContext.SeedDomainObjects(dbContext, CancellationToken.None);
        await dbContext.SaveChangesAsync();
    }
}