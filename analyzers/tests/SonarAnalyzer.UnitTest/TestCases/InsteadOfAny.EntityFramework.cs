using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

// https://github.com/SonarSource/sonar-dotnet/issues/7286
public class EntityFrameworkReproGH7286
{
    public class MyEntity
    {
        public int Id { get; set; }
    }

    public class MyDbContext : DbContext
    {
        public DbSet<MyEntity> MyEntities { get; set; }
    }

    public void GetEntities(MyDbContext dbContext, List<int> ids)
    {
        _ = dbContext.MyEntities.Where(e => ids.Any(i => e.Id == i)); // Noncompliant - FP, should raise in context of EntityFramework queries. Exist cannot be translated to SQL query
        _ = dbContext.MyEntities.Where(e => ids.Any(i => e.Id is i)); // Error [CS0150]
        // Noncompliant@+1
        _ = dbContext.MyEntities.Where(e => ids.Any(i => e.Id is 2)); // Error [CS8122]
        _ = dbContext.MyEntities.Where(e => ids.Any(i => e.Equals(i))); // Noncompliant - FP, should raise in context of EntityFramework queries. Exist cannot be translated to SQL query
        // This will generate a runtime error as EF does not know how to translate it to SQL Query
        _ = dbContext.MyEntities.Where(e => ids.Any(i => e.Id > i)); // Noncompliant - FP
    }
}
