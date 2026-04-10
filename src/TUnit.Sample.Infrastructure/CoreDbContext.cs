using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TUnit.Sample.Domain;
using TUnit.Sample.Infrastructure.Data.Seed;

namespace TUnit.Sample.Infrastructure;

public class CoreDbContext : DbContext
{
    public string SchemaName { get; set; }
    
    public DbSet<Person> Persons { get; set; }
    public DbSet<Book> Books { get; set; }

    public CoreDbContext(DbContextOptions<CoreDbContext> options, IConfiguration? configuration = null)
        : base(options)
    {
        SchemaName = configuration?["Database:Schema"] ?? "public";
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(SchemaName);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CoreDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }

    public static async Task SeedDomainObjects(DbContext context, CancellationToken cancellationToken)
        => await context.Database.CreateExecutionStrategy().ExecuteAsync(context,
            static async (ctx, ct) => {
                await PersonSeeder.SeedAsync(ctx, ct);
                await BookSeeder.SeedAsync(ctx, ct);
            },
            cancellationToken);
}