using System;

class Entity
{
    public DateTime EntityId { get; set; }  // Compliant - the Entity Framework library is not referenced
}

class DateTimeKey
{
    public DateTime Id { get; set; }
}
