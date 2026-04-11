using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TUnit.AspNetCore;
using TUnit.Sample.ApiService.Workers;
using TUnit.Sample.Common.Constants;
using TUnit.Sample.Infrastructure;
using TUnit.Sample.Infrastructure.Data.Factories;

namespace TUnit.Sample.ApiService.IntegrationTests.Utility;

public class WebApplicationFactory : TestWebApplicationFactory<Program>
{
    [ClassDataSource<PostgreSqlTestContainer>(Shared = SharedType.PerTestSession)]
    public PostgreSqlTestContainer PostgreSqlTestContainer { get; init; } = null!;
    
    protected override void ConfigureStartupConfiguration(IConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
        {
            [$"ConnectionStrings:{ResourceConstants.CoreDb}"] = PostgreSqlTestContainer.Container.GetConnectionString()
        });
    }
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(s => {
            // with this we remove premature database population to ensure that the test starts with a clean database.
            s.RemoveAll<DatabaseWorker>();
            s.RemoveAll<CoreDbContext>();
            s.RemoveAll<DbContextOptions<CoreDbContext>>();
            s.RemoveAll<IDbContextFactory<CoreDbContext>>();

 #pragma warning disable EF1001
            s.RemoveAll<IDbContextPool<CoreDbContext>>();
            s.RemoveAll<IScopedDbContextLease<CoreDbContext>>();
 #pragma warning restore EF1001

            s.AddDbContext<CoreDbContext>(options =>
                options.UseNpgsql(PostgreSqlTestContainer.Container.GetConnectionString())
                    .ReplaceService<IModelCacheKeyFactory, SchemaModelCacheKeyFactory>()
            );
        });
    }
}