using System;
using System.Diagnostics;

namespace Tests.Diagnostics
{
    class Foo
    {
        public Foo Me() => null;

        public bool Contains(string arg) => true;
        public bool DoStuff() => true;
        public bool Destroy() => true;
        public bool Remove(string arg) => true;
        public bool RemoveMe() => true;
        public bool Delete() => true;
        public bool Put() => true;
        public bool Set() => true;
        public bool Add() => true;
        public bool Pop() => true;
        public bool Update() => true;
        public bool Retain() => true;
        public bool Insert() => true;
        public bool Push() => true;
        public bool Append() => true;
        public bool Clear() => true;
        public bool Dequeue() => true;
        public bool Enqueue() => true;
        public bool Dispose() => true;
        public bool CanAddItem() => true;
        public bool SetEquals() => true;
    }

    class Program
    {
        public void Test()
        {
            var foo = new Foo();

            Debug.Assert(true);
            Debug.Fail("message");
            Debug.Assert(foo.Contains("a"));
            Debug.Assert(foo.DoStuff());
            Debug.Assert(foo.Destroy());
            Debug.Assert(foo.CanAddItem());
            Debug.Assert(foo.SetEquals());

            Debug.Assert(foo.Remove("a")); // Noncompliant {{Expressions used in 'Debug.Assert' should not produce side effects.}}
//                      ^^^^^^^^^^^^^^^^^
            Debug.Assert((foo?.Remove("a")).Value); // Noncompliant
//                      ^^^^^^^^^^^^^^^^^^^^^^^^^^
            Debug.Assert(foo.RemoveMe()); // Noncompliant

            Debug.Assert(foo.Delete()); // Noncompliant
            Debug.Assert(foo.Put()); // Noncompliant
            Debug.Assert(foo.Set()); // Noncompliant
            Debug.Assert(foo.Add()); // Noncompliant
            Debug.Assert(foo.Pop()); // Noncompliant
            Debug.Assert(foo.Update()); // Noncompliant
            Debug.Assert(foo.Retain()); // Noncompliant
            Debug.Assert(foo.Insert()); // Noncompliant
            Debug.Assert(foo.Push()); // Noncompliant
            Debug.Assert(foo.Append()); // Noncompliant
            Debug.Assert(foo.Clear()); // Noncompliant
            Debug.Assert(foo.Dequeue()); // Noncompliant
            Debug.Assert(foo.Enqueue()); // Noncompliant
            Debug.Assert(foo.Dispose()); // Noncompliant

            Debug.Assert((foo.Me()?.Me().Clear()).Value); // Noncompliant

            var b = true;
            Debug.Assert(b = false);    // FN NET-2619
        }
    }
}
