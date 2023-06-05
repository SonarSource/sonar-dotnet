using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Threading.Tasks;
using System.Data.Entity;

// https://github.com/SonarSource/sonar-dotnet/issues/7286
public class EntityFrameworkReproGH7286
{
    public class MyEntity
    {
        public int Id { get; set; }
    }

    public class FirstContext : DbContext
    {
        public DbSet<MyEntity> MyEntities { get; set; }
    }

    public class SecondContext : DbContext
    {
        public DbSet<MyEntity> SecondEntities { get; set; }
    }


    public void GetEntities(FirstContext dbContext, List<int> ids)
    {
        _ = dbContext.MyEntities.Where(e => ids.Any(i => e.Id == i)); // Compliant
        _ = dbContext.MyEntities.Where(e => ids.Any(i => e.Equals(i))); // Compliant
        _ = dbContext.MyEntities.Where(e => ids.Any(i => e.Id > i)); // Compliant

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
