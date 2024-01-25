using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

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

    public class MyDbContext : Microsoft.EntityFrameworkCore.DbContext
    {
        public Microsoft.EntityFrameworkCore.DbSet<MyEntity> MyEntities { get; set; }
    }

    public void GetEntitiesFromEntityFrameworkCoreDbContext(MyDbContext dbContext)
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

    public void GetEntitiesFromEntityFrameworkCoreDbSet(Microsoft.EntityFrameworkCore.DbSet<MyEntity> entities)
    {
        _ = entities.OrderBy(v => v.Id).ToList().Where(SomeTest).ToList(); // Noncompliant {{Use 'AsEnumerable' here instead.}}
        //                              ^^^^^^
        _ = entities.ToList().Where(SomeTest).ToList(); // Noncompliant {{Use 'AsEnumerable' here instead.}}
        //           ^^^^^^
        _ = (from v in entities
             orderby v.Id
             select v).ToList().Where(SomeTest).ToList(); // Noncompliant {{Use 'AsEnumerable' here instead.}}
        //             ^^^^^^
    }

    public void GetEntitiesFromEntityFrameworkDbSet_TEntity(System.Data.Entity.DbSet<MyEntity> entities)
    {
        _ = entities.OrderBy(v => v.Id).ToList().Where(SomeTest).ToList(); // Noncompliant {{Use 'AsEnumerable' here instead.}}
        //                              ^^^^^^
        _ = entities.ToList().Where(SomeTest).ToList(); // Noncompliant {{Use 'AsEnumerable' here instead.}}
        //           ^^^^^^
        _ = (from v in entities
             orderby v.Id
             select v).ToList().Where(SomeTest).ToList(); // Noncompliant {{Use 'AsEnumerable' here instead.}}
        //             ^^^^^^
    }

    public void GetEntitiesFromEntityFrameworkDbSet(System.Data.Entity.DbSet entities)
    {
        _ = entities.Cast<MyEntity>().OrderBy(v => v.Id).ToList().Where(SomeTest).ToList(); // Noncompliant {{Use 'AsEnumerable' here instead.}}
        //                                               ^^^^^^
        _ = entities.Cast<MyEntity>().ToList().Where(SomeTest).ToList(); // Noncompliant {{Use 'AsEnumerable' here instead.}}
        //                            ^^^^^^
    }

    public void GetEntitiesFromEntityFrameworkObjectQuery_TEntity(System.Data.Entity.Core.Objects.ObjectQuery<MyEntity> entities)
    {
        _ = entities.OrderBy(v => v.Id).ToList().Where(SomeTest).ToList(); // Noncompliant {{Use 'AsEnumerable' here instead.}}
        //                              ^^^^^^
        _ = entities.ToList().Where(SomeTest).ToList(); // Noncompliant {{Use 'AsEnumerable' here instead.}}
        //           ^^^^^^
        _ = (from v in entities
             orderby v.Id
             select v).ToList().Where(SomeTest).ToList(); // Noncompliant {{Use 'AsEnumerable' here instead.}}
        //             ^^^^^^
    }

    public bool SomeTest(MyEntity entity)
    {
        return true;
    }
}
