using System;
using System.ComponentModel.DataAnnotations;

// DateOnly and TimeOnly types are available in .NET 6+
class TemporalTypes
{
    class DateOnlyKey
    {
        public DateOnly Id { get; set; }                // Noncompliant
    }

    class TimeOnlyKey
    {
        public TimeOnly Id { get; set; }                // Noncompliant
    }
}
