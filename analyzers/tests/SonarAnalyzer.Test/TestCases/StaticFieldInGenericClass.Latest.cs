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

partial class PartialClass<T>
{
    static partial event EventHandler PartialEvent { add { } remove { } }   // FN NET-2783
}

class ExtensionProperties<T> { }

static class Extensions
{
    extension<T>(ExtensionProperties<T>)
    {
        static int Property => 0;   // FN NET-2785
    }
}

class FieldKeyword<T>
{
    static int Property { get => field; set => field = value; } // Noncompliant
}

namespace CSharp15
{
    public union GenericUnion<T>(T, string)
    {
        internal static string field; // Noncompliant NET-5033

        internal static T genericField; // Compliant, field type depends on the type parameter
    }

    public union GenericUnionWithProperty<T>(T, string)
    {
        static int Property { get; set; }       // Noncompliant NET-5033

        static T GenericProperty { get; set; }  // Compliant
    }

    public union GenericUnionWithNestedClass<T>(T, string)
    {
        class Nested
        {
            static int field;           // Noncompliant
            static T genericField;      // Compliant
        }
    }

    public union NonGenericUnion(int, string)
    {
        static int field;               // Compliant, the union is not generic
    }

    public union GenericUnionWithGenericBase<T>(T, string) : IComparable<T>
    {
        static int field;               // Compliant, base type depends on the type parameter
        public int CompareTo(T other) => 0;
    }

    public union GenericUnionWithNonGenericBase<T>(T, string) : IComparable<string>
    {
        static int field;               // Noncompliant
        public int CompareTo(string other) => 0;
    }
}

// https://github.com/SonarSource/sonar-dotnet/issues/9822
// https://sonarsource.atlassian.net/browse/NET-5200
namespace Repro_9822
{
    public interface IExporter<in TParameters>
    {
        System.Threading.Tasks.Task ExecuteExport(TParameters queryParameters, System.Threading.CancellationToken token);

        static abstract string ExportFilePrefix { get; }        // Noncompliant FP, static abstract members have no storage
        static abstract string ExportFileSuffix { get; set; }   // Noncompliant FP
    }
}
