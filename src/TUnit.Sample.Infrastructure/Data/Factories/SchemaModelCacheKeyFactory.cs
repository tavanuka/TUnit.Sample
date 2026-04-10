using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace TUnit.Sample.Infrastructure.Data.Factories;

public class SchemaModelCacheKeyFactory : IModelCacheKeyFactory
{
    public object Create(DbContext context, bool designTime) => context is CoreDbContext dbContext
        ? (context.GetType(), dbContext.SchemaName, designTime)
        : (context.GetType(), designTime);
}