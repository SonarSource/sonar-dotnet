using System;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    public class LockOnThisOrType
    {
        private string myStringField;

        public void MyLockingMethod()
        {
            lock (this) // Noncompliant
//                ^^^^
            {
                // ...
            }

            lock (lockObj)
            {
                // ...
            }

            lock (typeof(LockOnThisOrType)) // Noncompliant
//                ^^^^^^^^^^^^^^^^^^^^^^^^
            {
                // ...
            }
            lock ((new LockOnThisOrType()).GetType()) // Noncompliant {{Lock on a dedicated object instance instead.}}
            {
                // ...
            }
            lock ("foo") // Noncompliant
            {
            }
            lock (myStringField) // Noncompliant
            {
            }
        }

        object lockObj = new object();
    }
}
