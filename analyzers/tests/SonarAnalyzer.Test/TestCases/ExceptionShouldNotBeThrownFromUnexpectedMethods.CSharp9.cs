using System;

namespace Tests.CustomType
{
    public class ModuleInitializerAttribute : Attribute { }

    public class Foo
    {
        [ModuleInitializer]
        internal static void M1()
        {
            throw new Exception(); // Compliant - the attribute is not in the System.Runtime.CompilerServices namespace
        }
    }
}

namespace Tests.Diagnostics
{
    using System.Runtime.CompilerServices;

    record Record1 : IDisposable
    {
        static Record1() => throw new NotImplementedException(); // Compliant (allowed exception)
        public void Dispose()
        {
            throw new NotImplementedException(); // Compliant (allowed exception)
        }
    }

    record Record2 : IDisposable
    {
        static Record2() => throw new Exception(); // Noncompliant {{Remove this 'throw' expression.}}
//                          ^^^^^^^^^^^^^^^^^^^^^

        [ModuleInitializer]
        internal static void M1()
        {
            throw new Exception(); // Noncompliant
        }

        [Obsolete]
        [System.Runtime.CompilerServices.ModuleInitializerAttribute]
        internal static void M2() => throw new Exception(); // Noncompliant

        [Obsolete]
        internal static void M3() => throw new Exception(); // Compliant - different attribute

        [Obsolete("This attribute has arguments.", true)]
        internal static void M4() => throw new Exception(); // Compliant - different attribute

        event EventHandler OnSomething
        {
            add
            {
                throw new Exception(); // Noncompliant
            }
            remove
            {
                throw new InvalidOperationException(); // Compliant - allowed exception
            }
        }

        event EventHandler OnSomeArrow
        {
            add => throw new Exception(); // Noncompliant
            remove => throw new InvalidOperationException(); // Compliant - allowed exception
        }

        public override string ToString()
        {
            throw new Exception(); // Noncompliant
        }

        public void Dispose()
        {
            throw new Exception(); // Noncompliant
        }
    }
}
