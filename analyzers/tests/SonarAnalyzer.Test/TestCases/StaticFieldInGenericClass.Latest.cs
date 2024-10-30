using System.Collections.Generic;
using System;

namespace CSharp9
{
    record StaticFieldInGenericRecord<T>
    where T : class
    {
        internal static string field; //Noncompliant {{A static field in a generic type is not shared among instances of different close constructed types.}}

        public static string Prop1 { get; set; } // Noncompliant

        public string Prop2 { get; set; }

        public static T Prop3 { get; set; }
    }

    record StaticFieldInGenericPositionalRecord<T>(int Property)
        where T : class
    {
        internal static string field; //Noncompliant {{A static field in a generic type is not shared among instances of different close constructed types.}}

        public static string Prop1 { get; set; } // Noncompliant

        public string Prop2 { get; set; }

        public static T Prop3 { get; set; }
    }

    public interface IStatic<T>
    {
        public static string Field;                 // Noncompliant
        public static string Prop { get; set; }     // Noncompliant
        public static T Empty;                      // Compliant
    }
}

namespace CSharp10
{
    record struct StaticFieldInGenericRecordStruct<T>
        where T : class
    {
        public StaticFieldInGenericRecordStruct() { }

        internal static string field; // Noncompliant

        public static string Prop1 { get; set; } // Noncompliant

        public string Prop2 { get; set; } = "";

        public static T Prop3 { get; set; } = null;
    }

    record struct StaticFieldInGenericPositionalRecordStruct<T>(int Property)
        where T : class
    {
        public StaticFieldInGenericPositionalRecordStruct() : this(1) { }

        internal static string field; // Noncompliant

        public static string Prop1 { get; set; } // Noncompliant

        public string Prop2 { get; set; } = "";

        public static T Prop3 { get; set; } = null;
    }
}

// https://sonarsource.atlassian.net/browse/NET-425
namespace CSharp13
{
    public partial class LengthLimitedSingletonCollection<T> where T : new()
    {
        public static partial Dictionary<T, object> Instances => [];
        public static partial Dictionary<Type, object> Instances2 => new Dictionary<Type, object>();
    }

    public partial class LengthLimitedSingletonCollection<T> where T : new()
    {
        public static partial Dictionary<T, object> Instances { get; }     // Compliant
        public static partial Dictionary<Type, object> Instances2 { get; } // Noncompliant
    }
}
