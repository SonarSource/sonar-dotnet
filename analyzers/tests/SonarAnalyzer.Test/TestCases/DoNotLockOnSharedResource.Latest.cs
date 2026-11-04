using System;
using System.Collections.Generic;
using System.Threading;

namespace Tests.Diagnostics
{
    public class LockOnThisOrType(string myStringField)
    {
        public void RawStringLiterals()
        {
            lock ("""foo""") // Noncompliant
            { }
        }

        void NewlinesInStringInterpolation()
        {
            string s = "test";
            lock ($"{s
                .ToUpper()}")
            { }
            // Noncompliant@-3
        }

        public void MyLockingMethod()
        {
            lock (myStringField) // Noncompliant
            { }
        }

        public void EscapeChar()
        {
            lock ("\e") // Noncompliant
            { }
        }
    }

    public class LockOnLockType
    {
        private Lock _lock;
        public void LockOnLock()
        {
            lock (_lock) // Compliant
            { }
        }
    }
}

namespace CSharp15
{
    using System.Runtime.CompilerServices;

    public class ExtensionIndexers
    {
        public void LockOnExtensionIndexer(Bag bag)
        {
            lock (bag["x"]) { } // Noncompliant
            lock (bag[0]) { }   // Noncompliant
        }
    }

    public class Bag { }

    public static class Extensions
    {
        extension(Bag bag)
        {
            public string this[string name] => name;
            public Type this[int index] => null;
        }
    }

    public class ClassUnions
    {
        public void LockOnClassUnion(StringOrType u)
        {
            lock (u) { }        // Compliant
            lock (u.Value) { }  // Compliant
        }
    }

    [Union]
    public class StringOrType : IUnion
    {
        public StringOrType(string value) => Value = value;
        public StringOrType(Type value) => Value = value;
        public object Value { get; }
    }
}
