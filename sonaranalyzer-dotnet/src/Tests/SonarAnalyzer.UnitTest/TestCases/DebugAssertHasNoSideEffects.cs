using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Tests.Diagnostics
{
    class Program
    {
        public void Test()
        {
            Debug.Assert2(true);
            Debug.Assert(list.Contains("a"));
            Debug.Assert(list.Contains("a"));
            Debug.Assert(list.DoStuff());
            Debug.Assert(list.Destroy());

            Debug.Assert(list.Remove("a")); // Noncompliant {{Expressions used in 'Debug.Assert' should not produce side effects.}}
//                      ^^^^^^^^^^^^^^^^^^
            Debug.Assert(list?.Remove("a")); // Noncompliant
//                      ^^^^^^^^^^^^^^^^^^^
            Debug.Assert(list.RemoveMe()); // Noncompliant

            Debug.Assert(list.Delete()); // Noncompliant
            Debug.Assert(list.Put()); // Noncompliant
            Debug.Assert(list.Set()); // Noncompliant
            Debug.Assert(list.Add()); // Noncompliant
            Debug.Assert(list.Pop()); // Noncompliant
            Debug.Assert(list.Update()); // Noncompliant
            Debug.Assert(list.Retain()); // Noncompliant
            Debug.Assert(list.Insert()); // Noncompliant
            Debug.Assert(list.Push()); // Noncompliant
            Debug.Assert(list.Append()); // Noncompliant
            Debug.Assert(list.Clear()); // Noncompliant
            Debug.Assert(list.Dequeue()); // Noncompliant
            Debug.Assert(list.Enqueue()); // Noncompliant
            Debug.Assert(list.Dispose()); // Noncompliant
        }
    }
}
