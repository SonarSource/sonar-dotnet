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

    public List<MyEntity> GetEntities(MyDbContext dbContext, List<int> ids) =>
        dbContext
            .MyEntities
            .Where(e => ids.Any(i => e.Id == i)) // Noncompliant - FP, should not raise in context of EntityFramework queries. Exist cannot be translated to SQL query
            .ToList();
}
