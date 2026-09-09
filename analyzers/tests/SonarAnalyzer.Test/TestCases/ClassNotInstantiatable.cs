using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tests.Diagnostics
{
    public class Class0
    {
        public void M() { }
    }

    public class Class1 // Noncompliant {{This class can't be instantiated; make its constructor 'public'.}}
//               ^^^^^^
    {
        private Class1() { }
    }
    public sealed class Class1b // Noncompliant {{This class can't be instantiated; make at least one of its constructors 'public'.}}
    {
        private Class1b() { }
        private Class1b(int i) { }
        public void M() { }
    }

    public class Class2 // Compliant, suggested solution of S1118
    {
        private Class2() { }

        public static void M() { }
    }

    public class ClassWithOnlyConstants // Compliant, suggested solution of S1118 - Constants are static, even when they are private
    {
        private ClassWithOnlyConstants() { }

        private const int Value = 1;
    }

    // https://sonarsource.atlassian.net/browse/NET-4315
    public class ClassWithPrivateNestedType // Compliant - Only static members and nested types, none of which needs an instance of the containing type
    {
        private ClassWithPrivateNestedType() { }

        public static int Compute() => new Nested().GetValue();

        private sealed class Nested
        {
            public int GetValue() => GetHashCode();
        }
    }

    // https://sonarsource.atlassian.net/browse/NET-4315
    public class ClassWithOnlyNestedType // Compliant - Nested types are accessible without an instance of the containing type
    {
        private ClassWithOnlyNestedType() { }

        public class Nested { }
    }

    // The exemption applies to every nested type kind, not just nested classes
    public class ClassWithOnlyNestedStruct // Compliant
    {
        private ClassWithOnlyNestedStruct() { }

        public struct Nested { }
    }

    public class ClassWithOnlyNestedEnum // Compliant
    {
        private ClassWithOnlyNestedEnum() { }

        public enum Nested { A }
    }

    public class ClassWithOnlyNestedInterface // Compliant
    {
        private ClassWithOnlyNestedInterface() { }

        public interface INested { }
    }

    public class ClassWithOnlyNestedDelegate // Compliant
    {
        private ClassWithOnlyNestedDelegate() { }

        public delegate void NestedDelegate();
    }

    public class ClassWithOnlyInternalNestedType // Compliant - An internal nested type is reachable from the rest of the assembly
    {
        private ClassWithOnlyInternalNestedType() { }

        internal class Nested { }
    }

    public class ClassWithOnlyProtectedInternalNestedType // Compliant - 'protected internal' is also internal, so it is reachable from the rest of the assembly
    {
        private ClassWithOnlyProtectedInternalNestedType() { }

        protected internal class Nested { }
    }

    public class ClassWithOnlyProtectedNestedType // Noncompliant - All the constructors are private, so nothing outside can derive from the class and reach the nested type
    {
        private ClassWithOnlyProtectedNestedType() { }

        protected class Nested { }
    }

    public class ClassWithOnlyPrivateStaticNestedType // Compliant - A static nested type is a static member, even when it is private
    {
        private ClassWithOnlyPrivateStaticNestedType() { }

        private static class Nested { }
    }

    public class ClassWithPrivateAndPublicNestedTypes // Compliant - It is enough that one of the nested types is reachable from the outside
    {
        private ClassWithPrivateAndPublicNestedTypes() { }

        private struct Hidden { }

        public enum Visible { A }
    }

    public class ClassWithOnlyPrivateNestedType // Noncompliant - Neither the class nor its private nested type can be reached from the outside
    {
        private ClassWithOnlyPrivateNestedType() { }

        private class Nested { }
    }

    public class ClassWithOnlyPrivateNestedTypes // Noncompliant - Same as above, no matter the kind of the nested types
    {
        private ClassWithOnlyPrivateNestedTypes() { }

        private struct Hidden { }

        private enum AlsoHidden { A }
    }

    public class ClassWithPrivateNestedType2 // Noncompliant
    {
        private ClassWithPrivateNestedType2() { }

        public static int Compute() => new Nested().GetValue();

        public int InstanceCompute() => new Nested().GetValue();

        private sealed class Nested
        {
            public int GetValue() => GetHashCode();
        }
    }

    public sealed class Class3 // Compliant
    {
        private Class3() { }

        public void M() { }
        public static Class3 instance => new Class3();
    }

    public sealed class Class4 // Compliant
    {
        public void M() { }
    }

    public class Class6 // Compliant
    {
        private Class6() { }

        public void M() { } // Instance member, so the class is not exempted and the nested type inheriting from it is what makes it compliant

        public class Intermediate
        {
            public class Nested : Class6 // Noncompliant
            {
                private Nested()
                {

                }
            }
        }
    }

    public sealed class Class7 // Noncompliant
    {
        private Class7() { }

        public void M() { }
        public static Class0 instance => new Class0();
    }

    public class MyClassGeneric<T> // Compliant
    {
        private MyClassGeneric()
        {

        }
        public void M() { } // Instance member, so the class is not exempted and the nested type inheriting from it is what makes it compliant

        public class Nested : MyClassGeneric<int> { }
    }

    public class MyClassGeneric2<T>
    {
        private MyClassGeneric2()
        {

        }
        public object Create()
        {
            return new MyClassGeneric2<int>();
        }
    }

    public struct StructWithPrivateConstructor // Compliant - Only classes are checked, a struct is always instantiatable
    {
        private StructWithPrivateConstructor(int i) { }
    }

    public class MyAttribute : System.Attribute { }

    [My]
    public class WithAttribute1
    {
        private WithAttribute1()
        {
        }
    }

    public class WithAttribute2
    {
        [My]
        private WithAttribute2()
        {
        }
    }

    // https://github.com/SonarSource/sonar-dotnet/issues/3329
    public class Repro_3329 : System.Runtime.InteropServices.SafeHandle // Compliant, instance will be created by PInvoke of DllImport
    {
        private Repro_3329() : base(IntPtr.Zero, true) { }

        protected override bool ReleaseHandle() => true;
        public override bool IsInvalid => true;
    }
}
