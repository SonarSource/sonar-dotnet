using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Threading.Tasks;
using System;
using System.Linq;

public class EnitityFramework
{
    public async Task Query()
    {
        // Note to implementers: Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions and RelationalQueryableExtensions might be needed to be added to some sort of whitelist for IQueryables
        DbSet<object> dbSet = default;
        dbSet.Add(null); // Noncompliant
        dbSet.AddRange(null); // Noncompliant
        dbSet.All(x => true); // Noncompliant
        dbSet.Any(x => true); // Noncompliant
        dbSet.Average(x => 1); // Noncompliant
        dbSet.Contains(null); // Noncompliant
        dbSet.Count(); // Noncompliant
        dbSet.ElementAt(1); // Compliant: EF 8.0 only
        dbSet.ElementAtOrDefault(1); // Compliant: EF 8.0 only
        dbSet.ExecuteDelete(); // Noncompliant
        dbSet.ExecuteUpdate(x => x.SetProperty(x => x.ToString(), x => string.Empty)); // Noncompliant
        dbSet.Find(null); // Noncompliant
        dbSet.First(); // Noncompliant
        dbSet.FirstOrDefault(); // Noncompliant
        dbSet.Last(); // Noncompliant
        dbSet.LastOrDefault(); // Noncompliant
        dbSet.Load(); // Noncompliant
        dbSet.LongCount(); // Noncompliant
        dbSet.Max(); // Noncompliant
        dbSet.Min(); // Noncompliant
        dbSet.Single(); // Noncompliant
        dbSet.SingleOrDefault(); // Noncompliant
        dbSet.Sum(x => 0); // Noncompliant
        dbSet.ToArray(); // Noncompliant
        dbSet.ToDictionary(x => 0); // Noncompliant
        dbSet.ToList(); // Noncompliant
    }
}
