using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

// https://github.com/SonarSource/sonar-dotnet/issues/7286
public class EntityFrameworkReproGH7286
{
    public class MyEntity
    {
        public int Id { get; set; }
    }

    public class FirstContext: DbContext
    {
        public DbSet<MyEntity> MyEntities { get; set; }
    }

    public class SecondContext: DbContext
    {
        public DbSet<MyEntity> SecondEntities { get; set; }
    }


    public void GetEntities(FirstContext dbContext, List<int> ids)
    {
        _ = dbContext.MyEntities.Where(e => ids.Any(i => e.Id == i)); // Compliant
        _ = dbContext.MyEntities.Where(e => ids.Any(i => e.Equals(i))); // Compliant
        _ = dbContext.MyEntities.Where(e => ids.Any(i => e.Id > i)); // Compliant

        _ = dbContext.MyEntities.Where(e => ids.Any(i => e.Id is i)); // Error [CS9135]
        _ = dbContext.MyEntities.Where(e => ids.Any(i => e.Id is 2)); // Error [CS8122]

        var iqueryable = dbContext.MyEntities.OrderBy(e => e.Id);
        _ = iqueryable.Where(e => ids.Any(i => e.Id == i)); // Compliant
    }

    public async Task GetEntitiesAsync(SecondContext secondContext, List<int> ids)
    {
        _ = await secondContext
                .SecondEntities
                .Where(e => ids.Any(i => e.Id == i))
                .ToListAsync();
    }
}
