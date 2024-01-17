using System;
using Microsoft.EntityFrameworkCore;

class FluentApi
{
    class PersonDbContext: DbContext
    {
        public DbSet<Person> People { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Person>()
                .HasKey(x => x.DateOfBirth);
        }
    }

    class Person
    {
        public DateTime DateOfBirth { get; set; }       // FN - keys created with the Fluent API are too complex to track
    }
}
