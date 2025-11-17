using System;

// Record types aren't appropriate to use as entities,
// as Entity Framework depends on reference equality.
// https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/types/records
record RecordEntity
{
    public DateTime Id { get; set; } // Compliant
}

class Entity { }

static class Extensions
{
    extension(Entity entity)
    {
        public DateTime Id { get => default; set { } }  // Compliant
    }
}

class FieldKeyWord
{
    public DateTime Id  // Noncompliant
    {
        get => field;
        set => field = value;
    }
}
