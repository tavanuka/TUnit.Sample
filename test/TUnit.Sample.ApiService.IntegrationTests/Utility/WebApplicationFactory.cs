using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Diagnostics.CodeAnalysis;
using TUnit.AspNetCore;
using TUnit.Sample.Common.Constants;
using TUnit.Sample.Infrastructure;
using TUnit.Sample.Infrastructure.Data.Factories;

namespace TUnit.Sample.ApiService.IntegrationTests.Utility;

[SuppressMessage("Usage", "TUnit0043:Property must use `required` keyword")]
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