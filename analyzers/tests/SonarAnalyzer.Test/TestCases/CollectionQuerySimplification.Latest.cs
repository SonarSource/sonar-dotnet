using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

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


namespace CSharp14
{
    public static class Extensions
    {
        extension<TSource>(IEnumerable<TSource> source)
        {
            public bool NonCompliantProp => !source.ToList().Any(); // Noncompliant
            public bool CompliantProp => !source.Any();

            public bool NonCompliantMethod() { return source.Select(x => x as object).Any(x => x != null); }    // Noncompliant
            public bool CompliantMethod() { return source.OfType<object>().Any(x => x != null); }
        }
    }

    public class FieldKeyword
    {
        public IEnumerable<string> NonCompliantProp
        {
            get { return field.ToList().Select(x => x.ToString()); }    // Noncompliant
            set { }
        }

        public IEnumerable<string> CompliantProp
        {
            get { return field.Select(x => x.ToString()); }
            set { }
        }
    }

    public class NullConditionalAssignment
    {
        public void Test(List<string> strings)
        {
            var x = new Tester();

            x?.collection = strings.Where(x => x is string).Any();  // Noncompliant
        }
        public class Tester
        {
            public bool collection;
        }
    }

    public class SimpleLambdaParameters
    {
        delegate bool OutTest<T>(string text, out T result);
        delegate bool InTest<T>(in T value);

        public void Test(List<string> source)
        {
            OutTest<int> outTest = (text, out result) => int.TryParse(text, out result);
            InTest<string> inTest = (in value) => value.Length > 42;

            var NoncompliantOut = source.Where(x => outTest(x, out var result)).Any();   // Noncompliant
            var CompliantOut = source.Any(x => outTest(x, out var result));

            var NoncompliantIn = source.Where(x => inTest(in x)).Any();   // Noncompliant
            var CompliantIn = source.Any(x => inTest(in x));
        }
    }
}
