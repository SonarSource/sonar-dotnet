using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.IO;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.Collections.Generic;

public class EntityFramework
{
    public async Task Query()
    {
        DbSet<object> dbSet = default;
        dbSet.Add(null);             // Compliant https://github.com/SonarSource/sonar-dotnet/issues/9269
        dbSet.AddRange(null);        // Compliant https://github.com/SonarSource/sonar-dotnet/issues/9269
        dbSet.All(x => true);        // Noncompliant
        dbSet.Any(x => true);        // Noncompliant
        dbSet.Average(x => 1);       // Noncompliant
        dbSet.Contains(null);        // Noncompliant
        dbSet.Count();               // Noncompliant
        dbSet.ElementAt(1);          // Compliant: EF 8.0 only
        dbSet.ElementAtOrDefault(1); // Compliant: EF 8.0 only
        dbSet.ExecuteDelete();       // Noncompliant
        dbSet.ExecuteUpdate(x => x.SetProperty(x => x.ToString(), x => string.Empty)); // Noncompliant
        dbSet.Find(null);            // Noncompliant
        dbSet.First();               // Noncompliant
        dbSet.FirstOrDefault();      // Noncompliant
        dbSet.Last();                // Noncompliant
        dbSet.LastOrDefault();       // Noncompliant
        dbSet.Load();                // Noncompliant
        dbSet.LongCount();           // Noncompliant
        dbSet.Max();                 // Noncompliant
        dbSet.Min();                 // Noncompliant
        dbSet.Single();              // Noncompliant
        dbSet.SingleOrDefault();     // Noncompliant
        dbSet.Sum(x => 0);           // Noncompliant
        dbSet.ToArray();             // Noncompliant
        dbSet.ToDictionary(x => 0);  // Noncompliant
        dbSet.ToList();              // Noncompliant
    }

    public async Task NotIQueryable(IEnumerable<object> enumerable)
    {
        enumerable.All(x => true); // Compliant not an IQueryable
        enumerable.ToArray();      // Compliant
        enumerable.ToList();       // Compliant
    }

    public async Task Context(DbContext dbContext)
    {
        dbContext.Add(null);            // Compliant https://github.com/SonarSource/sonar-dotnet/issues/9269
        dbContext.AddRange(null);       // Compliant https://github.com/SonarSource/sonar-dotnet/issues/9269
        dbContext.Dispose();            // Noncompliant
        dbContext.Find<object>();       // Noncompliant
        dbContext.Find(typeof(object)); // Noncompliant
        dbContext.SaveChanges();        // Noncompliant
    }

    public async Task DatabaseFacade(DatabaseFacade databaseFacade)
    {
        databaseFacade.BeginTransaction();     // Noncompliant
        databaseFacade.CanConnect();           // Noncompliant
        databaseFacade.CommitTransaction();    // Noncompliant
        databaseFacade.EnsureCreated();        // Noncompliant
        databaseFacade.EnsureDeleted();        // Noncompliant
        databaseFacade.RollbackTransaction();  // Noncompliant

        // Extension methods
        bool isEnabled = true;
        databaseFacade.BeginTransaction(System.Data.IsolationLevel.Chaos);                           // Noncompliant
        databaseFacade.CloseConnection();                                                            // Noncompliant
        databaseFacade.ExecuteSql($"Select * From Table Where IsEnabled = {isEnabled}");             // Noncompliant
        databaseFacade.ExecuteSqlInterpolated($"Select * From Table Where IsEnabled = {isEnabled}"); // Noncompliant
        databaseFacade.ExecuteSqlRaw("Select * From Table Where IsEnabled = @isEnabled", isEnabled); // Noncompliant
        databaseFacade.GetAppliedMigrations();                                                       // Noncompliant
        databaseFacade.GetPendingMigrations();                                                       // Noncompliant
        databaseFacade.Migrate();                                                                    // Noncompliant
        databaseFacade.OpenConnection();                                                             // Noncompliant
        databaseFacade.UseTransaction(null);                                                         // Noncompliant
    }
}

// https://github.com/SonarSource/sonar-dotnet/issues/9590
public class Repro_9590
{
    public async Task DoSomeWork(IDbContextFactory<AppDbContext> factory, MyDbContextFactory factory2, NotADbContextFactory factory3)
    {
        using AppDbContext dbContext = factory.CreateDbContext(); // Compliant - CreateDbContextAsync is excluded
        using AppDbContext dbContext2 = factory2.CreateDbContext(); // Compliant - CreateDbContextAsync is excluded
        using AppDbContext dbContext3 = factory3.CreateDbContext(); // Noncompliant
    }

    public class MyDbContextFactory : IDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext() => throw new NotImplementedException();

        public Task<AppDbContext> CreateDbContextAsync() => throw new NotImplementedException();
    }

    public class NotADbContextFactory
    {
        public AppDbContext CreateDbContext() => throw new NotImplementedException();

        public Task<AppDbContext> CreateDbContextAsync() => throw new NotImplementedException();
    }

    public class AppDbContext : DbContext
    {
    }
}
