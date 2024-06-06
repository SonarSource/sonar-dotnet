using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Tests.TestCases
{
    internal class C
    {
        private static Dictionary<string, List<int>> Dict;
    }

    class StaticFieldInGenericClass<T/*comment*/, /*comment*/U>
        where T : class
    {
        private static readonly ConditionalWeakTable<T, Task<T>>.CreateValueCallback s_taskCreationCallback = Task.FromResult<T>;

        private static Dictionary<string, List<T>> Dict, Dict4 = new Dictionary<string, List<T>>();
        private static Dictionary<string, T[]> Dict2;

        public static string sProp1 { get; set; } // Noncompliant
//                           ^^^^^^
        public /*comment */ static string sProp2 { get; set; } // Noncompliant {{A static field in a generic type is not shared among instances of different close constructed types.}}

        public static int ArrowProperty => 42; // Compliant, doesn't have a static backing field

        public string sProp3 { get; set; }

        public static T tProp { get; set; }

        internal static string sField; // Noncompliant

        internal string NestedClassUsage => NestedClass.StringField;

        // https://github.com/SonarSource/sonar-dotnet/issues/4081
        private static class NestedClass
        {
            public static readonly string StringField = "String"; // Noncompliant
        }

        private static Dictionary<string, string> _attributes = new Dictionary<string, string>(); // Noncompliant

        public static List<int> Numbers { get; } = new List<int>(); // Noncompliant

        public static string State => nameof(State); // Compliant

        public static object New => new object(); // Compliant always a new instance

        public static object Y { get { return new object(); } } // Noncompliant - FP, always a new instance

        public static object Z // Noncompliant
        {
            get { return new object(); }
            set { }
        }

        protected static Dictionary<string, string> Attributes // Noncompliant - FP? It can use only static fields (for which we already raise)
        {
            get {
                if (_attributes == null)
                    InitColumns();
                return _attributes;
            }
        }

        private static void InitColumns() { }
    }

    struct StaticFieldInGenericStruct<T, U>
    where T : class
    {
        private static Dictionary<string, T[]> Dict2;

        public static string sProp1 { get; set; } // Noncompliant
    }

    // https://github.com/SonarSource/sonar-dotnet/issues/5238
    public class Wrapper<T> where T : class
    {
        public static T EmptyProp { get; set; }
        public static readonly T Empty = CreateEmpty();
        private static T singleton;

        private static T CreateEmpty() => null;
    }
}

// https://github.com/SonarSource/sonar-dotnet/issues/7521
namespace Repro_7521
{
    public class GenericBase<T> { } // Might be out of our control
    public interface IGeneric<T> { }
    public interface IBoring { }

    public class GenericDerived<T> : GenericBase<T>, IBoring
    {
        private static readonly string[] field = new[] { "a", "b", "c" }; // Compliant, we might not be able to change the base class, or want the field there
    }

    public class ImplementsInterface : IBoring, IGeneric<int>
    {
        private static readonly string[] field = new[] { "a", "b", "c" }; // Compliant
    }

    public class ImplementsInterfaceGeneric<T> : IBoring, IGeneric<T>
    {
        private static readonly string[] field = new[] { "a", "b", "c" }; // Compliant, we might not be able to change the base class, or want the field there
    }

    public class ImplementsInterfaceGenericIrrelevantForBase<T> : IBoring, IGeneric<int>
    {
        private static readonly string[] field = new[] { "a", "b", "c" }; // Noncompliant
    }

    public class Outer<T>
    {
        public class Inner
        {
            private static readonly string[] field = new[] { "a", "b", "c" }; // Noncompliant
        }

        public class InnerGeneric : GenericBase<T>
        {
            private static readonly string[] field = new[] { "a", "b", "c" }; // Compliant, we might not be able to change the base class, or want the field there
        }
    }
}

