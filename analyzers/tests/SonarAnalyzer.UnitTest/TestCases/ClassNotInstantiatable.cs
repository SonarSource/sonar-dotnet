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

    public class MyClassGeneric<T>
    {
        private MyClassGeneric()
        {

        }
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
