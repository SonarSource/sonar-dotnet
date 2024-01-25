using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using Microsoft.EntityFrameworkCore;

List<object> list = null;

list.Select(static (element, col) => element is Int32 and > 21).Any(element => element != null); // Compliant
list.Select((object _, int _) => 1).Any(element => element != null); // Compliant

list.Select((element, col) => element as object).Any(element => element != null);  //Noncompliant {{Use 'OfType<object>()' here instead.}}


// https://github.com/SonarSource/sonar-dotnet/issues/3604
public class EntityFrameworkReproGH3604
{
    public class MyEntity
    {
        public int Id { get; set; }
    }

    public class FirstContext : DbContext
    {
        public DbSet<MyEntity> MyEntities { get; set; }
    }

    public void GetEntities(FirstContext dbContext)
    {
        _ = dbContext.MyEntities.OrderBy(v => v.Id).ToList().Where(SomeTest).ToList(); // Noncompliant {{Use 'AsEnumerable' here instead.}}
        //                                          ^^^^^^
        _ = dbContext.MyEntities.ToList().Where(SomeTest).ToList(); // Noncompliant {{Use 'AsEnumerable' here instead.}}
        //                       ^^^^^^
        _ = (from v in dbContext.MyEntities
             orderby v.Id
             select v).ToList().Where(SomeTest).ToList(); // Noncompliant {{Use 'AsEnumerable' here instead.}}
        //             ^^^^^^
    }

    public void GetEntities(DbSet<MyEntity> entities)
    {
        _ = entities.OrderBy(v => v.Id).ToList().Where(SomeTest).ToList(); // Noncompliant {{Use 'AsEnumerable' here instead.}}
        //                              ^^^^^^
        _ = entities.ToList().Where(SomeTest).ToList(); // Noncompliant {{Use 'AsEnumerable' here instead.}}
        //           ^^^^^^
    }

    public bool SomeTest(MyEntity entity)
    {
        return true;
    }
}
