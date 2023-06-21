using Microsoft.EntityFrameworkCore;
using System;

// DateOnly and TimeOnly types are available in .NET 6+
class TemporalTypes
{
    class DateOnlyKey
    {
        public DateOnly Id { get; set; }            // Noncompliant
    }

    class TimeOnlyKey
    {
        public TimeOnly Id { get; set; }            // Noncompliant
    }
}

class ClassWithPrimaryKeyAttribute
{
    // The PrimaryKey attribute was introduced in Entity Framework 7.0.
    // While it's possible to create a key for a single property with this attribute, it's mainly created to create composite keys.
    [PrimaryKey(nameof(KeyProperty))]
    class PrimaryKeyWithSingleProperty
    {
        public DateTime KeyProperty { get; set; }   // FN - possible, but unlikely scenario
    }

    [PrimaryKey(nameof(DateProperty), nameof(IntProperty))]
    class PrimaryKeyWithMultipleProperties
    {
        public DateTime DateProperty { get; set; }  // Compliant - the rule will not raise warnings when a temporal type is part of a composite key
        public int IntProperty { get; set; }
    }
}
