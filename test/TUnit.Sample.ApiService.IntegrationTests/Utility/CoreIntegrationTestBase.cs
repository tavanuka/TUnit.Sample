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
    // Saving the database scope here to be able to dispose it after each test.
    // This is convenient because we don't have to deal with getting a DbContext in each test manually.
    private IServiceScope? _scope;
    
    [ClassDataSource<PostgreSqlTestContainer>(Shared = SharedType.PerTestSession)]
    public PostgreSqlTestContainer PostgreSqlTestContainer { get; init; } = null!;

    protected string SchemaName { get; private set; } = null!;

    protected CoreDbContext DbContext { get; private set; } = null!;
    
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

    // Hint: If you want to re-use the before logic, you can make another method and assign another BeforeAttribute hook.
    // Due to nature of source generation, base class hooks will be executed first, and onwards (bottoms-up).
    // Additionally, the Order property can also be assigned to dictate which hook gets triggered
    [Before(Test)]
    public Task SetupDatabaseContextBeforeTest()
    {
        try
        {
            Console.WriteLine("Before Test 1");
            // Because this happens right between lifecycle step 8 and 9, we can safely create the context. 
            // https://tunit.dev/docs/examples/aspnet#lifecycle-order
            _scope = Factory.Services.CreateScope();
            DbContext = _scope.ServiceProvider.GetRequiredService<CoreDbContext>();
            return Task.CompletedTask;
        }
        catch (Exception exception)
        {
            return Task.FromException(exception);
        }
    }

    [After(Test)]
    public async Task CleanupSchema()
    {
        if (_scope is IAsyncDisposable asyncScope)
            await asyncScope.DisposeAsync();
        else
            _scope?.Dispose();

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