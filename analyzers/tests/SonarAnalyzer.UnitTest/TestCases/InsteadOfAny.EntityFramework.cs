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
        _ = dbContext.MyEntities.Where(e => ids.Any(i => e.Id == i)); // Compliant
        _ = dbContext.MyEntities.Where(e => ids.Any(i => e.Equals(i))); // Compliant
        _ = dbContext.MyEntities.Where(e => ids.Any(i => e.Id > i)); // Compliant

        _ = dbContext.MyEntities.Where(e => ids.Any(i => e.Id is i)); // Error [CS0150]
        _ = dbContext.MyEntities.Where(e => ids.Any(i => e.Id is 2)); // Error [CS8122]
    }
}
