using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Tests.Diagnostics
{

    class OrderByRepeated
    {
        public void Test(DbSet<Sample> dbSet)
        {
            dbSet.OrderBy(s => s.Id).OrderBy(s => s.Name);                                      // Noncompliant
            dbSet.OrderBy(s => s.Id).ThenBy(s => s.Name);                                       // Compliant

            dbSet.OrderBy(s => s.Id).ThenBy(s => s.Name).OrderBy(s => s.Id);                    // Noncompliant
            dbSet.Where(s => s.Id > 0).OrderBy(s => s.Id).OrderBy(s => s.Name);                 // Noncompliant
            dbSet.Select(s => s).OrderBy(s => s.Id).OrderBy(s => s.Name);                       // Noncompliant
            dbSet.Select(s => new { s.Id, s.Name }).OrderBy(s => s.Id).OrderBy(s => s.Name);    // Noncompliant
            dbSet.OrderByDescending(s => s.Id).OrderByDescending(s => s.Name);                  // Noncompliant
            dbSet.OrderBy(s => s.Id).OrderByDescending(s => s.Name);                            // Noncompliant
            dbSet.OrderBy(s => s.Id)?.OrderBy(s => s.Name);                                     // Noncompliant
            dbSet.OrderBy(s => s.Id).ToList().OrderBy(s => s.Name);

            // An intermediate call that does not preserve IOrderedQueryable is a known FN: the second OrderBy still discards the first
            dbSet.OrderBy(s => s.Id).Where(s => s.Id > 0).OrderBy(s => s.Name);                 // FN
            dbSet.OrderBy(s => s.Id).Select(s => s).OrderBy(s => s.Name);                       // FN
            // Skip/Take depend on the ordering, so the second OrderBy is intentional (true negative)
            dbSet.OrderBy(s => s.Id).Skip(5).OrderBy(s => s.Name);                              // Compliant
            dbSet.OrderBy(s => s.Id).Take(10).OrderBy(s => s.Name);                             // Compliant
        }

        public void NotDbSet(IQueryable<Sample> queryable)
        {
            queryable.OrderBy(s => s.Id).OrderBy(s => s.Name); // Noncompliant
        }

        public void ThroughContext(SampleContext context)
        {
            context.Samples.OrderBy(s => s.Id).OrderBy(s => s.Name);        // Noncompliant
            context.Set<Sample>().OrderBy(s => s.Id).OrderBy(s => s.Name);  // Noncompliant
            context.Samples.OrderBy(s => s.Id).ThenBy(s => s.Name);         // Compliant
        }
    }

    public class Sample
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class SampleContext : DbContext
    {
        public DbSet<Sample> Samples { get; set; }
    }
}
